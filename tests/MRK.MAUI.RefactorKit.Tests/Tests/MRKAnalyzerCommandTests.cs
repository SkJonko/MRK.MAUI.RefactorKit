using Xunit;

namespace MRK.MAUI.RefactorKit.Tests;

/// <summary>
/// Contains the tests regarding the <see cref="MRKAnalyzerCommand"/>
/// </summary>
public sealed class MRKAnalyzerCommandTests : BaseSyntaxDiagnosticAnalyzerTests<MRKAnalyzerCommand>
{
	#region Constructors

	/// <summary>
	/// Default constructor
	/// </summary>
	public MRKAnalyzerCommandTests() : base()
	{

	}

	#endregion

	#region Test Methods

	/// <summary>
	/// Validates that when a deprecated asynchronous command is analyzed, at least one diagnostic error occurs
	/// </summary>
	[Fact]
	public async Task MRKAnalyzerCommand_DiagnosticErrorsOccur_WhenDeprecatedAsyncCommandIsAnalyzed()
	{
		var testCode = /* lang=c#-test */@"
		using CommunityToolkit.Mvvm.ComponentModel;
		using Microsoft.Maui.Controls;
		using System.Threading.Tasks;

		namespace Test
		{
			public class TestViewModel: ObservableObject
			{
				private Command _testCommand;

				public Command TestCommand => _testCommand ?? (_testCommand = new Command(async (object a) => await TestAllAsync(a)));

				public Task TestAllAsync(object test)
				{
					return Task.CompletedTask;
				}
			}
		}
		";

		var analyzerTest = new MvvmCSharpAnalyzerTest<MRKAnalyzerCommand>()
		{
			TestCode = testCode
		};

		await Assert.ThrowsAnyAsync<Exception>(() => analyzerTest.RunAsync());
	}

	/// <summary>
	/// Validates that when a deprecated command is analyzed, at least one diagnostic error occurs
	/// </summary>
	[Fact]
	public async Task MRKAnalyzerCommand_DiagnosticErrorsOccur_WhenDeprecatedCommandIsAnalyzed()
	{
		var testCode = /* lang=c#-test */@"
		using CommunityToolkit.Mvvm.ComponentModel;
		using Microsoft.Maui.Controls;

		namespace Test
		{
			public class TestViewModel: ObservableObject
			{
				private Command _testCommand;

				public Command TestCommand => _testCommand ?? (_testCommand = new Command((object a) => TestAll(a)));

				public void TestAll(object test)
				{
					return;
				}
			}
		}
		";

		var analyzerTest = new MvvmCSharpAnalyzerTest<MRKAnalyzerCommand>()
		{
			TestCode = testCode
		};

		await Assert.ThrowsAnyAsync<Exception>(() => analyzerTest.RunAsync());
	}

	/// <summary>
	/// Validates that when a valid asynchronous command is analyzed, no diagnostics occur
	/// </summary>
	[Fact]
	public async Task MRKAnalyzerCommand_NoDiagnosticsOccur_WhenValidAsyncCommandIsAnalyzed()
	{
		var testCode = /* lang=c#-test */@"
		using CommunityToolkit.Mvvm.ComponentModel;
		using CommunityToolkit.Mvvm.Input;
		using System.Threading.Tasks;

		namespace Test
		{
			public partial class TestViewModel : ObservableObject
			{
				[RelayCommand]
				private async Task TestAsync(object a)
				{
					await TestAllAsync(a);
				}

				public Task TestAllAsync(object test)
				{
					return Task.CompletedTask;
				}
			}
		}
		";

		var analyzerTest = new MvvmCSharpAnalyzerTest<MRKAnalyzerCommand>
		{
			TestCode = testCode
		};

		await TestHelpers.AssertNoExceptionThrownAsync(() => analyzerTest.RunAsync());
	}

	/// <summary>
	/// Validates that when a valid command is analyzed, no diagnostics occur
	/// </summary>
	[Fact]
	public async Task MRKAnalyzerCommand_NoDiagnosticsOccur_WhenValidCommandIsAnalyzed()
	{
		var testCode = /* lang=c#-test */@"
		using CommunityToolkit.Mvvm.ComponentModel;
		using CommunityToolkit.Mvvm.Input;

		namespace Test
		{
			public partial class TestViewModel : ObservableObject
			{
				[RelayCommand]
				private void Test(object a)
				{
					TestAll(a);
				}

				public void TestAll(object test)
				{
					return;
				}
			}
		}
		";

		var analyzerTest = new MvvmCSharpAnalyzerTest<MRKAnalyzerCommand>
		{
			TestCode = testCode
		};

		await TestHelpers.AssertNoExceptionThrownAsync(() => analyzerTest.RunAsync());
	}

	#endregion
}
