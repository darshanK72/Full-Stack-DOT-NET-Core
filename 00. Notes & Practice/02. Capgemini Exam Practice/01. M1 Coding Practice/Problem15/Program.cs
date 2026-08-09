namespace Problem15
{
    interface IPrice
    {
        double CalculatePrice(int units);

    }

    class Ingridient
    {
        private double _coffeePrice = 15;
        private double _sugarPrice = 5;
        private int _coffeQuantity;
        private int _sugarQuantity;

        public Ingridient(int coffeQuantity, int sugarQuantity)
        {
            this._coffeQuantity = coffeQuantity;
            this._sugarQuantity = sugarQuantity;
        }

        public double IngredientsAmount()
        {
            return this._coffeePrice * _coffeQuantity + this._sugarPrice * this._sugarQuantity;
        }
    }

    class Latte : IPrice
    {
        private int _coffeQuantity = 2;
        private int _sugarQuantity = 2;

        public double CalculatePrice(int quantity)
        {
            Ingridient ingridient = new Ingridient(this._coffeQuantity, this._sugarQuantity);

            return quantity * ingridient.IngredientsAmount();

        }
    }

    class Black : IPrice
    {
        private int _coffeQuantity = 4;
        private int _sugarQuantity = 2;

        public double CalculatePrice(int quantity)
        {
            Ingridient ingridient = new Ingridient(this._coffeQuantity, this._sugarQuantity);

            return quantity * ingridient.IngredientsAmount();
        }

    }

    class PaperBill
    {
        public double CalculateBeveragePrice(Func<int, double> calculatePrice, int quantity)
        {
            return calculatePrice(quantity);

        }
    }
    class Program
    {
        public static void Main(string[] args)
        {
            Func<int, double> calculatePrice = new Latte().CalculatePrice;
            var amount = new PaperBill().CalculateBeveragePrice(calculatePrice, 10);
            Console.WriteLine($"For 10 Latte Coffee : {amount}");

            calculatePrice = new Black().CalculatePrice;
            amount = new PaperBill().CalculateBeveragePrice(calculatePrice, 5);
            Console.WriteLine($"For 5 Black Coffee : {amount}");
        }
    }
}