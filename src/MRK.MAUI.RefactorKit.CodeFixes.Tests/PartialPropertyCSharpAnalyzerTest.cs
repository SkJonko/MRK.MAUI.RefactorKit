using System.Collections.Immutable;

using Microsoft.CodeAnalysis;

namespace MRK.MAUI.RefactorKit.CodeFixes.Tests;

/// <summary>
/// The C# analyzer test that ignores the diagnostic with the <see cref="PartialPropertyDiagnosticId"/>
/// </summary>
internal sealed class PartialPropertyCSharpAnalyzerTest : ObservablePropertyCSharpAnalyzerTest
{
	#region Constants

	/// <summary>
	/// The id for the C# 13 and before Partial Property diagnostic
	/// CS0267: The 'partial' modifier can only appear immediately before 'class', 'record', 'struct', 'interface'.
	/// Before C# 13, partial wasn't allowed on properties or indexers.
	/// More info:https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/partial-declarations#partial-types
	/// </summary>
	public const string PartialPropertyDiagnosticId = "CS0267";

	#endregion

	#region Protected Methods

	/// <summary>
	/// <inheritdoc/>
	/// </summary>
	/// <param name="diagnostics"><inheritdoc/></param>
	/// <returns></returns>
	protected override ImmutableArray<(Project project, Diagnostic diagnostic)> FilterDiagnostics(ImmutableArray<(Project project, Diagnostic diagnostic)> diagnostics)
	{
		var newResults = diagnostics.RemoveAll(x => x.diagnostic.Id != PartialPropertyDiagnosticId);

		return base.FilterDiagnostics(newResults);
	}

	#endregion
}
