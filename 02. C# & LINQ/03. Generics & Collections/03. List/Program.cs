namespace ListDemo;

public class Contact
{
    public int? ContactId { get; set; }
    public string? ContactName { get; set; }
    public string? CellNo { get; set; }
}

public class Program
{
    public static List<Contact> ContactList = new List<Contact>();

    public static void Main(string[] args)
    {
        Console.WriteLine("------------- Contact Manager Application ------------------\n");
        while (true)
        {
            Console.Write("Enter One of the Option\n1. Create Contact\n2. Show Contact\n3. Edit Contact\n4. Show All Contacts\nYour Choice : ");
            int choice = int.Parse(Console.ReadLine()!);

            Console.WriteLine();

            switch (choice)
            {
                case 1:
                    AddContact();
                    break;
                case 2:
                    DisplayContact();
                    break;
                case 3:
                    EditContact();
                    break;
                case 4:
                    ShowAllContacts();
                    break;
                default:
                    Console.WriteLine("Incorrect Choice !! Try Again.");
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

    public static void AddContact()
    {
        Contact contact = new Contact();
        Console.Write("Enter Contact No : ");
        contact.ContactId = int.Parse(Console.ReadLine()!);
        Console.Write("Enter Contact Name : ");
        contact.ContactName = Console.ReadLine();
        Console.Write("Enter Cell No : ");
        contact.CellNo = Console.ReadLine();

        ContactList.Add(contact);

        Console.WriteLine("Contact Added Successfully !!");
        Console.WriteLine();
    }

    public static void EditContact()
    {
        Console.Write("Enter Contact Id : ");
        int id = int.Parse(Console.ReadLine()!);
        bool flag = false;

        foreach (Contact contact in ContactList)
        {
            if (contact.ContactId == id)
            {
                flag = true;
                Console.Write("Enter New Contact Name : ");
                contact.ContactName = Console.ReadLine();
                Console.Write("Enter New Cell No : ");
                contact.CellNo = Console.ReadLine();
            }
        }

        if (flag)
        {
            Console.WriteLine("Contact Updated Successfully !!");
            Console.WriteLine();
        }
        else
        {
            Console.WriteLine("Contact Not Found !!");
            Console.WriteLine();
        }
    }

    public static void DisplayContact()
    {
        Console.Write("Enter Contact Id : ");
        int id = int.Parse(Console.ReadLine()!);
        bool flag = false;

        foreach (Contact contact in ContactList)
        {
            if (contact.ContactId == id)
            {
                flag = true;
                Console.WriteLine("Contact Id : " + contact.ContactId);
                Console.WriteLine("Contact Name : " + contact.ContactName);
                Console.WriteLine("Cell No : " + contact.CellNo);
                Console.WriteLine();
            }
        }

        if (!flag)
        {
            Console.WriteLine("Contact Not Found !!");
        }
    }

    public static void ShowAllContacts()
    {
        Console.WriteLine("Displaying All Contacts");
        Console.WriteLine("---------------------------------------");
        foreach (Contact contact in ContactList)
        {
            Console.WriteLine("Contact Id : " + contact.ContactId);
            Console.WriteLine("Contact Name : " + contact.ContactName);
            Console.WriteLine("Cell No : " + contact.CellNo);
            Console.WriteLine("------------------------------------------");
        }
    }
}
