using System.Collections.Immutable;

using Microsoft.CodeAnalysis;

using MRK.MAUI.RefactorKit.Tests;

namespace MRK.MAUI.RefactorKit.CodeFixes.Tests;

/// <summary>
/// The C# analyzer test that ignores the diagnostic with the <see cref="TestConstants.PartialPropertyDiagnosticId"/>
/// </summary>
internal sealed class PartialPropertyCSharpCodeFixTest : ObservablePropertyCSharpCodeFixTest
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