namespace Problem2
{
    class ExceptionZero : Exception
    {
        public ExceptionZero(string message) : base(message)
        {

        }
    }

    class ExceptionOne : Exception
    {
        public ExceptionOne(string message) : base(message)
        {

        }
    }
    internal class Counter
    {
        public int[] data;

        public Counter(int[] data)
        {
            this.data = data;
        }

        public string getCount()
        {
            string message = "";
            try
            {
                int czero = 0;
                int cone = 0;

                foreach (int item in data)
                {
                    if (item == 0)
                    {
                        czero++;
                    }
                    else if (item == 1)
                    {
                        cone++;
                    }

                }

                if (czero % 2 == 0 && cone % 2 == 0)
                {
                    message += "Greate";
                }
                else if (czero % 2 == 1 && cone % 2 == 1)
                {
                    message += "Greate";
                }
                else if (czero % 2 == 0 && cone % 2 == 1)
                {
                    throw new ExceptionOne("One comes odd times");
                }
                else if (czero % 2 == 1 && cone % 2 == 0)
                {
                    throw new ExceptionZero("Zero comes odd times");
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return message;

        }


    }
    class Program
    {
        public static void Main(string[] args)
        {
            int[] data = { 0, 1, 0, 0, 0, 0,1, 1, 1 };
            Counter counter = new Counter(data);
            Console.WriteLine(counter.getCount());
           

        }
    }
}