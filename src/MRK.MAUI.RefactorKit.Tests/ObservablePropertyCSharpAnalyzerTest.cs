using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace MRK.MAUI.RefactorKit.Tests;

/// <summary>
/// The C# analyzer test for an observable property
/// </summary>
/// <typeparam name="TAnalyzer">The type of the analyzer</typeparam>
internal class ObservablePropertyCSharpAnalyzerTest<TAnalyzer> : CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
	where TAnalyzer : DiagnosticAnalyzer, new()
{
	#region Constructors

	/// <summary>
	/// Default constructor
	/// </summary>
	public ObservablePropertyCSharpAnalyzerTest() : base()
	{
		ReferenceAssemblies = TestConstants.Net9MvvmAssemblies;
	}

	#endregion
}
