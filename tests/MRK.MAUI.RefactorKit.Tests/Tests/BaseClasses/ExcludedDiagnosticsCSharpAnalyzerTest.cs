using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MRK.MAUI.RefactorKit.Tests;

/// <summary>
/// The C# analyzer test that ignores the diagnostic with ids specified in the <see cref="ExcludedDiagnosticIds"/>
/// </summary>
/// <typeparam name="TAnalyzer">The type of the analyzer</typeparam>
internal abstract class ExcludedDiagnosticsCSharpAnalyzerTest<TAnalyzer> : MvvmCSharpAnalyzerTest<TAnalyzer>
	where TAnalyzer : DiagnosticAnalyzer, new()
{
	#region Protected Properties

	/// <summary>
	/// The diagnostic ids that will be excluded from the test
	/// </summary>
	protected abstract IEnumerable<string> ExcludedDiagnosticIds { get; }

	#endregion

	#region Protected Methods

	/// <summary>
	/// <inheritdoc/>
	/// </summary>
	/// <param name="diagnostics"><inheritdoc/></param>
	/// <returns></returns>
	protected sealed override ImmutableArray<(Project project, Diagnostic diagnostic)> FilterDiagnostics(ImmutableArray<(Project project, Diagnostic diagnostic)> diagnostics)
	{
		var newResults = diagnostics.RemoveAll(x => ExcludedDiagnosticIds.Contains(x.diagnostic.Id));

		return base.FilterDiagnostics(newResults);
	}

	#endregion
}
