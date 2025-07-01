using Microsoft.CodeAnalysis.Testing;

using MRK.MAUI.RefactorKit.Tests;

using Xunit;

namespace MRK.MAUI.RefactorKit.CodeFixes.Tests;

/// <summary>
/// Contains the tests for the <see cref="MRKCodeFixProviderCommand"/>
/// </summary>
public sealed class MRKCodeFixProviderCommandTests
{
	#region Constructors

	/// <summary>
	/// Default constructor
	/// </summary>
	public MRKCodeFixProviderCommandTests() : base()
	{

	}

	#endregion

	#region Tests Methods

	/// <summary>
	/// Validates that when a deprecated command, the expected code fix is applied successfully
	/// </summary>
	[Fact]
	public async Task MRKCodeFixProviderCommand_CodeFixIsAppliedSuccessfully_WhenDeprecatedCommandIsAnalyzed()
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

		var fixedCode = /* lang=c#-test */@"
		using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Test
		{
			public class TestViewModel: ObservableObject
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

		var codeFixTest = new MvvmCSharpCodeFixTest<MRKAnalyzerCommand, MRKCodeFixProviderCommand>()
		{
			TestCode = testCode,
			FixedCode = fixedCode,
			NumberOfFixAllIterations = 2,
			NumberOfFixAllInDocumentIterations = 2,
			NumberOfFixAllInProjectIterations = 2,
			NumberOfIncrementalIterations = 2
		};

		var exceptedAnalyzerDiagnostic = new DiagnosticResult(MRKAnalyzerCommand.Rule).WithArguments("TestCommand")
										.WithSpan(11, 20, 11, 31);

		codeFixTest.ExpectedDiagnostics.Add(exceptedAnalyzerDiagnostic);

		await TestHelpers.AssertNoExceptionThrownAsync(() => codeFixTest.RunAsync());
	}

	/// <summary>
	/// Validates that when a deprecated asynchronous command is analyzed, the expected code fix is applied successfully
	/// </summary>
	[Fact]
	public async Task MRKCodeFixProviderCommand_CodeFixIsAppliedSuccessfully_WhenDeprecatedAsyncCommandIsAnalyzed()
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

		var fixedCode = /* lang=c#-test */@"
		using CommunityToolkit.Mvvm.ComponentModel;
		using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;

namespace Test
		{
			public class TestViewModel: ObservableObject
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

		var codeFixTest = new MvvmCSharpCodeFixTest<MRKAnalyzerCommand, MRKCodeFixProviderCommand>()
		{
			TestCode = testCode,
			FixedCode = fixedCode,
			NumberOfFixAllIterations = 2,
			NumberOfFixAllInDocumentIterations = 2,
			NumberOfFixAllInProjectIterations = 2,
			NumberOfIncrementalIterations = 2
		};

		var exceptedAnalyzerDiagnostic = new DiagnosticResult(MRKAnalyzerCommand.Rule).WithArguments("TestCommand")
										.WithSpan(12, 20, 12, 31);

		codeFixTest.ExpectedDiagnostics.Add(exceptedAnalyzerDiagnostic);

		await TestHelpers.AssertNoExceptionThrownAsync(() => codeFixTest.RunAsync());
	}

	#endregion
}
