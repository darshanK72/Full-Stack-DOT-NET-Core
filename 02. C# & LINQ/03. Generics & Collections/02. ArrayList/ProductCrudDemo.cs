using System.Collections;

namespace ArrayListDemo;

public class Product
{
    public int ProductNo { get; set; }
    public string? ProductName { get; set; }
    public double? ProductPrice { get; set; }
}

public static class ProductCrudDemo
{
    public static ArrayList ProductList = new ArrayList();

    public static void Run()
    {
        Console.WriteLine("------------- Product CRUD Application ------------------\n");
        while (true)
        {
            Console.Write("Enter One of the Option\n1. Add Product\n2. Search Product\n3. Delete Product\n4. Show All Products\nYour Choice : ");
            int choice = int.Parse(Console.ReadLine()!);

            Console.WriteLine();

            switch (choice)
            {
                case 1:
                    AddProduct();
                    break;
                case 2:
                    SearchProduct();
                    break;
                case 3:
                    DeleteProduct();
                    break;
                case 4:
                    ShowAllProducts();
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

    public static void AddProduct()
    {
        Product product = new Product();
        Console.Write("Enter Product No : ");
        product.ProductNo = int.Parse(Console.ReadLine()!);
        Console.Write("Enter Product Name : ");
        product.ProductName = Console.ReadLine();
        Console.Write("Enter Product Price : ");
        product.ProductPrice = double.Parse(Console.ReadLine()!);

        int i = 0;

        if (ProductList.Count == 0)
        {
            ProductList.Add(product);
        }
        else
        {
            bool flag = false;
            foreach (Product prod in ProductList)
            {
                if (product.ProductNo < prod.ProductNo)
                {
                    flag = true;
                    ProductList.Insert(i++, product);
                    break;
                }
                i++;
            }

            if (!flag)
            {
                ProductList.Add(product);
            }
        }

        Console.WriteLine("Product Added Successfully !!");
        Console.WriteLine();
    }

    public static void SearchProduct()
    {
        Console.Write("Enter Product No : ");
        int id = int.Parse(Console.ReadLine()!);
        bool flag = false;

        foreach (Product prod in ProductList)
        {
            if (prod.ProductNo == id)
            {
                flag = true;
                Console.WriteLine("Product No : " + prod.ProductNo);
                Console.WriteLine("Product Name : " + prod.ProductName);
                Console.WriteLine("Price : " + prod.ProductPrice);
                break;
            }
        }

        if (!flag)
        {
            Console.WriteLine("Product Not Found !!");
            Console.WriteLine();
        }
    }

    public static void DeleteProduct()
    {
        Console.Write("Enter Product No : ");
        int id = int.Parse(Console.ReadLine()!);
        bool flag = false;

        int i = 0;
        foreach (Product prod in ProductList)
        {
            if (prod.ProductNo == id)
            {
                flag = true;
                ProductList.RemoveAt(i);
                break;
            }
            i++;
        }

        if (!flag)
        {
            Console.WriteLine("Product Not Found !!");
        }
    }

    public static void ShowAllProducts()
    {
        Console.WriteLine("Displaying All Products");
        Console.WriteLine("---------------------------------------");
        foreach (Product prod in ProductList)
        {
            Console.WriteLine("Product No : " + prod.ProductNo);
            Console.WriteLine("Product Name : " + prod.ProductName);
            Console.WriteLine("Price : " + prod.ProductPrice);
            Console.WriteLine("------------------------------------------");
        }
    }
}
