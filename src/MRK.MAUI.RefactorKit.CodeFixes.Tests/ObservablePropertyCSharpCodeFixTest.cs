using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

using MRK.MAUI.RefactorKit.Tests;

namespace MRK.MAUI.RefactorKit.CodeFixes.Tests;

/// <summary>
/// The C# analyzer test for an observable property
/// </summary>
internal class ObservablePropertyCSharpCodeFixTest : CSharpCodeFixTest<MRKAnalyzerProperty, MRKCodeFixProviderProperty, DefaultVerifier>
{
	#region Constructors

	/// <summary>
	/// Default constructor
	/// </summary>
	public ObservablePropertyCSharpCodeFixTest() : base()
	{
		ReferenceAssemblies = TestConstants.Net9MvvmAssemblies;
	}

	#endregion
}