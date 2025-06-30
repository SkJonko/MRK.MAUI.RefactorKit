using Microsoft.CodeAnalysis.Testing;

using MRK.MAUI.RefactorKit.Tests;

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

	/// <summary>
	/// Validates that when a deprecated observable property is analyzed, the expected code fix is applied successfully
	/// </summary>
	[Fact]
	public async Task MRKCodeFixProviderProperty_CodeFixIsAppliedSuccessfully_WhenDeprecatedPropertyIsAnalyzed()
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

		var codeFixTest = new PartialPropertyCSharpCodeFixTest()
		{
			TestCode = testCode,
			FixedCode = fixedCode
		};

		var exceptedAnalyzerDiagnostic = new DiagnosticResult(MRKAnalyzerProperty.Rule).WithArguments("Name")
										.WithSpan(10, 19, 10, 23);

		codeFixTest.ExpectedDiagnostics.Add(exceptedAnalyzerDiagnostic);

		await TestHelpers.AssertNoExceptionThrownAsync(() => codeFixTest.RunAsync());
	}

	/// <summary>
	/// Validates that when a deprecated observable property with a NotifyPropertyChangedFor target is analyzed, the expected code fix is applied successfully
	/// </summary>
	[Fact]
	public async Task MRKCodeFixProviderProperty_CodeFixIsAppliedSuccessfully_WhenDeprecatedPropertyWithNotifyPropertyChangedForAttributeIsAnalyzed()
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
						OnPropertyChanged(nameof(CanExecuteCommand));
					}
				}

				public bool CanExecuteCommand  { get; set; }
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
        [NotifyPropertyChangedFor(nameof(CanExecuteCommand))]
        public partial string Name { get; set; }
        public bool CanExecuteCommand  { get; set; }
			}
		}
		";

		var codeFixTest = new PartialPropertyCSharpCodeFixTest()
		{
			TestCode = testCode,
			FixedCode = fixedCode
		};

		var exceptedAnalyzerDiagnostic = new DiagnosticResult(MRKAnalyzerProperty.Rule).WithArguments("Name")
										.WithSpan(10, 19, 10, 23);

		codeFixTest.ExpectedDiagnostics.Add(exceptedAnalyzerDiagnostic);

		await TestHelpers.AssertNoExceptionThrownAsync(() => codeFixTest.RunAsync());
	}

	#endregion
}