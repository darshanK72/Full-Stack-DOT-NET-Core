using System.Reflection;

namespace Problem6
{
    class Data
    {
        public String Name { get; set; }
        public int Size { get; set; }
        public double Speed { get; set; }
        public String Time { get;set; }

        public Data(string Name)
        {
            this.Name = Name;
        }

        public Data(string Name,int Size)
        {
            this.Name = Name;
            this.Size = Size;
        }

        public Data(string Name,int Size,double Speed)
        {
            this.Name = Name;
            this.Size = Size;
            this.Speed = Speed;
        }

        public Data(string name, int size, double speed, string time) : this(name, size, speed)
        {
            Time = time;
        }
    }
    class Program
    {
        public string GetConstructorInfo(Type type)
        {
            string output = "";
            ConstructorInfo[] cinfo = type.GetConstructors();

            output += cinfo[0].ToString();
            
            for(int i = 1; i < cinfo.Length; i++)
            {
                output += ", ";
                output += cinfo[i].ToString();
            }

            return output;
            
        }

        public string GetTypeInfo(Type type)
        {
            string output = "";
            PropertyInfo[] cinfo = type.GetProperties();

            output += cinfo[0].ToString();

            for (int i = 1; i < cinfo.Length; i++)
            {
                output += ", ";
                output += cinfo[i].ToString();
            }

            return output;
        }
        public static void Main(string[] args)
        {
            Type type = typeof(Data);
            Program p = new Program();
            var constructorList = p.GetConstructorInfo(type);
            Console.WriteLine(constructorList);

        }
    }
}