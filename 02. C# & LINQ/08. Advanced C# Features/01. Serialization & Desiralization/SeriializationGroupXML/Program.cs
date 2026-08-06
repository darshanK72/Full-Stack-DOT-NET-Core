using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace SerializableGroup
{
    [Serializable]
    public class Person
    {
        public string Name { get; set; }
        public string Institute { get; set; }
        public string Occupation { get; set; }
        public int YearofBirth { get; set; }

        public Person()
        {

        }

        Person(string name, string institute, string occupation, int yearofBirth)
        {
            Name = name;
            Institute = institute;
            Occupation = occupation;
            YearofBirth = yearofBirth;
        }
    }
    class Program
    {
        public static void Main(string[] args)
        {

            List<Person> list = new List<Person>()
            {
                new Person(){Name="Darshan",Institute = "DYP",Occupation = "Engineer",YearofBirth=2000},
                new Person(){Name="Suyesh",Institute = "KKW",Occupation = "Engineer",YearofBirth=1999},
                new Person(){Name="Dipak",Institute = "DYP",Occupation = "Engineer",YearofBirth=2001},
                new Person(){Name="Parth",Institute = "DYP",Occupation = "Engineer",YearofBirth=2002},
                new Person(){Name="Suyog",Institute = "DYP",Occupation = "Engineer",YearofBirth=1998}

            };


            List<Person> newList = new List<Person>();

            foreach (var person in list)
            {
                if (person.YearofBirth >= 1999)
                {
                    newList.Add(person);
                }
            }


            Ser(newList);

            List<Person> returnedList = Deser();


            foreach (var person in returnedList)
            {
                Console.WriteLine(person.Name + " " + person.Institute);
            }

        }

        public static void Ser(List<Person> list)
        {
            FileStream stream = new FileStream("Persons.xml", FileMode.Create, FileAccess.Write);

            XmlSerializer ser = new XmlSerializer(typeof(List<Person>));

            ser.Serialize(stream, list);
            
            Console.WriteLine("Object Serilized!!");

            stream.Close();

        }

        public static List<Person> Deser()
        {
            FileStream stream = new FileStream("Persons.txt", FileMode.Open, FileAccess.Read);

            XmlSerializer ser = new XmlSerializer(typeof(List<Person>));

            List<Person> list = (List<Person>)ser.Deserialize(stream);

            Console.WriteLine("Object Deserilized");

            stream.Close();

            return list;

        }

    }
}