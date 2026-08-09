namespace Problem1
{
     internal class Khata
    {
        public Dictionary<string, int> record = new Dictionary<string, int>();

        public Khata(Dictionary<string, int> record)
        {
            this.record = record;
        }

        public int getTotal()
        {
            int total = 0;
            foreach(KeyValuePair<string, int> kvp in this.record)
            {
                total += kvp.Value;
            }
            return total;
        }

        public int getRepeteAmount()
        {
            Dictionary<int,int> uniqueRecoards = new Dictionary<int,int>();
            foreach(KeyValuePair<string, int> kvp in record)
            {
               
                if(uniqueRecoards.ContainsKey(kvp.Value))
                {
                    uniqueRecoards[kvp.Value]++;
                }
                else
                {
                    uniqueRecoards[kvp.Value] = 1;
                }
            }
            return uniqueRecoards.Count;
        }
    }
    class Program
    {

        public static void Main(string[] args)
        {
            Dictionary<string,int> recoards = new Dictionary<string,int>();

            recoards.Add("Milk", 100);
            recoards.Add("Tea", 50);

            Khata khata = new Khata(recoards);

            Console.WriteLine(khata.getTotal());
            Console.WriteLine(khata.getRepeteAmount());
        }
    }
}