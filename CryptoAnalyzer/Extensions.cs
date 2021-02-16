using System;

namespace CryptoAnalyzer.Extensions
{
	public static class Extensions
	{
        public static decimal RoundXSignificantDigit(this decimal input, int x)
        {
            int stop = 1;
            if (x > 1) stop = (int) Math.Pow(10,x);
            int precision = 0;          
            if (input < 1 && input != 0)
            {
                var val = input;
                while (Math.Abs(val) < stop)
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
