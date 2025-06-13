using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MRK.MAUI.RefactorKit
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MRKAnalyzerProperty : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "MRK0001";
        public const string OnPropertyChanged = "OnPropertyChanged";
        public const string SetProperty = "SetProperty";

        static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.PropertyAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.PropertyAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.PropertyAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        const string Category = "Naming";

        static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId,
                                                                                     Title,
                                                                                     MessageFormat,
                                                                                     Category,
                                                                                     DiagnosticSeverity.Error,
                                                                                     isEnabledByDefault: true,
                                                                                     description: Description,
                                                                                     helpLinkUri: "https://github.com/SkJonko/MRK.MAUI.RefactorKit/blob/main/docs/rules/MRK0001.md");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeProperty, SymbolKind.Property);
        }

        static void AnalyzeProperty(SymbolAnalysisContext context)
        {
            var propertySymbol = (IPropertySymbol)context.Symbol;

            var syntaxRefs = propertySymbol.DeclaringSyntaxReferences;
            foreach (var syntaxRef in syntaxRefs)
            {
                var node = syntaxRef.GetSyntax(context.CancellationToken) as PropertyDeclarationSyntax;

                if (node != null && node.AccessorList != null)
                {
                    var setter = node.AccessorList.Accessors.FirstOrDefault(a => a.Kind() == SyntaxKind.SetAccessorDeclaration);
                    if (setter != null && setter.Body != null)
                    {
                        // Look for invocation of OnPropertyChanged or SetProperty in the setter body
                        var containsNotify = setter.Body.Statements
                            .OfType<ExpressionStatementSyntax>()
                            .Select(s => s.Expression)
                            .OfType<InvocationExpressionSyntax>()
                            .Any(invocation =>
                            {
                                var expr = invocation.Expression as IdentifierNameSyntax;

                                // Check if the identifier is OnPropertyChanged or SetProperty
                                return expr != null &&
                                    (expr.Identifier.Text == OnPropertyChanged || expr.Identifier.Text == SetProperty);
                            });

                        if (containsNotify)
                        {
                            var diagnostic = Diagnostic.Create(Rule, node.Identifier.GetLocation(), propertySymbol.Name);
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                }
            }
        }
    }
}
