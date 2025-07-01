using Microsoft.CodeAnalysis.Diagnostics;

namespace MRK.MAUI.RefactorKit.Tests;

/// <summary>
/// The C# analyzer test that ignores the diagnostic with the <see cref="TestConstants.PartialMembersDiagnosticId"/>
/// </summary>
/// <typeparam name="TAnalyzer">The type of the analyzer</typeparam>
internal sealed class PartialMemberCSharpAnalyzerTest<TAnalyzer> : ExcludedDiagnosticsCSharpAnalyzerTest<TAnalyzer>
	where TAnalyzer : DiagnosticAnalyzer, new()
{
	#region Protected Properties

	/// <summary>
	/// <inheritdoc/>
	/// </summary>
	/// <returns></returns>
	protected override IEnumerable<string> ExcludedDiagnosticIds { get; } = [TestConstants.PartialMembersDiagnosticId];

	#endregion
}
