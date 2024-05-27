using System;

namespace Flow.Launcher.Plugin.ByteUnitConverter
{
    public class Main_Test
    {
        public static void Main()
        {
            // ulong maxInt 18446744073709551615
            // decimal maxInt 29229670614629174706176

            string sd = "29229670614629174706175.0";
            decimal.TryParse(sd, out var de1);
            Console.WriteLine(de1.ToString("N1"));
            var truncate = Math.Truncate(de1);

            var result = de1 - truncate;
            Console.WriteLine(result);
            Console.WriteLine(result == 0m);
        }
    }
}