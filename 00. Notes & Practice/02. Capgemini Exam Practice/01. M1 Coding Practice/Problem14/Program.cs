using System.Reflection;

namespace Problem14
{
    interface InterfaceA
    {
        void InterfaceMethod();
    }

    class Sample : InterfaceA
    {
        public Sample()
        {

        }

        public void InterfaceMethod()
        {
            Console.WriteLine("Printed from Sample class");

        }
    }

    class Verification
    {
        Type type = typeof(Sample);
        public bool CheckInterfaceImplemented(string interfaceName)
        {
            var inf = this.type.GetInterface(interfaceName);
            if(inf != null)
            {
                return true ;
            }
            return false;
 
        }

        public bool CheckIfConstructorExists(string constructorName)
        {
            ConstructorInfo[] cinfo = this.type.GetConstructors();

            foreach(ConstructorInfo c in cinfo)
            {
                if (c.ToString() == constructorName)
                {
                    return true;
                }
            }

            return false;

        }

        public bool CheckIfMethodExists(string methodName)
        {
            MethodInfo info = type.GetMethod(methodName);
            if(info != null)
            {
                return true;
            }
            return false;
        }
    }
    class Program
    {
        public static void Main(string[] args)
        {
            Verification verification = new Verification();

            bool resultc = verification.CheckIfConstructorExists("Void .ctor()");
            bool resulti = verification.CheckInterfaceImplemented("InterfaceA");
            bool resultm = verification.CheckIfMethodExists("InterfaceMethod");

            Console.WriteLine(resultc + "\n" + resulti + "\n" + resultm);


        }
    }
}