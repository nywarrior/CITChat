using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace CITChat.Translators
{
    public class InflationaryEnglishTranslator : IContentTranslator
    {
        private const int MaxNumberValueToTranslate = 20;

        private const char IgnoreChar = '~';

        private static readonly IList<KeyValuePair<string, int>> AdditionalNumberWordToNumbers = new List
            <KeyValuePair<string, int>>
            {
                new KeyValuePair<string, int>("won", 1),
                new KeyValuePair<string, int>("too", 2),
                new KeyValuePair<string, int>("to", 2),
                new KeyValuePair<string, int>("fore", 4),
                new KeyValuePair<string, int>("for", 4),
                new KeyValuePair<string, int>("ate", 8),
            };

        /// <summary>
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public string TranslateContent(string content)
        {
            content = content.ToLowerInvariant();
            StringBuilder sb = new StringBuilder();
            string[] words = content.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            int index = 0;
            foreach (string word in words)
            {
                if (index > 0)
                {
                    sb.Append(" ");
                }
                string translatedWord = TranslateWord(word);
                sb.Append(translatedWord);
                index++;
            }
            string translatedContent = sb.ToString();
            return translatedContent;
        }

        private static string TranslateWord(string word)
        {
            word = IncrementDigits(word);
            word = IncrementLongWordsInRange(word, 0, MaxNumberValueToTranslate);
            foreach (KeyValuePair<string, int> keyValuePair in AdditionalNumberWordToNumbers)
            {
                string doubleWord = keyValuePair.Key;
                long number = keyValuePair.Value;
                word = IncrementLongWord(word, doubleWord, number);
            }
            word = RemoveIgnoreChars(word);
            return word;
        }

        private static string IncrementLongWordsInRange(string word, int startValue, int endValue)
        {
            for (long number = startValue; number < endValue; number++)
            {
                string doubleWord = DoubleToWordHelper.DoubleToWords(number);
                word = IncrementLongWord(word, doubleWord, number);
            }
            return word;
        }

        private static string RemoveIgnoreChars(string word)
        {
            string[] wordParts = word.Split(new[] {IgnoreChar}, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder stringBuilder2 = new StringBuilder();
            foreach (string wordPart in wordParts)
            {
                stringBuilder2.Append(wordPart);
            }
            word = stringBuilder2.ToString();
            return word;
        }

        private static string IncrementDigits(string word)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (char c in word)
            {
                int number;
                bool success = int.TryParse(c.ToString(CultureInfo.InvariantCulture), out number);
                if (success)
                {
                    stringBuilder.Append(number + 1);
                }
                else
                {
                    stringBuilder.Append(c);
                }
            }
            word = stringBuilder.ToString();
            return word;
        }

        private static string IncrementLongWord(string word, string longWord, long l)
        {
            int doubleWordIndex = word.IndexOf(longWord, StringComparison.Ordinal);
            if (doubleWordIndex >= 0)
            {
                string prefix = word.Substring(0, doubleWordIndex);
                string suffix = word.Substring(doubleWordIndex + longWord.Length);
                string incrementedDoubleWord = DoubleToWordHelper.DoubleToWords(l + 1);
                // Add an escape char before each char in the new word so we don't replace other numbers in it.
                StringBuilder stringBuilder = new StringBuilder();
                foreach (char c in incrementedDoubleWord)
                {
                    stringBuilder.Append(IgnoreChar);
                    stringBuilder.Append(c);
                }
                string newIncrementedDoubleWord = stringBuilder.ToString();
                word = prefix + newIncrementedDoubleWord + suffix;
            }
            return word;
        }
    }
}