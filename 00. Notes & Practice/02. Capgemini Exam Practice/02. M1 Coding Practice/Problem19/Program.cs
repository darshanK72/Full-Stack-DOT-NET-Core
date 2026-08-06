using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Linq;

namespace Problem19
{
    [Serializable]
    class Team
    {
        public string teamName { get; set; }
        public string color { get; set; }
        public int memberCount { get; set; }

        public Team()
        {

        }
        public Team(string teamName,string color,int memberCount)
        {
            this.teamName = teamName;
            this.color = color;
            this.memberCount = memberCount;
        }

        public string Encrypt(ArrayList teams)
        {
            FileStream fileStream = new FileStream("team.txt", FileMode.Create, FileAccess.Write);
            BinaryFormatter bf = new BinaryFormatter();


            /*var newteams = from t in teams.ToArray()
                           where ((Team)t).memberCount > 13
                           select t;*/




            ArrayList arrayList = new ArrayList();
            foreach (Team team in teams)
            {
                if (team.memberCount >= 13)
                {
                    arrayList.Add(team);
                }
            }

            bf.Serialize(fileStream, arrayList);

            fileStream.Close();

            return "Secret";
        }

        public ArrayList Decrypt()
        {
            FileStream fileStream = new FileStream("team.txt", FileMode.Open, FileAccess.Read);

            
            
            BinaryFormatter bf = new BinaryFormatter();

            ArrayList arr  = (ArrayList)bf.Deserialize(fileStream);

            fileStream.Close();

            return arr;
        }

    }
    class Program
    {
        public static void Main(string[] args)
        {
            
            ArrayList arr = new ArrayList()
            {
                new Team("1","Blue",24),
                new Team("2","Pink",12),
                new Team("3","Red",10),
                new Team("4","Green",21),
                new Team("5", "Yellow", 13)
            };

            /*List<Team> newList = new List<Team>()
            {
                new Team("1","Blue",24),
                new Team("2","Pink",12),
                new Team("3","Red",10),
                new Team("4","Green",21),
                new Team("5", "Yellow", 13)
            };

            newList.FindAll((e) => e.memberCount > 13);

            newList.ForEach((e) => { Console.WriteLine("Hello World"); });*/

            Team team = new Team();

            Console.WriteLine(team.Encrypt(arr));

            ArrayList arr2 = team.Decrypt();

            foreach(Team t in arr2)
            {
                Console.WriteLine($"{t.teamName} : {t.memberCount}");
            }
            




        }
    }
}