using System.Collections.Immutable;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using NSubstitute;

namespace MRK.MAUI.RefactorKit.Tests;

/// <summary>
/// The base class for the tests regarding a syntax <see cref="DiagnosticAnalyzer"/>
/// </summary>
/// <typeparam name="TDiagnosticAnalyzer"></typeparam>
public abstract class BaseSyntaxDiagnosticAnalyzerTests<TDiagnosticAnalyzer> : BaseDiagnosticAnalyzerTests<TDiagnosticAnalyzer>
	where TDiagnosticAnalyzer : DiagnosticAnalyzer, new()
{
	#region Protected Methods

	/// <summary>
	/// <inheritdoc/>
	/// </summary>
	/// <exception cref="NotImplementedException"></exception>
	protected sealed override void AssertNoExceptionThrownInRegisterAction(AnalysisContext analysisContext)
	{
		ArgumentNullException.ThrowIfNull(analysisContext);

		var validSyntaxNodeAction = Arg.Any<Action<SyntaxNodeAnalysisContext>>();

		var validSymbolKinds = Arg.Is<ImmutableArray<SyntaxKind>>(x => IsValidPropertyDeclarationSyntaxArgument(x));

		Assert.True(AssertNoExceptionThrown(() => analysisContext.Received(1).RegisterSyntaxNodeAction(validSyntaxNodeAction, validSymbolKinds)));
	}

	#endregion

	#region Private Methods

	/// <summary>
	/// Checks whether the specified <paramref name="symbolKinds"/> is a valid argument
	/// </summary>
	/// <param name="symbolKinds">The kinds of syntax</param>
	/// <returns></returns>
	private static bool IsValidPropertyDeclarationSyntaxArgument(ImmutableArray<SyntaxKind> symbolKinds)
	{
		if (symbolKinds.Length != 1)
		{
			return false;
		}

		if (symbolKinds[0] != SyntaxKind.PropertyDeclaration)
		{
			return false;
		}

		return true;
	}

	#endregion
}
