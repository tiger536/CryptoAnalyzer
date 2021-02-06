using System;

namespace CryptoAnalyzer.Extensions
{
	public static class Extensions
	{
        public static decimal RoundThirdSignificantDigit(this decimal input)
        {
            int precision = 0;          
            if (input < 1)
            {
                var val = input;
                while (Math.Abs(val) < 100)
                {
                    val *= 10;
                    precision++;
                }
            }
            else
                precision = 2;

            return Math.Round(input, precision);
        }
    }
}
