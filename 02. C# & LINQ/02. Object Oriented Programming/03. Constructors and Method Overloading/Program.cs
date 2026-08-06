namespace ConstructorsAndMethodOverloading;

public class Bird
{
    public string Name;
    public double? Maxheight;

    public Bird()
    {
        Name = "Mountain Eagle";
        Maxheight = 500;
    }

    public Bird(string birdname, double max_ht)
    {
        Name = birdname;
        Maxheight = max_ht;
    }

    public void fly()
    {
        Console.WriteLine($"{Name} is flying at altitude {Maxheight}");
    }

    public void fly(string AtHeight)
    {
        if (double.Parse(AtHeight) <= Maxheight)
            Console.WriteLine(Name + " flying at " + AtHeight);
        else
            Console.WriteLine($"{Name} cannot fly at this height");
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        Bird b = new Bird("Eagle", double.Parse("200"));

        b.fly();
        b.fly("300");
    }
}
