using System;
using System.Globalization;

namespace Scratch
{
    class Program
    {
        static void Main(string[] args)
        {
            var culture = new CultureInfo("vi-VN");
            
            try { Console.WriteLine("1.5 -> " + decimal.Parse("1.5", culture)); } catch (Exception e) { Console.WriteLine(e.Message); }
            try { Console.WriteLine("12.5 -> " + decimal.Parse("12.5", culture)); } catch (Exception e) { Console.WriteLine(e.Message); }
            try { Console.WriteLine("100.5 -> " + decimal.Parse("100.5", culture)); } catch (Exception e) { Console.WriteLine(e.Message); }
            try { Console.WriteLine("1,5 -> " + decimal.Parse("1,5", culture)); } catch (Exception e) { Console.WriteLine(e.Message); }
        }
    }
}
