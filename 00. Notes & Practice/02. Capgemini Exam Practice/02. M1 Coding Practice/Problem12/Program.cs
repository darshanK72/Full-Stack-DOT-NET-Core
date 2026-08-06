namespace Problem12
{
    enum Karat { Karat18,Karat20};

    interface IComodity
    {
        double CalculatePrice(Karat karat, int quantity);
    }

    class Gold : IComodity
    {
        private const double Price = 40000;

        public double CalculatePrice(Karat karat,int quantity)
        {
            if(karat == null)
            {
                throw new ArgumentOutOfRangeException("Karat is not give");
            }
            else
            {
                if(karat == Karat.Karat18)
                {
                    return Price * 1.1 * quantity;
                }
                else
                {
                    return Price * 1.2 * quantity;
                }
            }
        }

    }

    class Diamond : IComodity
    {
        private const double Price = 60000;

        public double CalculatePrice(Karat karat, int quantity)
        {
            if (karat == null)
            {
                throw new ArgumentOutOfRangeException("Karat is not give");
            }
            else
            {
                if (karat == Karat.Karat18)
                {
                    return Price * 1.2 * quantity;
                }
                else
                {
                    return Price * 1.3 * quantity;
                }
            }
        }

    }

    class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var calc = new Gold().CalculatePrice(Karat.Karat18, 2);
                Console.WriteLine(calc);

                calc = new Diamond().CalculatePrice(Karat.Karat20, 3);
                Console.WriteLine(calc);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
           

        }
    }
}