using Microsoft.CodeAnalysis.Diagnostics;

namespace MRK.MAUI.RefactorKit.Tests;

/// <summary>
/// The C# analyzer test that ignores the diagnostic with the <see cref="TestConstants.AttributePropertySetterDiagnosticId"/>
/// </summary>
/// <typeparam name="TAnalyzer">The type of the analyzer</typeparam>
internal sealed class AttributePropertySetterCSharpCodeFixTest<TAnalyzer> : ExcludedDiagnosticsCSharpAnalyzerTest<TAnalyzer>
	where TAnalyzer : DiagnosticAnalyzer, new()
{
	#region Protected Properties

	/// <summary>
	/// <inheritdoc/>
	/// </summary>
	/// <returns></returns>
	protected override IEnumerable<string> ExcludedDiagnosticIds { get; } = [TestConstants.AttributePropertySetterDiagnosticId];

	#endregion
}