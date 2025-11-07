namespace Project.Domain.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// An extension method that checks if a string is null or empty.
        /// </summary>
        /// <param name="s">The string instance to check.</param>
        /// <returns>True if the string is null or empty, otherwise false.</returns>
        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }
    }
}