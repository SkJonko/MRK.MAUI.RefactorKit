using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace MRK.MAUI.RefactorKit.Tests;

/// <summary>
/// The C# analyzer test targeting MVVM related assemblies
/// </summary>
/// <typeparam name="TAnalyzer">The type of the analyzer</typeparam>
internal class MvvmCSharpAnalyzerTest<TAnalyzer> : CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
	where TAnalyzer : DiagnosticAnalyzer, new()
{
	#region Constructors

	/// <summary>
	/// Default constructor
	/// </summary>
	public MvvmCSharpAnalyzerTest() : base()
	{
		ReferenceAssemblies = TestConstants.Net9MvvmAssemblies;
	}

	#endregion
}
/// <summary>
/// The C# analyzer test for an observable property
/// </summary>
/// <typeparam name="TAnalyzer">The type of the analyzer</typeparam>
internal class DelegateCommandCSharpAnalyzerTest<TAnalyzer> : CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
	where TAnalyzer : DiagnosticAnalyzer, new()
{
	#region Constructors

	/// <summary>
	/// Default constructor
	/// </summary>
	public DelegateCommandCSharpAnalyzerTest() : base()
	{
		ReferenceAssemblies = TestConstants.Net9MvvmAssemblies;
	}

	#endregion
}
