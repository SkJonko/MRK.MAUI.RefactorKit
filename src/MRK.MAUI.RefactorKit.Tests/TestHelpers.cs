using Xunit;

namespace MRK.MAUI.RefactorKit.Tests;

/// <summary>
/// Contains the helper methods used across the tests
/// </summary>
public static class TestHelpers
{
	#region Public Methods

	/// <summary>
	/// Asserts that no exception is thrown when the <paramref name="action"/> is invoked
	/// </summary>
	/// <param name="action">The action that will be invoked</param>
	/// <returns></returns>
	public static async Task AssertNoExceptionThrownAsync(Func<Task> action)
		=> Assert.True(await WasNoExceptionThrownAsync(action).ConfigureAwait(false));

	#endregion

	#region Private Methods

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

	#endregion
}
