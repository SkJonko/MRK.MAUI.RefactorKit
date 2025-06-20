using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MRK.MAUI.RefactorKit
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class MRKAnalyzerDelegateCommand : DiagnosticAnalyzer
	{
		public const string DiagnosticId = "MRK0002";

		private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.DelegateCommandAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
		private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.DelegateCommandAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
		private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.DelegateCommandAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
		private const string Category = "Refactoring";

		private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId,
																			 Title,
																			 MessageFormat,
																			 Category,
																			 DiagnosticSeverity.Error,
																			 isEnabledByDefault: true,
																			 description: Description,
																			 helpLinkUri: "https://github.com/SkJonko/MRK.MAUI.RefactorKit/blob/main/docs/rules/MRK0002.md");

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();
			context.RegisterSyntaxNodeAction(AnalyzeProperty, SyntaxKind.PropertyDeclaration);
		}

		private static void AnalyzeProperty(SyntaxNodeAnalysisContext context)
		{
			var propDecl = (PropertyDeclarationSyntax)context.Node;
			var type = context.SemanticModel.GetTypeInfo(propDecl.Type).Type;
			
			if (type == null)
			{
				return;
			}

			if (type.Name == "DelegateCommand")
			{
				var diagnostic = Diagnostic.Create(Rule, propDecl.Identifier.GetLocation(), propDecl.Identifier.Text);
				context.ReportDiagnostic(diagnostic);
			}
		}
	}
}
