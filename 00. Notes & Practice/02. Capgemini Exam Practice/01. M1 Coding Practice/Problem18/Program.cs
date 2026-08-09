using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Problem18
{
    public class Calculator
    {
        public int int1 { get; set; }
        public int int2 { get; set; }
        public string str1 { get; set; }
        public string str2 { get; set; }

        public Calculator(int int1, int int2)
        {
            this.int1 = int1;
            this.int2 = int2;
        }

        public Calculator(string str1,string str2)
        {
            this.str1 = str1;
            this.str2 = str2;
        }

        public int numbeSum()
        {
            return this.int1 + this.int2;
        }

        public string stringSum()
        {
            return this.str1 + this.str2;
        }

    }

    class CalculatorCheck
    {
        public string constructorInfo(Type type)
        {
            ConstructorInfo[] constructors =  type.GetConstructors();
            string output = "";
            output += constructors[0].ToString();

            foreach(ConstructorInfo cinfo in constructors)
            {
                output += "/";
                output += cinfo;
            }
            return output;
        }

        public string datainfo(Type type)
        {
            PropertyInfo[] properties = type.GetProperties();
            string output = "";
            output += properties[0].ToString();
            foreach(PropertyInfo pinfo in properties)
            {
                output += "/";
                output += pinfo;
            }

            return output;
        }
    }
    class Program
    {
        public static void Main(string[] args)
        {
            Type type = typeof(Calculator);

            CalculatorCheck calc = new CalculatorCheck();

            Console.WriteLine(calc.constructorInfo(type));
            Console.WriteLine(calc.datainfo(type));

        }
    }
}