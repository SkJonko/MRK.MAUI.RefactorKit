using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace MRK.MAUI.RefactorKit.CodeFixes.Tests;

/// <summary>
/// The C# analyzer test for an observable property
/// </summary>
internal class ObservablePropertyCSharpAnalyzerTest : CSharpAnalyzerTest<MRKAnalyzerProperty, DefaultVerifier>
{
	#region Constructors

	/// <summary>
	/// Default constructor
	/// </summary>
	public ObservablePropertyCSharpAnalyzerTest() : base()
	{
		ReferenceAssemblies = ReferenceAssemblies.Net
							.Net90
							.AddPackages([new PackageIdentity("CommunityToolkit.Mvvm", "8.4.0")]);
	}

	#endregion
}
