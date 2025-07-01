using Microsoft.CodeAnalysis.Testing;

namespace MRK.MAUI.RefactorKit.Tests;

/// <summary>
/// Contains the constants used across the tests
/// </summary>
public static class TestConstants
{
	/// <summary>
	/// The id for the C# 13 and before Partial Member diagnostic
	/// CS0267: The 'partial' modifier can only appear immediately before 'class', 'record', 'struct', 'interface'.
	/// Before C# 13, partial wasn't allowed on properties or indexers.
	/// More info:https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/partial-declarations#partial-types
	/// </summary>
	public const string PartialMembersDiagnosticId = "CS0267";

	/// <summary>
	/// The id for the attribute property setter diagnostic
	/// CS0267: Property, indexer, or event on an attribute property is not supported by the language version.
	/// </summary>
	public const string AttributePropertySetterDiagnosticId = "CS1545";

	/// <summary>
	/// The referenced assemblies for a .Net 9 project that uses the MVVM pattern
	/// </summary>
	public static readonly ReferenceAssemblies Net9MvvmAssemblies = ReferenceAssemblies.Net
							.Net90
							.AddPackages([
								new PackageIdentity("CommunityToolkit.Mvvm", "8.4.0"),
								new PackageIdentity("Microsoft.Maui.Controls", "9.0.81"),
								new PackageIdentity("Microsoft.Toolkit.Mvvm", "7.1.2"),
								new PackageIdentity("Prism.Core", "9.0.537")
								]);
}
