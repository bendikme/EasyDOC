using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EasyDOC.Utilities
{
    public class EasyUtilities
    {
        /// <summary>
        /// Transform a string "item=43&..." like into [43,...]
        /// </summary>
        /// <param name="sort">A serialized string from</param>
        /// <returns>An array of identifiers</returns>
        public static List<int> SerializedOrderToArray(string sort)
        {
            return sort.Split('&').Select(part => part.Split('=')[1]).Select(int.Parse).Distinct().ToList();
        }

        public static bool IsAbsoluteUrl(string url)
        {
            Uri result;
            return Uri.TryCreate(url, UriKind.Absolute, out result);
        }

        public static string SlugifyTitle(string phrase, bool lowercase = true)
        {
            if (lowercase)
            {
                phrase = phrase.ToLower();
            }

            var str = RemoveAccent(phrase);

            // invalid chars           
            str = Regex.Replace(str, @"[^_a-zA-Z0-9\s-]", "");
            // convert multiple spaces into one space   
            str = Regex.Replace(str, @"\s+", " ").Trim();
            // cut and trim 
            str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
            str = Regex.Replace(str, @"\s", "-"); // hyphens   
            return str;
        }

        public static bool IsImageFile(string type)
        {
            var supportedTypes = new[] { "jpg", "jpeg", "gif", "png" };
            return supportedTypes.Contains(type);
        }


        private static string RemoveAccent(string txt)
        {
            var bytes = System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(txt);
            return System.Text.Encoding.ASCII.GetString(bytes);
        }

        public static string SlugifyTitleKeepUppercase(string name)
        {
            return SlugifyTitle(name, false);
        }
    }
}