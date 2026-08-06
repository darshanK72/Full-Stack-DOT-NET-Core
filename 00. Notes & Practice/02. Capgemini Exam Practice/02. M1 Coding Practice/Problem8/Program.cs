namespace Problem8
{
    public delegate void Operation(string name);
    class Program
    {
        public static void Main(string[] args)
        {
            Program program = new Program();
            Operation operation = new Operation(program.NameLength);
            operation += new Operation(program.NumberOfWords);
            operation += new Operation(program.CheckDigits);
            operation += new Operation(program.CheckSymbols);

            operation.Invoke("Hello Word 235 this is my home, hwo 5 & 52346992 ^@ 19 %8*** @% $ # ! $$$$) hwo are you");

        }

        public void NameLength(string name)
        {
            Console.WriteLine($"Length : {name.Length}");
        }

        public void NumberOfWords(string name)
        {
            string[] words = name.Split(" ");
            Console.WriteLine("Number of Words : " + words.Length);
        }
        
        public void CheckDigits(string name)
        {
            int ints = 0;
            foreach(Char ch in name)
            {
                if (Char.IsDigit(ch))
                {
                    ints++;
                }
            }
            Console.WriteLine($"Number of Digits : {ints}");
        }

        public void CheckSymbols(string name)
        {
            int syms = 0;
            foreach(Char ch in name)
            {
                if (Char.IsSymbol(ch))
                {
                    Console.WriteLine(ch);
                    syms++;
                }

                Char.IsSymbol('+');
            }
            Console.WriteLine($"Number of Symbols : {syms}");
        }
    }
}