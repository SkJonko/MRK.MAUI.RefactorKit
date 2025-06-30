using Microsoft.CodeAnalysis.Testing;

namespace MRK.MAUI.RefactorKit.Tests;

/// <summary>
/// Contains the constants used across the tests
/// </summary>
public static class TestConstants
{
	/// <summary>
	/// The id for the C# 13 and before Partial Property diagnostic
	/// CS0267: The 'partial' modifier can only appear immediately before 'class', 'record', 'struct', 'interface'.
	/// Before C# 13, partial wasn't allowed on properties or indexers.
	/// More info:https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/partial-declarations#partial-types
	/// </summary>
	public const string PartialPropertyDiagnosticId = "CS0267";

	/// <summary>
	/// The referenced assemblies for a .Net 9 project that uses the MVVM pattern
	/// </summary>
	public static readonly ReferenceAssemblies Net9MvvmAssemblies = ReferenceAssemblies.Net
							.Net90
							.AddPackages([new PackageIdentity("CommunityToolkit.Mvvm", "8.4.0")]);
}
