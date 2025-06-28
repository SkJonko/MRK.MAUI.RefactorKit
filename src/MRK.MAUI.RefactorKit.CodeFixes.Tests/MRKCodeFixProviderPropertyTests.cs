using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

using Xunit;

namespace MRK.MAUI.RefactorKit.CodeFixes.Tests;

/// <summary>
/// Contains the tests for the <see cref="MRKCodeFixProviderProperty"/>
/// </summary>
public sealed class MRKCodeFixProviderPropertyTests
{
	#region Constructors

	/// <summary>
	/// Default constructor
	/// </summary>
	public MRKCodeFixProviderPropertyTests() : base()
	{

	}

	#endregion

	#region Tests Methods

	[Fact]
	public async Task Test1()
	{
		var testCode = /* lang=c#-test */@"
		using CommunityToolkit.Mvvm.ComponentModel;

		namespace Test
		{
			public partial class TestViewModel : ObservableObject
			{
				private string _name;

				public string Name
				{
					get => _name;
					set
					{
						_name = value;
						OnPropertyChanged(nameof(Name));
					}
				}
			}
		}
		";

		var fixedCode = /* lang=c#-test */@"
		using CommunityToolkit.Mvvm.ComponentModel;

		namespace Test
		{
			public partial class TestViewModel : ObservableObject
			{
				[ObservableProperty]
				public partial string Name { get; set; }
			}
		}
		";

		var codeFixTest = new CSharpCodeFixTest<MRKAnalyzerProperty, MRKCodeFixProviderProperty, DefaultVerifier>()
		{
			ReferenceAssemblies = ReferenceAssemblies.Net
									.Net90
									.AddPackages([new PackageIdentity("CommunityToolkit.Mvvm", "8.4.0")]),
			TestCode = testCode,
			BatchFixedCode = fixedCode,
		};

		var t = new DiagnosticResult(MRKAnalyzerProperty.Rule).WithArguments("Name").WithSpan(10, 19, 10, 23);

		codeFixTest.ExpectedDiagnostics.Add(t);

		await codeFixTest.RunAsync();
	}

	/// <summary>
	/// Validates that when a deprecated observable property is analyzed, at least one diagnostic error occurs
	/// </summary>
	[Fact]
	public async Task MRKAnalyzerProperty_DiagnosticErrorsOccur_WhenDeprecatedPropertyIsAnalyzed()
	{
		var testCode = /* lang=c#-test */@"
		using CommunityToolkit.Mvvm.ComponentModel;

		namespace Test
		{
			public class TestViewModel: ObservableObject
			{

				private string _name;

				public string Name
				{
					get => _name;
					set
					{
						_name = value;
						OnPropertyChanged(nameof(Name));
					}
				}
			}
		}
		";

		var analyzerTest = new ObservablePropertyCSharpAnalyzerTest()
		{
			TestCode = testCode
		};

		await Assert.ThrowsAnyAsync<Exception>(() => analyzerTest.RunAsync());
	}

	/// <summary>
	/// Validates that when a valid observable property is analyzed, no diagnostics occur
	/// </summary>
	[Fact]
	public async Task MRKAnalyzerProperty_NoDiagnosticsOccur_WhenValidPropertyIsAnalyzed()
	{
		var testCode = /* lang=c#-test */@"
		using CommunityToolkit.Mvvm.ComponentModel;

		namespace Test
		{
			public partial class TestViewModel : ObservableObject
			{
				[ObservableProperty]
				public partial string Name { get; set; }
			}
		}
		";

		var analyzerTest = new PartialPropertyCSharpAnalyzerTest
		{
			TestCode = testCode,
		};

		await AssertNoExceptionThrown(() => analyzerTest.RunAsync());
	}

	#endregion

	/// <summary>
	/// Asserts that no exception is thrown when the <paramref name="action"/> is invoked
	/// </summary>
	/// <param name="action">The action that will be invoked</param>
	/// <returns></returns>
	private static async Task AssertNoExceptionThrown(Func<Task> action)
		=> Assert.True(await WasNoExceptionThrownAsync(action).ConfigureAwait(false));

	/// <summary>
	/// Checks that no exception is thrown when the <paramref name="action"/> is invoked
	/// </summary>
	/// <param name="action">The action that will be invoked</param>
	/// <returns></returns>
	private static async Task<bool> WasNoExceptionThrownAsync(Func<Task> action)
	{
		ArgumentNullException.ThrowIfNull(action);

		try
		{
			await action().ConfigureAwait(false);

			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}
}

public class TestVerifier : DefaultVerifier
{
	public override void Equal<T>(T expected, T actual, string? message = null)
	{
		base.Equal(expected, actual, message);
	}

	public override IVerifier PushContext(string context)
	{
		return base.PushContext(context);
	}
}