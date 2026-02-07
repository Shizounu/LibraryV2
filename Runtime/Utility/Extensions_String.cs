
namespace Shizounu.Library.Utility
{
    /// <summary>
    /// Utility to remove whitespaces from a string
    /// source: https://youtu.be/yBM112uokM8?si=tKdzzAOegxV2rMJU
    /// </summary>
    public static class TextUtility
    {

    
        public static bool IsWhitespace(this char character)
        {
            switch (character)
            {
                case '\u0020':
                case '\u00A0':
                case '\u1680':
                case '\u2000':
                case '\u2001':
                case '\u2002':
                case '\u2003':
                case '\u2004':
                case '\u2005':
                case '\u2006':
                case '\u2007':
                case '\u2008':
                case '\u2009':
                case '\u200A':
                case '\u202F':
                case '\u205F':
                case '\u3000':
                case '\u2028':
                case '\u2029':
                case '\u0009':
                case '\u000A':
                case '\u000B':
                case '\u000C':
                case '\u000D':
                case '\u0085':
                    {
                        return true;
                    }

                default:
                    {
                        return false;
                    }
            }
        }

        // While unnecessary for this project, I've used the method seen here: https://stackoverflow.com/a/37368176
        // Benchmarks: https://stackoverflow.com/a/37347881
        public static string RemoveWhitespaces(this string text)
        {
            int textLength = text.Length;

            char[] textCharacters = text.ToCharArray();

            int currentWhitespacelessTextLength = 0;

            for (int currentCharacterIndex = 0; currentCharacterIndex < textLength; ++currentCharacterIndex)
            {
                char currentTextCharacter = textCharacters[currentCharacterIndex];

                if (currentTextCharacter.IsWhitespace())
                {
                    continue;
                }

                textCharacters[currentWhitespacelessTextLength++] = currentTextCharacter;
            }

            return new string(textCharacters, 0, currentWhitespacelessTextLength);
        }

        // See here for alternatives: https://stackoverflow.com/questions/3210393/how-do-i-remove-all-non-alphanumeric-characters-from-a-string-except-dash
        public static string RemoveSpecialCharacters(this string text)
        {
            int textLength = text.Length;

            char[] textCharacters = text.ToCharArray();

            int currentWhitespacelessTextLength = 0;

            for (int currentCharacterIndex = 0; currentCharacterIndex < textLength; ++currentCharacterIndex)
            {
                char currentTextCharacter = textCharacters[currentCharacterIndex];

                if (!char.IsLetterOrDigit(currentTextCharacter) && !currentTextCharacter.IsWhitespace())
                {
                    continue;
                }

                textCharacters[currentWhitespacelessTextLength++] = currentTextCharacter;
            }

            return new string(textCharacters, 0, currentWhitespacelessTextLength);
        }

        /// <summary>
        /// Truncate string to maximum length with optional suffix.
        /// </summary>
        public static string Truncate(this string text, int maxLength, string suffix = "...")
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;

            return text[..(maxLength - suffix.Length)] + suffix;
        }

        /// <summary>
        /// Reverse the string.
        /// </summary>
        public static string Reverse(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            char[] charArray = text.ToCharArray();
            System.Array.Reverse(charArray);
            return new string(charArray);
        }

        /// <summary>
        /// Convert to title case.
        /// </summary>
        public static string ToTitleCase(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text.ToLower());
        }

        /// <summary>
        /// Check if string contains value with specified comparison.
        /// </summary>
        public static bool Contains(this string text, string value, System.StringComparison comparison)
        {
            return text?.IndexOf(value, comparison) >= 0;
        }

        /// <summary>
        /// Shorthand for string.IsNullOrEmpty.
        /// </summary>
        public static bool IsNullOrEmpty(this string text)
        {
            return string.IsNullOrEmpty(text);
        }

        /// <summary>
        /// Shorthand for string.IsNullOrWhiteSpace.
        /// </summary>
        public static bool IsNullOrWhiteSpace(this string text)
        {
            return string.IsNullOrWhiteSpace(text);
        }

        /// <summary>
        /// Repeat the string a specified number of times.
        /// </summary>
        public static string Repeat(this string text, int count)
        {
            if (string.IsNullOrEmpty(text) || count <= 0)
                return string.Empty;

            return string.Concat(System.Linq.Enumerable.Repeat(text, count));
        }
    }
}