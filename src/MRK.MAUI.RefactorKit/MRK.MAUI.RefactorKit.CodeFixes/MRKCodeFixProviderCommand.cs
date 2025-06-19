using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace MRK.MAUI.RefactorKit
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MRKCodeFixProviderCommand)), Shared]
    public class MRKCodeFixProviderCommand : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(MRKAnalyzerCommand.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider()
            => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the property declaration identified by the diagnostic.
            var propertyDecl = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf()
                .OfType<PropertyDeclarationSyntax>().FirstOrDefault();

            if (propertyDecl == null)
            {
                return;
            }

            // Register the code fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: CodeFixResources.DelegateCommandFixTitle,
                    createChangedSolution: c => ConvertToRelayCommandAsync(context.Document, propertyDecl, c),
                    equivalenceKey: nameof(MRKAnalyzerDelegateCommand)),
                diagnostic);
        }

        /// <summary>
        /// Transforms the old Command property into a new method with the [RelayCommand] attribute.
        /// </summary>
        private async Task<Solution> ConvertToRelayCommandAsync(Document document, PropertyDeclarationSyntax propDecl, CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken);
            var classDecl = propDecl.Parent as ClassDeclarationSyntax;

            if (classDecl == null)
            {
                return document.Project.Solution;
            }

            // 1. Find the backing field for the property (e.g., _testCommand)
            var backingField = FindBackingField(classDecl, propDecl, semanticModel);

            // 2. Extract the logic and parameters from the Command's constructor lambda.
            var lambdaExpression = propDecl.DescendantNodes().OfType<LambdaExpressionSyntax>().FirstOrDefault();
            if (lambdaExpression == null)
            {
                return document.Project.Solution; // Could not find lambda expression.
            }

            // Extract the body of the lambda.
            ExpressionSyntax commandLogicExpression = null;
            if (lambdaExpression.Body is ExpressionSyntax expressionBody)
            {
                commandLogicExpression = expressionBody;
            }
            else if (lambdaExpression.Body is BlockSyntax blockBody)
            {
                var statement = blockBody.Statements.FirstOrDefault() as ExpressionStatementSyntax;
                commandLogicExpression = statement?.Expression;
            }

            if (commandLogicExpression == null)
            {
                return document.Project.Solution; // Cannot find the command's core logic expression.
            }

            SeparatedSyntaxList<ParameterSyntax> parameters = default;
            if (lambdaExpression is ParenthesizedLambdaExpressionSyntax pLambda)
            {
                parameters = pLambda.ParameterList.Parameters;
            }
            else if (lambdaExpression is SimpleLambdaExpressionSyntax sLambda)
            {
                parameters = SyntaxFactory.SingletonSeparatedList(sLambda.Parameter);
            }

            // Determine if the original lambda was async
            bool isAsync = lambdaExpression.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword);

            // 3. Create the new method declaration, now with parameters.
            var newMethodName = GetNewMethodName(propDecl.Identifier.Text, isAsync);
            var newMethod = CreateRelayCommandMethod(newMethodName, commandLogicExpression, isAsync, parameters);

            // 4. Add the [RelayCommand] attribute to the new method.
            var relayCommandAttribute = SyntaxFactory.Attribute(SyntaxFactory.ParseName("RelayCommand"));
            var attributeList = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(relayCommandAttribute));
            newMethod = newMethod.AddAttributeLists(attributeList);

            // 5. Replace the old property and backing field with the new method.
            editor.InsertBefore(propDecl, new[] { newMethod });
            editor.RemoveNode(propDecl);

            if (backingField != null)
            {
                editor.RemoveNode(backingField);
            }

            return editor.GetChangedDocument().Project.Solution;
        }

        /// <summary>
        /// Finds the private field that is used as a backing field for the command property.
        /// </summary>
        private FieldDeclarationSyntax FindBackingField(ClassDeclarationSyntax classDecl, PropertyDeclarationSyntax propDecl, SemanticModel semanticModel)
        {
            ExpressionSyntax expression = null;

            // Case 1: Expression-bodied property: public Command MyCommand => ...
            if (propDecl.ExpressionBody != null)
            {
                expression = propDecl.ExpressionBody.Expression;
            }
            // Case 2: Property with a getter: public Command MyCommand { get { ... } }
            else
            {
                var getter = propDecl.AccessorList?.Accessors.FirstOrDefault(a => a.IsKind(SyntaxKind.GetAccessorDeclaration));
                if (getter != null)
                {
                    // Subcase 2a: Expression-bodied getter: get => ...
                    if (getter.ExpressionBody != null)
                    {
                        expression = getter.ExpressionBody.Expression;
                    }
                    // Subcase 2b: Standard getter with a body: get { return ...; }
                    else if (getter.Body != null)
                    {
                        var returnStatement = getter.Body.Statements.OfType<ReturnStatementSyntax>().FirstOrDefault();
                        expression = returnStatement?.Expression;
                    }
                }
            }

            if (!(expression is BinaryExpressionSyntax coalesce) || !coalesce.IsKind(SyntaxKind.CoalesceExpression))
            {
                return null;
            }

            var fieldIdentifier = (coalesce.Left as IdentifierNameSyntax)?.Identifier.Text;
            if (string.IsNullOrEmpty(fieldIdentifier))
            {
                return null;
            }

            // Find the corresponding field declaration in the class.
            return classDecl.Members.OfType<FieldDeclarationSyntax>()
                .FirstOrDefault(f => f.Declaration.Variables.Any(v => v.Identifier.Text == fieldIdentifier));
        }

        /// <summary>
        /// Derives the new method name from the old property name.
        /// </summary>
        private string GetNewMethodName(string propertyName, bool isAsync)
        {
            if (propertyName.EndsWith("Command", StringComparison.OrdinalIgnoreCase))
            {
                propertyName = propertyName.Substring(0, propertyName.Length - "Command".Length);
            }

            if (isAsync)
            {
                return propertyName + "Async";
            }
            return propertyName;
        }

        /// <summary>
        /// Creates the new method syntax that will replace the command property.
        /// It now accepts parameters to add to the method signature.
        /// </summary>
        private MethodDeclarationSyntax CreateRelayCommandMethod(string methodName, ExpressionSyntax body, bool isAsync, SeparatedSyntaxList<ParameterSyntax> parameters)
        {
            TypeSyntax returnType;
            if (isAsync)
            {
                returnType = SyntaxFactory.ParseTypeName("Task");
            }
            else
            {
                returnType = SyntaxFactory.ParseTypeName("void");
            }

            var methodDeclaration = SyntaxFactory.MethodDeclaration(returnType, methodName)
                                                 .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));

            // Add parameters to the method if they exist.
            if (parameters.Any())
            {
                methodDeclaration = methodDeclaration.AddParameterListParameters(parameters.ToArray());
            }

            // If the original command was async, preserve the async modifier.
            if (isAsync)
            {
                methodDeclaration = methodDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.AsyncKeyword));
            }

            // Create the method body.
            var expressionStatement = SyntaxFactory.ExpressionStatement(body);
            var block = SyntaxFactory.Block(expressionStatement);
            methodDeclaration = methodDeclaration.WithBody(block);

            return methodDeclaration;
        }
    }
}