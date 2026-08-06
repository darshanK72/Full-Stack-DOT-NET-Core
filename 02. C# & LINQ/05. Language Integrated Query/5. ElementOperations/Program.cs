namespace ElementOperations
{
    class Program
    {
        public static void Main(string[] args)
        {
            int[] arr1 = new int[] { 52, 672, 90, 92, 32, 653, 7, 71, 4, 7, 74, 5, 7, 84, 378, 8, 4, 5, 3, 4, 747, 2, 3, 2, 34, 4, 24 };


            int eleModSevenFirst = arr1.First(ele => ele % 7 == 0);
            int eleModSevenLast = arr1.Last(ele => ele % 7 == 0);

            Console.WriteLine("First Element Divisible by Seven : " + eleModSevenFirst);
            Console.WriteLine("Last Element Divisible by Seven : " + eleModSevenLast);

            int eleModHundredFirst = arr1.FirstOrDefault(ele => ele % 100 == 0);
            int eleModHundredLast = arr1.LastOrDefault(ele => ele % 100 == 0);

            Console.WriteLine("First Element Divisible by Hundred : "  + eleModHundredFirst);
            Console.WriteLine("Last Element Divisible by Hundred : " + eleModHundredLast);


            Console.WriteLine("Element At Index 3 : " + arr1.ElementAt(3));
            Console.WriteLine("Element At Index 30 : " + arr1.ElementAtOrDefault(30));

            Console.WriteLine("Only Element in arr1 : " + arr1.Single(ele => ele == 71));

            Console.WriteLine("Only Element in arr1 : " + arr1.SingleOrDefault(ele => ele == 30000));



        }
    }
}