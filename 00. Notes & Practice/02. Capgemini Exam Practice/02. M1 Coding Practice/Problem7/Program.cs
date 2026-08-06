using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;

namespace Problem7
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

            Ser(list);

            List<Person> list2 = Deser();

            foreach(Person p in list2)
            {
                Console.WriteLine($"{p.Name} : {p.YearofBirth}");
            }

        }

        public static void Ser(List<Person> people)
        {
            FileStream fileStream = new FileStream("Persons.txt", FileMode.Create, FileAccess.Write);

            BinaryFormatter formatter = new BinaryFormatter();


            /*var newList = from person in people
                          where person.YearofBirth >= 1999
                          select person;
*/  
            List<Person> newList = people.FindAll((person) => person.YearofBirth >= 1999);

            formatter.Serialize(fileStream, newList);

            fileStream.Close();

        }

        public static List<Person> Deser()
        {

            FileStream fileStream = new FileStream("Persons.txt", FileMode.Open, FileAccess.Read);

            BinaryFormatter formatter = new BinaryFormatter();

            List<Person> list = (List<Person>)formatter.Deserialize(fileStream);

            return list;

        }

    }


}