using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

using MRK.MAUI.RefactorKit.Tests;

namespace MRK.MAUI.RefactorKit.CodeFixes.Tests;

/// <summary>
/// The C# code fix test targeting MVVM related assemblies
/// </summary>
/// <typeparam name="TAnalyzer">The type of the analyzer</typeparam>
/// <typeparam name="TCodeFix">The type of the code fix provider</typeparam>
internal class MvvmCSharpCodeFixTest<TAnalyzer, TCodeFix> : CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>
	where TAnalyzer : DiagnosticAnalyzer, new()
	where TCodeFix : CodeFixProvider, new()
{
	#region Constructors

	/// <summary>
	/// Default constructor
	/// </summary>
	public MvvmCSharpCodeFixTest() : base()
	{
		ReferenceAssemblies = TestConstants.Net9MvvmAssemblies;
	}

	#endregion
}