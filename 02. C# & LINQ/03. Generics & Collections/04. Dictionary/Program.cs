namespace DictionaryDemo;

public class KeyAlreadyPresentException : ApplicationException
{
    public KeyAlreadyPresentException(string message) : base(message)
    {
    }
}

public class Program
{
    public static Dictionary<string, string> FileList = new Dictionary<string, string>();

    public static void Main(string[] args)
    {
        while (true)
        {
            Console.Write("Enter Your Option\n1. Add File\n2. Show All Files\nYour Choice : ");
            int choice = int.Parse(Console.ReadLine()!);
            Console.WriteLine();

            switch (choice)
            {
                case 1:
                    AddFile();
                    break;
                case 2:
                    ShowAllFiles();
                    break;
                default:
                    Console.WriteLine("Incorrect Choice ! Try Again.");
                    break;
            }

            Console.Write("Do You Want to Continue Yes(y)/No(n) : ");
            string? temp = Console.ReadLine();
            Console.WriteLine();

            if (temp == "No" || temp == "no" || temp == "n")
            {
                Console.WriteLine("Thank You For Using Application!!");
                break;
            }
        }
    }

    public static void AddFile()
    {
        try
        {
            Console.Write("Enter Name Of File : ");
            string fileName = Console.ReadLine()!;
            Console.Write("Enter File Extension : ");
            string fileExtension = Console.ReadLine()!;
            fileExtension = "." + fileExtension.TrimStart('.');

            if (FileList.ContainsKey(fileName))
            {
                throw new KeyAlreadyPresentException($"File With {fileName} Already Exists !!");
            }

            FileList.Add(fileName, fileExtension);
        }
        catch (KeyAlreadyPresentException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public static void ShowAllFiles()
    {
        Console.WriteLine("-------------- List of Files ---------------");
        Console.WriteLine();

        foreach (var file in FileList)
        {
            Console.WriteLine(file.Key + file.Value);
        }

        Console.WriteLine("------------------ END ---------------------");
    }
}
