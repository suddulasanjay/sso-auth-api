using System.Diagnostics.CodeAnalysis;

namespace SSOAuthAPI.Utilities
{
    public static class AppllicationExtensions

    {
        /// <summary>
        /// Checks if the current host environment name is local
        /// </summary>
        /// <param name="hostEnvironment">An instance of <see cref="IHostEnvironment"/>.</param>
        /// <returns>True if the environment name is local, otherwise false.</returns>
        public static bool IsLocal(this IHostEnvironment hostEnvironment)
        {
            return hostEnvironment.IsEnvironment("local");
        }

        public static void ForEach<T>(this IEnumerable<T>? enumerable, Action<T> iterFunc)
        {
            if (enumerable is null) return;

            foreach (var iterand in enumerable)
            {
                iterFunc(iterand);
            }
        }

        public static Dictionary<TKey, IEnumerable<T>> ToListedDictionary<TKey, T>(this IEnumerable<T> enumerable, Func<T, TKey> keySelector) where TKey : notnull
        {
            return enumerable.GroupBy(keySelector).ToDictionary(x => x.Key, x => x.AsEnumerable());
        }

        public static bool IsNullOrEmpty([NotNullWhen(false)] this string? data)
        {
            return string.IsNullOrEmpty(data);
        }

        public static bool HasValue([NotNullWhen(true)] this string? data)
        {
            return !string.IsNullOrEmpty(data);
        }
    }
}
