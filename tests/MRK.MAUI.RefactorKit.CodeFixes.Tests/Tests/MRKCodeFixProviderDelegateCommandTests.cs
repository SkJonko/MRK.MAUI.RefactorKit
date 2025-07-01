using Microsoft.CodeAnalysis.Testing;

using MRK.MAUI.RefactorKit.Tests;

using Xunit;

namespace MRK.MAUI.RefactorKit.CodeFixes.Tests;

/// <summary>
/// Contains the tests for the <see cref="MRKCodeFixProviderDelegateCommand"/>
/// </summary>
public sealed class MRKCodeFixProviderDelegateCommandTests
{
	#region Constructors

	/// <summary>
	/// Default constructor
	/// </summary>
	public MRKCodeFixProviderDelegateCommandTests() : base()
	{

	}

	#endregion

	#region Tests Methods

	/// <summary>
	/// Validates that when a deprecated command, the expected code fix is applied successfully
	/// </summary>
	[Fact]
	public async Task MRKCodeFixProviderDelegateCommand_CodeFixIsAppliedSuccessfully_WhenDeprecatedCommandIsAnalyzed()
	{
		var testCode = /* lang=c#-test */@"
		using CommunityToolkit.Mvvm.ComponentModel;
		using Prism.Commands;

		namespace Test
		{
			public class TestViewModel: ObservableObject
			{
				private DelegateCommand _searchBtnCommand;

				public DelegateCommand SearchBtnCommand =>
					_searchBtnCommand ?? (_searchBtnCommand = new DelegateCommand(ExecuteSearchBtnCommand, CanExecuteSearchBtnCommand));

				public void ExecuteSearchBtnCommand()
				{
					if (true)
					{

					}
				}

				bool CanExecuteSearchBtnCommand()
				{
					if (true)
					{
						return true;
					}

					if (false)
					{
						return false;
					}
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
        [RelayCommand(CanExecute = nameof(CanExecuteSearchBtnCommand))]
        public void SearchBtn()
				{
					if (true)
					{

					}
				}

				bool CanExecuteSearchBtnCommand()
				{
					if (true)
					{
						return true;
					}

					if (false)
					{
						return false;
					}
				}
			}
		}
		";

		var codeFixTest = new AttributePropertySetterCSharpAnalyzerTest<MRKAnalyzerDelegateCommand, MRKCodeFixProviderDelegateCommand>()
		{
			TestCode = testCode,
			FixedCode = fixedCode,
			NumberOfFixAllIterations = 2,
			NumberOfFixAllInDocumentIterations = 2,
			NumberOfFixAllInProjectIterations = 2,
			NumberOfIncrementalIterations = 2
		};

		var exceptedAnalyzerDiagnostic = new DiagnosticResult(MRKAnalyzerDelegateCommand.Rule).WithArguments("SearchBtnCommand")
										.WithSpan(11, 28, 11, 44);

		codeFixTest.ExpectedDiagnostics.Add(exceptedAnalyzerDiagnostic);

		await TestHelpers.AssertNoExceptionThrownAsync(() => codeFixTest.RunAsync());
	}

	/// <summary>
	/// Validates that when a deprecated asynchronous command is analyzed, the expected code fix is applied successfully
	/// </summary>
	[Fact(Skip = "Unfinished feature")]
	public async Task MRKCodeFixProviderDelegateCommand_CodeFixIsAppliedSuccessfully_WhenDeprecatedAsyncCommandIsAnalyzed()
	{
		var testCode = /* lang=c#-test */@"
		using CommunityToolkit.Mvvm.ComponentModel;
		using Prism.Commands;

		namespace Test
		{
			public class TestViewModel: ObservableObject
			{
				private bool _canExecuteCommand = false;

				public bool CanExecuteCommand
				{
					get { return _canExecuteCommand; }
					set { SetProperty(ref _canExecuteCommand, value); }
				}

				private DelegateCommand _searchBtnCommand;

				public DelegateCommand SearchBtnCommand =>
					_searchBtnCommand ?? (_searchBtnCommand = new DelegateCommand(ExecuteSearchBtnCommand, CanExecuteSearchBtnCommand));

				public async void ExecuteSearchBtnCommand()
				{
					if (true)
					{

					}
				}

				bool CanExecuteSearchBtnCommand()
				{
					return CanExecuteCommand;
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
				private bool _canExecuteCommand = false;

				public bool CanExecuteCommand
				{
					get { return _canExecuteCommand; }
					set { SetProperty(ref _canExecuteCommand, value); }
				}

        [RelayCommand(CanExecute = nameof(CanExecuteCommand))]
        public async void SearchBtnAsync()
				{
					if (true)
					{

				}
    }
}
		";

		var codeFixTest = new AttributePropertySetterCSharpAnalyzerTest<MRKAnalyzerDelegateCommand, MRKCodeFixProviderDelegateCommand>()
		{
			TestCode = testCode,
			FixedCode = fixedCode,
			NumberOfFixAllIterations = 2,
			NumberOfFixAllInDocumentIterations = 2,
			NumberOfFixAllInProjectIterations = 2,
			NumberOfIncrementalIterations = 2
		};

		var exceptedAnalyzerDiagnostic = new DiagnosticResult(MRKAnalyzerDelegateCommand.Rule).WithArguments("SearchBtnCommand")
										.WithSpan(19, 28, 19, 44);

		codeFixTest.ExpectedDiagnostics.Add(exceptedAnalyzerDiagnostic);

		await codeFixTest.RunAsync();
		await TestHelpers.AssertNoExceptionThrownAsync(() => codeFixTest.RunAsync());
	}

	#endregion
}