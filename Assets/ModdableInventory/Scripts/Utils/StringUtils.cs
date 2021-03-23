using System.Globalization;

namespace ModdableInventory.Utils
{
    public static class StringUtils
    {
        public static string FloatToString(float number, int precision)
        {
            return number.ToString($"F{precision.ToString()}", CultureInfo.InvariantCulture);
        }

        public static string NoSpacesAndLowerCaseString(string str)
        {
            return str.Replace(" ", "").ToLower();
        }

        public static bool StringContainsName(string str, string name, bool ignoreSpacesAndCasing = true)
        {
            if (ignoreSpacesAndCasing)
            {
                str = StringUtils.NoSpacesAndLowerCaseString(str);
                name = StringUtils.NoSpacesAndLowerCaseString(name);
            }

            return str.Contains(name);
        }
    }
}