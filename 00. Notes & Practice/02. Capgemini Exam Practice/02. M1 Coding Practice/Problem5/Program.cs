namespace Problem5
{
    class InternetException : Exception
    {
        public InternetException(string message) : base(message) 
        { 
        }
    }
    class Internet
    {
        public string Name { get; set; }
        public int DataLimit { get; set; }
        public int Speed { get; set; }

        public Internet(string name, int dataLimit, int speed)
        {
            Name = name;
            DataLimit = dataLimit;
            Speed = speed;
        }

        public string DownloadMovie(int Size)
        {
            if(this.DataLimit < Size)
            {
                throw new InternetException("File too large");
            }
            if(this.Speed < 200)
            {
                throw new InternetException("Low Bandwidth");
            }

            int time = (Size * 1024) / Speed;
            if(time > 100)
            {
                throw new InternetException("Time exceeded");
            }

            return "Can be downloaded";
        }

        public string WatchMovie(int Size)
        {

            string message = "";
            try
            {
                this.DownloadMovie(Size);
            }
            catch(InternetException e)
            {
                return "Cannot be downloaded";
            }
            catch(Exception e)
            {
                return "Other Exception";
            }

            
            return "Watch listed";
        }
    }
   class Program
   {
        public static void Main(string[] args)
        {
            Internet obj = new Internet("The Paycheck", 200, 100);
            Console.WriteLine(obj.WatchMovie(200));

        }
   }
}