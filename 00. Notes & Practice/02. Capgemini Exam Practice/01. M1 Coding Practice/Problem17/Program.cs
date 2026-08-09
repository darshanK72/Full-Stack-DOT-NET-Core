namespace Problem17
{
    delegate string World(string x, string y);
    class Hello
    {
        public static string compCountry(string Country1,string Country2)
        {
            if(Country1.Length > Country2.Length)
            {
                return Country1;
            }
            else if(Country1.Length < Country2.Length)
            {
                return Country2;
            }
            else
            {
                return "Both countries are of equal Length";
            }
        }

        public static string concatCountry(string Country1,string Country2)
        {
            return Country1.Substring(0, Country1.Length / 2) + Country2.Substring(Country2.Length / 2);
        }
    }
    class Program
    {
        public static void Main(string[] args)
        {
            World obj1 = new World(Hello.compCountry);
            World obj2 = new World(Hello.concatCountry);

            Console.WriteLine(obj1("USA", "India"));
            Console.WriteLine(obj2("USA", "India"));;


        }
    }
}