namespace Problem4
{
    public class Reel
    {
        public string Name { get; set; }
        public bool IsPosted { get; set; }
        public int Views { get; set; }

        public Reel(string name, bool isPosted, int views)
        {
            Name = name;
            IsPosted = isPosted;
            Views = views;
        }
    }

    class Instagram
    {
        private IList<Reel> Reels = new List<Reel>();


        public Instagram(IList<Reel> reels)
        {
            foreach(Reel reel in reels)
            {
                if (Reels.Contains(reel))
                {
                    throw new InvalidOperationException();
                }
                else
                {
                    Reels.Add(reel);
                }
            }
            
        }   

        public string ViewReel(string Name,bool Posted)
        {
            if (!Posted)
            {
                throw new InvalidOperationException();
            }

            bool flag = true;
            string message = "";

            foreach(Reel real in Reels)
            {
                if(real.Name == Name)
                {
                    flag = false;
                    if (!real.IsPosted)
                    {
                        message += "Not Posted yet";
                    }
                    else
                    {
                        message += $"{real.Views} views";
                    }

                    break;
                    
                }
            }

            if(flag)
            {
                message += "Not Present";
            }

            return message;
        }

        public string DeleteReel(string Name,bool Posted)
        {
            if (!Posted)
            {
                throw new InvalidOperationException();
            }

            bool flag = true;
            string message = "";
            
            foreach (Reel real in Reels)
            {
                if (real.Name == Name)
                {
                    flag = false;
                    Reels.Remove(real);
                    break;
                }
                
            }

            if (flag)
            {
                message += "Not Present";
            }

            return message;
        }
    }
    class Program
    {
        public static void Main(string[] args)
        {

            List<Reel> lst = new List<Reel>
            {
                new Reel(name : "1", isPosted : true, views : 100),
                new Reel(name : "2", isPosted : false, views : 0),
                new Reel(name : "3", isPosted : false, views : 0)
            };


            var inst = new Instagram(lst);

            Console.WriteLine(inst.DeleteReel("Kachha Badam",true));

        }
    }
}