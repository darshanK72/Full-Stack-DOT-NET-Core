namespace Practice1
{

    class Program
    {
        public static void Main(string[] args)
        {
            int[] arr1 = new int[] { 52, 672, 90, 92, 32, 653, 7, 71, 4, 7, 74, 5, 7, 84, 378, 8, 4, 5, 3, 4, 747, 2, 3, 2, 34, 4, 24 };


            // Query Syntex

            /*var output = from i in arr1
                         where i >= 40
                         select i;*/

            /*var output2 = from i in arr1
                          where i % 2 == 0
                          select i;*/


            // Method Syntax

            var output1 = arr1.Where(i => i >= 40); // Numbers greater than 40

            var output2 = arr1.Where(i => i % 2 == 0); // Even Numbers


            foreach (int i in output2)
            {
                Console.Write(i + " ");
            }

            Console.WriteLine(arr1.Max());
            Console.WriteLine(arr1.Min());
            Console.WriteLine(arr1.Average());
            Console.WriteLine(arr1.Sum());
            Console.WriteLine(arr1.Count());
            Console.WriteLine(arr1.Contains(25));

            var product = arr1.Aggregate((a, b) => (a * b) % 20000);
            Console.WriteLine("Product of all elelemts  : " + product);

            bool isAllSquares = arr1.All(ele => Math.Sqrt(ele) * Math.Sqrt(ele) == ele);
            bool isAnySquare = arr1.Any(ele => Math.Sqrt(ele) * Math.Sqrt(ele) == ele);

            Console.WriteLine($"Is All Square : {isAllSquares}\nIs Any Square :  {isAnySquare}");

        }
    }
}