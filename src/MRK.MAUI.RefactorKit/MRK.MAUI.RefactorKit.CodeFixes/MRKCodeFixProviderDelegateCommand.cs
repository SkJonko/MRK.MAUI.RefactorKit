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
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MRKCodeFixProviderDelegateCommand)), Shared]
	public class MRKCodeFixProviderDelegateCommand : CodeFixProvider
	{
		public sealed override ImmutableArray<string> FixableDiagnosticIds
			=> ImmutableArray.Create(MRKAnalyzerDelegateCommand.DiagnosticId);

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

			context.RegisterCodeFix(
				CodeAction.Create(
					CodeFixResources.DelegateCommandFixTitle,
					c => ConvertToRelayCommandAsync(context.Document, propertyDecl, c),
					nameof(MRKAnalyzerDelegateCommand)),
				diagnostic);
		}

		async Task<Document> ConvertToRelayCommandAsync(Document document, PropertyDeclarationSyntax propertyDecl, CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
			var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

			var classDecl = propertyDecl.Parent as ClassDeclarationSyntax;
			if (classDecl == null)
			{
				return document;
			}

			var fieldName = GetBackingFieldName(propertyDecl);
			var fieldDecl = classDecl.Members
				.OfType<FieldDeclarationSyntax>()
				.FirstOrDefault(f => f.Declaration.Variables.Any(v => v.Identifier.Text == fieldName));

			var executeMethodName = GetExecuteMethodName(propertyDecl);
			var canExecuteMethodName = GetCanExecuteMethodName(propertyDecl);

			var executeMethod = classDecl.Members
				.OfType<MethodDeclarationSyntax>()
				.FirstOrDefault(m => m.Identifier.Text == executeMethodName);

			var canExecuteMethod = classDecl.Members
				.OfType<MethodDeclarationSyntax>()
				.FirstOrDefault(m => m.Identifier.Text == canExecuteMethodName);

			// Remove property and field
			editor.RemoveNode(propertyDecl);

			if (fieldDecl != null)
			{
				editor.RemoveNode(fieldDecl);
			}

			// Determine CanExecute target
			string canExecuteTarget = null;
			bool canRemoveCanExecuteMethod = false;
			if (canExecuteMethod != null)
			{
				// Check if the method is a simple wrapper: "return CanExecuteCommand;"
				if (canExecuteMethod.Body != null &&
					canExecuteMethod.Body.Statements.Count == 1 &&
					canExecuteMethod.Body.Statements[0] is ReturnStatementSyntax returnStmt &&
					returnStmt.Expression is IdentifierNameSyntax idName)
				{
					canExecuteTarget = idName.Identifier.Text;
					canRemoveCanExecuteMethod = true;
				}
				else
				{
					// fallback: use the method name
					canExecuteTarget = canExecuteMethod.Identifier.Text;
				}
			}

			// Add [RelayCommand(CanExecute = nameof(...))] to execute method
			if (executeMethod != null)
			{
				// If async, add Async suffix
				var isAsync = executeMethod.Modifiers.Any(m => m.IsKind(SyntaxKind.AsyncKeyword));
				var newMethodName = propertyDecl.Identifier.Text.Replace("Command", "") + (isAsync ? "Async" : "");
				var attributeArgs = !string.IsNullOrEmpty(canExecuteTarget)
					? SyntaxFactory.ParseAttributeArgumentList($"(CanExecute = nameof({canExecuteTarget}))")
					: SyntaxFactory.ParseAttributeArgumentList("");

				var relayCommandAttr = SyntaxFactory.Attribute(
					SyntaxFactory.IdentifierName("RelayCommand"),
					attributeArgs);

				var newAttrList = executeMethod.AttributeLists.Add(
					SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(relayCommandAttr)));

				var newMethod = executeMethod
					.WithAttributeLists(newAttrList)
					.WithIdentifier(SyntaxFactory.Identifier(newMethodName));

				editor.ReplaceNode(executeMethod, newMethod);
			}

			// Remove CanExecute method if it is a simple wrapper
			if (canRemoveCanExecuteMethod && canExecuteMethod != null)
			{
				editor.RemoveNode(canExecuteMethod);
			}

			return editor.GetChangedDocument();
		}

		#region Private

		string GetBackingFieldName(PropertyDeclarationSyntax propertyDecl)
		{
			// TODO: Rething this logic to handle different naming conventions.
			var name = propertyDecl.Identifier.Text;
			if (name.EndsWith("Command"))
			{
				name = name.Substring(0, name.Length - "Command".Length);
			}

			return $"_{char.ToLowerInvariant(name[0])}{name.Substring(1)}Command";
		}

		string GetExecuteMethodName(PropertyDeclarationSyntax propertyDecl)
			=> $"Execute{propertyDecl.Identifier.Text}";

		string GetCanExecuteMethodName(PropertyDeclarationSyntax propertyDecl)
			=> $"CanExecute{propertyDecl.Identifier.Text}";

		#endregion
	}
}