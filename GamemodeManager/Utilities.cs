namespace GamemodeManager
{
	/// <summary>
	/// An ancillary class of methods
	/// </summary>
	public static class Utilities
	{
		/// <summary>
		/// Return a new collection within the range specified
		/// </summary>
		/// <typeparam name="T">The type of the input array</typeparam>
		/// <param name="input">The input array</param>
		/// <param name="startIndex">The index to start at</param>
		/// <param name="endIndex">The index to stop at</param>
		/// <returns>The new array</returns>
		public static T[] Range<T>(this T[] input, int startIndex, int endIndex)
		{
			T[] result = new T[endIndex - startIndex];

			int index = 0;
			while (startIndex++ < endIndex)
			{
				result[index++] = input[startIndex];
			}

			return result;
		}

		/// <summary>
		/// Return a new collection of the size specified
		/// </summary>
		/// <typeparam name="T">The type of the input array</typeparam>
		/// <param name="input">The input array</param>
		/// <param name="amount">The amount of values to select</param>
		/// <returns>The new array</returns>
		public static T[] Range<T>(this T[] input, int amount) =>
			Range(input, 0, amount);
	}
}
