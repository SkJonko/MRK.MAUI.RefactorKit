using Xunit;

namespace MRK.MAUI.RefactorKit.Tests;

/// <summary>
/// Contains the tests regarding the <see cref="MRKAnalyzerDelegateCommand"/>
/// </summary>
public sealed class MRKAnalyzerDelegateCommandTests : BaseSyntaxDiagnosticAnalyzerTests<MRKAnalyzerDelegateCommand>
{
	#region Constructors

	/// <summary>
	/// Default constructor
	/// </summary>
	public MRKAnalyzerDelegateCommandTests() : base()
	{

	}

	#endregion

	#region Test Methods

	/// <summary>
	/// Validates that when a deprecated asynchronous delegate command is analyzed, at least one diagnostic error occurs
	/// </summary>
	[Fact]
	public async Task MRKAnalyzerDelegateCommand_DiagnosticErrorsOccur_WhenDeprecatedAsyncDelegateCommandIsAnalyzed()
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

		var analyzerTest = new MvvmCSharpAnalyzerTest<MRKAnalyzerDelegateCommand>()
		{
			TestCode = testCode
		};

		await Assert.ThrowsAnyAsync<Exception>(() => analyzerTest.RunAsync());
	}

	/// <summary>
	/// Validates that when a deprecated delegate command is analyzed, at least one diagnostic error occurs
	/// </summary>
	[Fact]
	public async Task MRKAnalyzerDelegateCommand_DiagnosticErrorsOccur_WhenDeprecatedDelegateCommandIsAnalyzed()
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

		var analyzerTest = new MvvmCSharpAnalyzerTest<MRKAnalyzerDelegateCommand>()
		{
			TestCode = testCode
		};

		await Assert.ThrowsAnyAsync<Exception>(() => analyzerTest.RunAsync());
	}

	/// <summary>
	/// Validates that when a valid asynchronous relay command is analyzed, no diagnostics occur
	/// </summary>
	[Fact]
	public async Task MRKAnalyzerDelegateCommand_NoDiagnosticsOccur_WhenValidAsyncRelayCommandIsAnalyzed()
	{
		var testCode = /* lang=c#-test */@"
		using CommunityToolkit.Mvvm.ComponentModel;
		using CommunityToolkit.Mvvm.Input;

		namespace Test
		{
			public partial class TestViewModel : ObservableObject
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
		}
		";

		var analyzerTest = new AttributePropertySetterCSharpCodeFixTest<MRKAnalyzerDelegateCommand>
		{
			TestCode = testCode
		};

		await TestHelpers.AssertNoExceptionThrownAsync(() => analyzerTest.RunAsync());
	}

	/// <summary>
	/// Validates that when a valid relay command is analyzed, no diagnostics occur
	/// </summary>
	[Fact]
	public async Task MRKAnalyzerDelegateCommand_NoDiagnosticsOccur_WhenValidRelayCommandIsAnalyzed()
	{
		var testCode = /* lang=c#-test */@"
		using CommunityToolkit.Mvvm.ComponentModel;
		using CommunityToolkit.Mvvm.Input;

		namespace Test
		{
			public partial class TestViewModel : ObservableObject
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

		var analyzerTest = new AttributePropertySetterCSharpCodeFixTest<MRKAnalyzerDelegateCommand>
		{
			TestCode = testCode
		};

		await TestHelpers.AssertNoExceptionThrownAsync(() => analyzerTest.RunAsync());
	}

	#endregion
}