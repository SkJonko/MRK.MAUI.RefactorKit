using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using MRK.MAUI.RefactorKit.Tests;

namespace MRK.MAUI.RefactorKit.CodeFixes.Tests;

/// <summary>
/// The C# code fix test that ignores the diagnostic with the <see cref="TestConstants.PartialMembersDiagnosticId"/>
/// </summary>
/// <typeparam name="TAnalyzer">The type of the analyzer</typeparam>
internal sealed class PartialMemberCSharpCodeFixTest<TAnalyzer, TCodeFix> : ExcludedDiagnosticsCSharpCodeFixTest<TAnalyzer, TCodeFix>
	where TAnalyzer : DiagnosticAnalyzer, new()
	where TCodeFix : CodeFixProvider, new()
{
	#region Protected Properties

	/// <summary>
	/// <inheritdoc/>
	/// </summary>
	/// <returns></returns>
	protected override IEnumerable<string> ExcludedDiagnosticIds { get; } = [TestConstants.PartialMembersDiagnosticId];

	#endregion
}