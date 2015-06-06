using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CITChat.Translators
{
    public class ContentReverseTranslator : IContentTranslator
    {
        /// <summary>
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public string TranslateContent(string content)
        {
            StringBuilder sb = new StringBuilder();
            IEnumerable<char> reverseContentChars = content.Reverse();
            foreach (char c in reverseContentChars)
            {
                sb.Append(c);
            }
            string translatedContent = sb.ToString();
            return translatedContent;
        }
    }
}