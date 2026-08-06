using Newtonsoft.Json;
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
            string fileName = "Persons.json";

            string jsonformat = JsonConvert.SerializeObject(list);

            File.WriteAllText(fileName, jsonformat);    

            
            Console.WriteLine("Object Serilized!!");

        }

        public static List<Person> Deser()
        {
            string fileName = "Persons.json";
            string jsonFormat = File.ReadAllText(fileName);

            List<Person> list = JsonConvert.DeserializeObject<List<Person>>(jsonFormat);

            Console.WriteLine("Object Deserilized");;

            return list;

        }

    }
}