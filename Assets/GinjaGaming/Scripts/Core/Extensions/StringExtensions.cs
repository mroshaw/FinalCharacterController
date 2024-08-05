using System;
using System.Text;
using System.Text.RegularExpressions;

namespace GinjaGaming.Core.Extensions
{
    public static class StringExtensions
    {
        public static bool Contains(this string originalString, string toCheck, StringComparison comp)
        {
            return originalString?.IndexOf(toCheck, comp) >= 0;
        }

        public static string Replace(this string originalString, string oldValue, string newValue, StringComparison comparison)
        {
            StringBuilder newStringBuilder = new StringBuilder();

            int previousIndex = 0;
            int index = originalString.IndexOf(oldValue, comparison);
            while (index != -1)
            {
                newStringBuilder.Append(originalString.Substring(previousIndex, index - previousIndex));
                newStringBuilder.Append(newValue);
                index += oldValue.Length;

                previousIndex = index;
                index = originalString.IndexOf(oldValue, index, comparison);
            }
            newStringBuilder.Append(originalString.Substring(previousIndex));

            return newStringBuilder.ToString();
        }

        public static string RemoveString(this string originalString, string stringToRemove)
        {
            int assetIndex = originalString.IndexOf(stringToRemove, StringComparison.Ordinal);
            string newString = assetIndex < 0
                ? originalString
                : originalString.Remove(assetIndex, stringToRemove.Length);

            return newString;
        }

        public static string RemoveWhiteSpace(this string originalString)
        {
            return Regex.Replace(originalString, @"\s+", "");
        }

        public static string AddSpacesCapitalised(this string originalString)
        {
            return Regex.Replace(originalString, @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0");
        }

        public static string FirstCharToLowerCase(this string originalString)
        {
            if (!string.IsNullOrEmpty(originalString) && char.IsUpper(originalString[0]))
            {
                string newString = originalString.Length == 1 ? char.ToLower(originalString[0]).ToString() : char.ToLower(originalString[0]) + originalString[1..];
                return newString;
            }
            return originalString;
        }
    }
}