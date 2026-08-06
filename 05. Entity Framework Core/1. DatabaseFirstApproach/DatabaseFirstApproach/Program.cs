using DatabaseFirstApproach.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace DatabaseFirstApproach
{
    class Program
    {
        public static void Main(string[] args)
        {
            using (var dbcontext = new BikeStoresContext())
            {
                // -------- Linq to Entities
                var productList = from prod in dbcontext.Products
                                  select prod;

                Console.WriteLine("-------------------- List of All Products -----------------");
                foreach (Product p in productList){
                   
                    Console.WriteLine($"Product Id : {p.ProductId}\nProduct Name : {p.ProductName}\nProduct Price : {p.ListPrice}");
                    Console.WriteLine("-----------------------------------");
                }

                // --------- Raw Queries

                var ProductNameList = dbcontext.Products.FromSql($"SELECT * FROM Products").ToList();

                foreach(Product product in ProductNameList)
                {
                    Console.WriteLine(product.ProductName);
                }

                Staff stf = dbcontext.Staffs.Find(1);

                Console.WriteLine();
                Console.WriteLine($"{stf.FirstName + " " + stf.LastName}");



            }
            
        }
    }
}