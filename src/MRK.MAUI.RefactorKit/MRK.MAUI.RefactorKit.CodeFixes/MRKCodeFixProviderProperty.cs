using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MRK.MAUI.RefactorKit
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MRKCodeFixProviderProperty)), Shared]
	public class MRKCodeFixProviderProperty : CodeFixProvider
	{

		public sealed override ImmutableArray<string> FixableDiagnosticIds
			=> ImmutableArray.Create(MRKAnalyzerProperty.DiagnosticId);

		public sealed override FixAllProvider GetFixAllProvider()
			=> WellKnownFixAllProviders.BatchFixer;

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var diagnostic = context.Diagnostics.First();

			// Find the property identifier token, then get the containing property declaration
			var token = root.FindToken(diagnostic.Location.SourceSpan.Start);
			var node = token.Parent?.FirstAncestorOrSelf<PropertyDeclarationSyntax>();

			if (node == null)
			{
				return;
			}

			context.RegisterCodeFix(
				CodeAction.Create(
					CodeFixResources.PropertyFixTitle,
					ct => ConvertToObservableFieldAsync(context.Document, node, ct),
					nameof(MRKAnalyzerProperty)),
				diagnostic);
		}

		async Task<Document> ConvertToObservableFieldAsync(Document document, PropertyDeclarationSyntax propDecl, CancellationToken cancellationToken)
		{
			var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

			// Find the setter and extract OnPropertyChanged calls
			var notifyForNames = new List<string>();
			var propertyName = propDecl.Identifier.Text;

			var setter = propDecl.AccessorList?.Accessors.FirstOrDefault(a => a.Kind() == SyntaxKind.SetAccessorDeclaration);

			string backingFieldName = string.Empty;

			if (setter != null && setter.Body != null)
			{
				foreach (var statement in setter.Body.Statements)
				{
					// Try to find assignment to a field (e.g., _canExecuteCommand = value;)
					if (statement is ExpressionStatementSyntax exprStmt &&
						exprStmt.Expression is AssignmentExpressionSyntax assignExpr &&
						assignExpr.Left is IdentifierNameSyntax leftId)
					{
						backingFieldName = leftId.Identifier.Text;
					}
					else if (statement is ExpressionStatementSyntax exprStmt2 &&
							exprStmt2.Expression is InvocationExpressionSyntax invocation2 &&
							invocation2.Expression is IdentifierNameSyntax idName2 &&
							idName2.Identifier.Text == MRKAnalyzerProperty.SetProperty &&
							invocation2.ArgumentList.Arguments.Count > 0)
					{
						var firstArg = invocation2.ArgumentList.Arguments[0].Expression;

						if (firstArg is PrefixUnaryExpressionSyntax prefixUnary &&
							prefixUnary.Operand is IdentifierNameSyntax refId)
						{
							backingFieldName = refId.Identifier.Text;
						}
						else if (firstArg is IdentifierNameSyntax refIdDirect)
						{
							backingFieldName = refIdDirect.Identifier.Text;
						}
					}

					if (statement is ExpressionStatementSyntax exprStmt3 &&
						exprStmt3.Expression is InvocationExpressionSyntax invocation &&
						invocation.Expression is IdentifierNameSyntax idName &&
						idName.Identifier.Text == MRKAnalyzerProperty.OnPropertyChanged)
					{
						if (invocation.ArgumentList.Arguments.Count == 1)
						{
							var arg = invocation.ArgumentList.Arguments[0].Expression;

							if (arg is InvocationExpressionSyntax nameofInvoke &&
								nameofInvoke.Expression is IdentifierNameSyntax nameofId &&
								nameofId.Identifier.Text == "nameof" &&
								nameofInvoke.ArgumentList.Arguments.Count == 1)
							{
								var nameofArg = nameofInvoke.ArgumentList.Arguments[0].Expression.ToString();
								if (!string.Equals(nameofArg, propertyName, System.StringComparison.Ordinal))
								{
									notifyForNames.Add(nameofArg);
								}
							}
							else if (arg is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression))
							{
								if (!string.Equals(literal.Token.ValueText, propertyName, System.StringComparison.Ordinal))
								{
									notifyForNames.Add(literal.Token.ValueText);
								}
							}
						}
					}
				}
			}

			EqualsValueClauseSyntax initializer = null;

			// Try to get initializer from the backing field
			FieldDeclarationSyntax fieldToRemove = null;
			if (!string.IsNullOrEmpty(backingFieldName))
			{
				var classDecl = propDecl.Parent as ClassDeclarationSyntax;

				if (classDecl != null)
				{
					fieldToRemove = classDecl.Members
						.OfType<FieldDeclarationSyntax>()
						.FirstOrDefault(f =>
							f.Declaration.Variables.Any(v => v.Identifier.Text == backingFieldName));

					if (fieldToRemove != null)
					{
						var variable = fieldToRemove.Declaration.Variables
							.FirstOrDefault(v => v.Identifier.Text == backingFieldName);
						if (variable?.Initializer != null)
						{
							initializer = variable.Initializer;
						}
					}
				}
			}

			// If the property itself has an initializer, prefer that
			if (propDecl.Initializer != null)
			{
				initializer = propDecl.Initializer;
			}

			var variableName = propDecl.Identifier.Text;
			var fieldName = char.ToUpperInvariant(variableName[0]) + variableName.Substring(1);

			// Build attribute lists
			var attributes = new List<AttributeListSyntax>
									{
										SyntaxFactory.AttributeList(
											SyntaxFactory.SingletonSeparatedList(
												SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("ObservableProperty"))))
									};

			foreach (var name in notifyForNames.Distinct())
			{
				attributes.Add(
					SyntaxFactory.AttributeList(
						SyntaxFactory.SingletonSeparatedList(
							SyntaxFactory.Attribute(
								SyntaxFactory.IdentifierName("NotifyPropertyChangedFor"),
								SyntaxFactory.AttributeArgumentList(
									SyntaxFactory.SingletonSeparatedList(
										SyntaxFactory.AttributeArgument(
											SyntaxFactory.ParseExpression($"nameof({name})")
										)
									)
								)
							)
						)
					)
				);
			}

			var newProperty = SyntaxFactory.PropertyDeclaration(
								propDecl.Type,
								SyntaxFactory.Identifier(fieldName))
							.AddModifiers(
								SyntaxFactory.Token(SyntaxKind.PublicKeyword),
								SyntaxFactory.Token(SyntaxKind.PartialKeyword))
							.WithAttributeLists(SyntaxFactory.List(attributes))
							.WithAccessorList(
								SyntaxFactory.AccessorList(
									SyntaxFactory.List(new[]
									{
											SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
												.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
											SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
												.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
									})
								)
							);

			// If there is an initializer, add it to the property
			if (initializer != null)
			{
				newProperty = newProperty
					.WithInitializer(initializer)
					.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
			}

			// Get the leading trivia from the original property
			var leadingTrivia = propDecl.GetLeadingTrivia();

			// Normalize to a single blank line (one newline)
			var normalizedTrivia = SyntaxFactory.TriviaList(
				SyntaxFactory.ElasticCarriageReturnLineFeed
			);

			// If the original property had comments, preserve them
			var comments = leadingTrivia.Where(t => t.IsKind(SyntaxKind.SingleLineCommentTrivia) || t.IsKind(SyntaxKind.MultiLineCommentTrivia)).ToList();

			if (comments.Count > 0)
			{
				normalizedTrivia = normalizedTrivia.InsertRange(0, comments);
			}

			// Apply the trivia to the new property
			newProperty = newProperty.WithLeadingTrivia(normalizedTrivia);

			// Insert the new field before the property, then remove the property
			editor.InsertBefore(propDecl, newProperty);
			editor.RemoveNode(propDecl);

			// Remove the backing field if it exists

			if (!string.IsNullOrEmpty(backingFieldName))
			{
				var classDecl = propDecl.Parent as ClassDeclarationSyntax;

				if (classDecl != null)
				{
					if (fieldToRemove != null)
					{
						editor.RemoveNode(fieldToRemove);
					}
				}
			}

			var root = await document.GetSyntaxRootAsync();

			var compilationUnit = root as CompilationUnitSyntax;

			var importantUsing = SyntaxFactory.UsingDirective(
					SyntaxFactory.ParseName("CommunityToolkit.Mvvm.ComponentModel"));

			if (!compilationUnit.Usings.Any(u => u.Name.ToString().Contains("CommunityToolkit.Mvvm.ComponentModel")))
			{
				var ac = editor.OriginalRoot as CompilationUnitSyntax;
				var newRoot = ac.AddUsings(importantUsing);
				editor.ReplaceNode(root, newRoot);
			}

			return editor.GetChangedDocument();
		}
	}
}