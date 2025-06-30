using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MRK.MAUI.RefactorKit.Tests;

/// <summary>
/// The C# analyzer test that ignores the diagnostic with the <see cref="TestConstants.PartialPropertyDiagnosticId"/>
/// </summary>
/// <typeparam name="TAnalyzer">The type of the analyzer</typeparam>
internal sealed class PartialPropertyCSharpAnalyzerTest<TAnalyzer> : ObservablePropertyCSharpAnalyzerTest<TAnalyzer>
	where TAnalyzer : DiagnosticAnalyzer, new()
{
	#region Protected Methods

	/// <summary>
	/// <inheritdoc/>
	/// </summary>
	/// <param name="diagnostics"><inheritdoc/></param>
	/// <returns></returns>
	protected override ImmutableArray<(Project project, Diagnostic diagnostic)> FilterDiagnostics(ImmutableArray<(Project project, Diagnostic diagnostic)> diagnostics)
	{
		var newResults = diagnostics.RemoveAll(x => x.diagnostic.Id == TestConstants.PartialPropertyDiagnosticId);

		return base.FilterDiagnostics(newResults);
	}

	#endregion
}
