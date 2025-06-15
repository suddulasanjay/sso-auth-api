namespace SSOAuthAPI.Utilities
{
    public class MiscUtility
    {

        private static Random _random = new Random();

        public static string RandomString(int length, string chars)
        {
            return new string(Enumerable.Repeat(chars, length).Select(s => s[_random.Next(s.Length)]).ToArray());
        }

        public static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[_random.Next(s.Length)]).ToArray());
        }

    }
}
