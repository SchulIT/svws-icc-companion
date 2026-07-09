using System;

namespace SvwsIccImporter.Extensions
{
    public static class StringEx
    {
        public static string? Cleanup(this string? input)
        {
            var result = input?.Trim();

            if (string.IsNullOrEmpty(result))
            {
                return null;
            }

            return result;
        }

        public static DateTime? ToDateTime(this string? input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }

            if (DateTime.TryParse(input, out DateTime result))
            {

                return result;
            }


            return null;
        }
    }
}
