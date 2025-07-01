using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using NSubstitute;

using Xunit;

namespace MRK.MAUI.RefactorKit.Tests;

/// <summary>
/// Contains the tests regarding the <see cref="MRKAnalyzerProperty"/>
/// </summary>
public sealed class MRKAnalyzerPropertyTests : BaseDiagnosticAnalyzerTests<MRKAnalyzerProperty>
{
	#region Constructors

	/// <summary>
	/// Default constructor
	/// </summary>
	public MRKAnalyzerPropertyTests() : base()
	{

	}

	#endregion

	#region Test Methods

	/// <summary>
	/// Validates that when a deprecated observable property is analyzed, at least one diagnostic error occurs
	/// </summary>
	[Fact]
	public async Task MRKAnalyzerProperty_DiagnosticErrorsOccur_WhenDeprecatedObservablePropertyIsAnalyzed()
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

		var analyzerTest = new MvvmCSharpAnalyzerTest<MRKAnalyzerProperty>()
		{
			TestCode = testCode
		};

		await Assert.ThrowsAnyAsync<Exception>(() => analyzerTest.RunAsync());
	}

	/// <summary>
	/// Validates that when a deprecated observable property is analyzed, at least one diagnostic error occurs
	/// </summary>
	[Fact]
	public async Task MRKAnalyzerProperty_DiagnosticErrorsOccur_WhenDeprecatedPropertyWithNotifyPropertyChangedTargetIsAnalyzed()
	{
		var testCode = /* lang=c#-test */@"
		using CommunityToolkit.Mvvm.ComponentModel;

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
			}
		}
		";

		var analyzerTest = new MvvmCSharpAnalyzerTest<MRKAnalyzerProperty>()
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

		var analyzerTest = new PartialMemberCSharpAnalyzerTest<MRKAnalyzerProperty>
		{
			TestCode = testCode
		};

		await TestHelpers.AssertNoExceptionThrownAsync(() => analyzerTest.RunAsync());
	}

	/// <summary>
	/// Validates that when a valid observable property is analyzed, no diagnostics occur
	/// </summary>
	[Fact]
	public async Task MRKAnalyzerProperty_NoDiagnosticsOccur_WhenValidPropertyWithNotifyPropertyChangedForAttributeIsAnalyzed()
	{
		var testCode = /* lang=c#-test */@"
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

		var analyzerTest = new PartialMemberCSharpAnalyzerTest<MRKAnalyzerProperty>
		{
			TestCode = testCode
		};

		await TestHelpers.AssertNoExceptionThrownAsync(() => analyzerTest.RunAsync());
	}

	#endregion

	#region Protected Methods

	/// <summary>
	/// <inheritdoc/>
	/// </summary>
	protected sealed override void AssertNoExceptionThrownInRegisterAction(AnalysisContext analysisContext)
	{
		ArgumentNullException.ThrowIfNull(analysisContext);

		var validSymbolAction = Arg.Any<Action<SymbolAnalysisContext>>();

		var validSymbolKinds = Arg.Is<ImmutableArray<SymbolKind>>(x => IsValidPropertySymbolArgument(x));

		AssertNoExceptionThrown(() => analysisContext.Received(1).RegisterSymbolAction(validSymbolAction, validSymbolKinds));
	}

	#endregion

	#region Private Methods

	/// <summary>
	/// Checks whether the specified <paramref name="symbolKinds"/> is a valid argument
	/// </summary>
	/// <param name="symbolKinds">The kinds of symbol</param>
	/// <returns></returns>
	private static bool IsValidPropertySymbolArgument(ImmutableArray<SymbolKind> symbolKinds)
	{
		if (symbolKinds.Length != 1)
		{
			return false;
		}

		if (symbolKinds[0] != SymbolKind.Property)
		{
			return false;
		}

		return true;
	}

	#endregion
}
