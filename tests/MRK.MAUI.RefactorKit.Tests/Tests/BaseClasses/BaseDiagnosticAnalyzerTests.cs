using Microsoft.CodeAnalysis.Diagnostics;

using NSubstitute;

using Xunit;

namespace MRK.MAUI.RefactorKit.Tests;

/// <summary>
/// The base class for the tests regarding a <see cref="DiagnosticAnalyzer"/>
/// </summary>
/// <typeparam name="TDiagnosticAnalyzer"></typeparam>
public abstract class BaseDiagnosticAnalyzerTests<TDiagnosticAnalyzer>
	where TDiagnosticAnalyzer : DiagnosticAnalyzer, new()
{
	#region Test Methods

	/// <summary>
	/// Validates that only one element is contained in the <see cref="TDiagnosticAnalyzer.SupportedDiagnostics"/>
	/// </summary>
	[Fact]
	public void DiagnosticAnalyzer_OnlyOneElementExistsInTheSupportedDiagnostics_WhenConstructionIsCompleted()
	{
		var diagnosticAnalyzer = new TDiagnosticAnalyzer();

		Assert.Single(diagnosticAnalyzer.SupportedDiagnostics);
	}

	/// <summary>
	/// Validates that an <see cref="Exception"/> is thrown when a null argument is passed in the 
	/// <see cref="TDiagnosticAnalyzer.Initialize(AnalysisContext)"/> method
	/// </summary>
	[Fact]
	public void Initialize_ThrowsAnException_WhenNullArgumentIsPassed()
	{
		var diagnosticAnalyzer = new TDiagnosticAnalyzer();

		Assert.ThrowsAny<Exception>(() => diagnosticAnalyzer.Initialize(null));
	}

	/// <summary>
	/// Validates that no <see cref="Exception"/> are thrown when the
	/// <see cref="MRKAnalyzerProperty.Initialize(AnalysisContext)"/> method
	/// is called successfully
	/// </summary>
	[Fact]
	public void Initialize_NoExceptionsAreThrown_WhenTheInitializationIsSuccessful()
	{
		var diagnosticAnalyzer = new TDiagnosticAnalyzer();

		var analysisContext = Substitute.For<AnalysisContext>();

		diagnosticAnalyzer.Initialize(analysisContext);

		AssertNoExceptionThrown(() => analysisContext.Received(1).ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None));

		AssertNoExceptionThrown(() => analysisContext.Received(1).EnableConcurrentExecution());

		AssertNoExceptionThrownInRegisterAction(analysisContext);
	}

	#endregion

	#region Protected Methods

	/// <summary>
	/// Asserts that no exception is thrown when the <paramref name="action"/> is invoked
	/// </summary>
	/// <param name="action">The action that will be invoked</param>
	/// <returns></returns>
	protected static void AssertNoExceptionThrown(Action action)
		=> Assert.True(WasNoExceptionThrown(action));

	/// <summary>
	/// Asserts whether the register action is executed no exception is thrown
	/// </summary>
	/// <param name="analysisContext">The analysis context</param>
	protected abstract void AssertNoExceptionThrownInRegisterAction(AnalysisContext analysisContext);

	#endregion

	#region Private Methods

	/// <summary>
	/// Checks that no exception is thrown when the <paramref name="action"/> is invoked
	/// </summary>
	/// <param name="action">The action that will be invoked</param>
	/// <returns></returns>
	private static bool WasNoExceptionThrown(Action action)
	{
		ArgumentNullException.ThrowIfNull(action);

		try
		{
			action();

			return true;
		}
		catch (Exception)

		{
			return false;
		}
	}

	#endregion
}