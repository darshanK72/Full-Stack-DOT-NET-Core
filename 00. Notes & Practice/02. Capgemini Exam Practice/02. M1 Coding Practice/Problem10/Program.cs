using System.Collections;

namespace Problem10
{
    class Manipulator
    {
        ArrayList teams = new ArrayList();

        public Manipulator(ArrayList teams)
        {
            this.teams = teams;
        }

        public string filtreCount(Boolean isEven)
        {
            if (isEven)
            {
                for(int i = 0; i < teams.Count; i++)
                {
                    if(i%2 == 0)
                    {
                        if ((int)teams[i] % 2 != 0)
                        {
                            throw new BadDataException("Even Error");
                        }
                    }
                }

                return "Happy Team";

            }
            else
            {
                for (int i = 0; i < teams.Count; i++)
                {
                    if (i % 2 != 0)
                    {
                        if ((int)teams[i] % 2 == 0)
                        {
                            throw new BadDataException("Odd Error");
                        }
                    }
                }
                return "Happy Team";

            }
        }


    }

    class BadDataException: Exception
    {
        public BadDataException(string message): base(message)
        {

        }
    }

    class Program
    {
        public static void Main(string[] args)
        {
            ArrayList teams = new ArrayList();
            teams.Add(3);
            teams.Add(4);
            teams.Add(2);
            teams.Add(3);
            teams.Add(4);

            Manipulator manipulator = new Manipulator(teams);

            try
            {
                Console.WriteLine(manipulator.filtreCount(false));
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


        }
    }
}