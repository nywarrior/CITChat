using System;
using System.Globalization;

namespace CITChat.Translators
{
    /// <summary>
    /// </summary>
    public static class DoubleToWordHelper
    {
        /// <summary>
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static string DoubleToWords(double n)
        {
            double intPart;
            double decPart = 0;
            if (n == 0)
            {
                return "zero";
            }
            try
            {
                string[] splitter = n.ToString(CultureInfo.InvariantCulture).Split('.');
                if (splitter.Length > 1)
                {
                    intPart = double.Parse(splitter[0]);
                    decPart = double.Parse(splitter[1]);
                }
                else
                {
                    intPart = n;
                }
            }
            catch
            {
                intPart = n;
            }
            string words = SimpleDoubleToWords(intPart);
            if (decPart > 0)
            {
                if (words != "")
                {
                    words += " and ";
                }
                int counter = decPart.ToString(CultureInfo.InvariantCulture).Length;
                switch (counter)
                {
                    case 1:
                        words += SimpleDoubleToWords(decPart) + " tenths";
                        break;
                    case 2:
                        words += SimpleDoubleToWords(decPart) + " hundredths";
                        break;
                    case 3:
                        words += SimpleDoubleToWords(decPart) + " thousandths";
                        break;
                    case 4:
                        words += SimpleDoubleToWords(decPart) + " ten-thousandths";
                        break;
                    case 5:
                        words += SimpleDoubleToWords(decPart) + " hundred-thousandths";
                        break;
                    case 6:
                        words += SimpleDoubleToWords(decPart) + " millionths";
                        break;
                    case 7:
                        words += SimpleDoubleToWords(decPart) + " ten-millionths";
                        break;
                }
            }
            return words;
        }

        private static string SimpleDoubleToWords(double n) //converts double to words
        {
            string[] numbersArr = new[]
                {
                    "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve",
                    "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen"
                };
            string[] tensArr = new[] {"twenty", "thirty", "fourty", "fifty", "sixty", "seventy", "eighty", "ninty"};
            string[] suffixesArr = new[]
                {
                    "thousand", "million", "billion", "trillion", "quadrillion", "quintillion", "sextillion", "septillion",
                    "octillion", "nonillion", "decillion", "undecillion", "duodecillion", "tredecillion",
                    "Quattuordecillion", "Quindecillion", "Sexdecillion", "Septdecillion", "Octodecillion",
                    "Novemdecillion", "Vigintillion"
                };
            string words = "";

            bool tens = false;
            if (n < 0)
            {
                words += "negative ";
                n *= -1;
            }
            int power = (suffixesArr.Length + 1)*3;
            while (power > 3)
            {
                double pow = Math.Pow(10, power);
                if (n >= pow)
                {
                    if (n%pow > 0)
                    {
                        words += SimpleDoubleToWords(Math.Floor(n/pow)) + " " + suffixesArr[(power/3) - 1] + ", ";
                    }
                    else if (n%pow == 0)
                    {
                        words += SimpleDoubleToWords(Math.Floor(n/pow)) + " " + suffixesArr[(power/3) - 1];
                    }
                    n %= pow;
                }
                power -= 3;
            }
            if (n >= 1000)
            {
                if (n%1000 > 0) words += SimpleDoubleToWords(Math.Floor(n/1000)) + " thousand, ";
                else words += SimpleDoubleToWords(Math.Floor(n/1000)) + " thousand";
                n %= 1000;
            }
            if (0 <= n && n <= 999)
            {
                if ((int) n/100 > 0)
                {
                    words += SimpleDoubleToWords(Math.Floor(n/100)) + " hundred";
                    n %= 100;
                }
                if ((int) n/10 > 1)
                {
                    if (words != "")
                        words += " ";
                    words += tensArr[(int) n/10 - 2];
                    tens = true;
                    n %= 10;
                }
                if (n < 20 && n > 0)
                {
                    if (words != "" && !tens)
                    {
                        words += " ";
                    }
                    words += (tens ? "-" + numbersArr[(int) n - 1] : numbersArr[(int) n - 1]);
                    n -= Math.Floor(n);
                }
            }
            return words;
        }
    }
}