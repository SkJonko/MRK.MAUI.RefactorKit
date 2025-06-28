using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using NSubstitute;

using Xunit;

namespace MRK.MAUI.RefactorKit.Tests;

/// <summary>
/// Contains the tests regarding the <see cref="MRKAnalyzerProperty"/>
/// </summary>
public sealed class MRKAnalyzerPropertyTests : BaseDiagnosticAnalyzerTests<MRKAnalyzerProperty>
{
	#region Constructors

	/// <summary>
	/// Default constructor
	/// </summary>
	public MRKAnalyzerPropertyTests() : base()
	{

	}

	#endregion

	#region Protected Methods

	/// <summary>
	/// <inheritdoc/>
	/// </summary>
	protected sealed override void AssertNoExceptionThrownInRegisterAction(AnalysisContext analysisContext)
	{
		ArgumentNullException.ThrowIfNull(analysisContext);

		var validSymbolAction = Arg.Any<Action<SymbolAnalysisContext>>();

		var validSymbolKinds = Arg.Is<ImmutableArray<SymbolKind>>(x => IsValidPropertySymbolArgument(x));

		AssertNoExceptionThrown(() => analysisContext.Received(1).RegisterSymbolAction(validSymbolAction, validSymbolKinds));
	}

	#endregion

	#region Private Methods

	/// <summary>
	/// Checks whether the specified <paramref name="symbolKinds"/> is a valid argument
	/// </summary>
	/// <param name="symbolKinds">The kinds of symbol</param>
	/// <returns></returns>
	private static bool IsValidPropertySymbolArgument(ImmutableArray<SymbolKind> symbolKinds)
	{
		if (symbolKinds.Length != 1)
		{
			return false;
		}

		if (symbolKinds[0] != SymbolKind.Property)
		{
			return false;
		}

		return true;
	}

	#endregion
}
