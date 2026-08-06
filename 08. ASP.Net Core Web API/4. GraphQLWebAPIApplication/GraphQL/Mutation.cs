using GraphQLWebAPIApplication.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace GraphQLWebAPIApplication.GraphQL
{
    public class Mutation
    {
        // Product Mutations
        public async Task<Product> AddProduct(Product input, [Service] AppDbContext context)
        {
            context.Products.Add(input);
            await context.SaveChangesAsync();
            return input;
        }

        public async Task<Product> UpdateProduct(Product input, [Service] AppDbContext context)
        {
            context.Products.Update(input);
            await context.SaveChangesAsync();
            return input;
        }

        public async Task<bool> DeleteProduct(int id, [Service] AppDbContext context)
        {
            var product = await context.Products.FindAsync(id);
            if (product == null) return false;
            context.Products.Remove(product);
            await context.SaveChangesAsync();
            return true;
        }

        // Category Mutations
        public async Task<Category> AddCategory(Category input, [Service] AppDbContext context)
        {
            context.Categories.Add(input);
            await context.SaveChangesAsync();
            return input;
        }

        public async Task<Category> UpdateCategory(Category input, [Service] AppDbContext context)
        {
            context.Categories.Update(input);
            await context.SaveChangesAsync();
            return input;
        }

        public async Task<bool> DeleteCategory(int id, [Service] AppDbContext context)
        {
            var category = await context.Categories.FindAsync(id);
            if (category == null) return false;
            context.Categories.Remove(category);
            await context.SaveChangesAsync();
            return true;
        }

        // Customer Mutations
        public async Task<Customer> AddCustomer(Customer input, [Service] AppDbContext context)
        {
            context.Customers.Add(input);
            await context.SaveChangesAsync();
            return input;
        }

        public async Task<Customer> UpdateCustomer(Customer input, [Service] AppDbContext context)
        {
            context.Customers.Update(input);
            await context.SaveChangesAsync();
            return input;
        }

        public async Task<bool> DeleteCustomer(int id, [Service] AppDbContext context)
        {
            var customer = await context.Customers.FindAsync(id);
            if (customer == null) return false;
            context.Customers.Remove(customer);
            await context.SaveChangesAsync();
            return true;
        }

        // Order Mutations
        public async Task<Order> AddOrder(Order input, [Service] AppDbContext context)
        {
            context.Orders.Add(input);
            await context.SaveChangesAsync();
            return input;
        }

        public async Task<Order> UpdateOrder(Order input, [Service] AppDbContext context)
        {
            context.Orders.Update(input);
            await context.SaveChangesAsync();
            return input;
        }

        public async Task<bool> DeleteOrder(int id, [Service] AppDbContext context)
        {
            var order = await context.Orders.FindAsync(id);
            if (order == null) return false;
            context.Orders.Remove(order);
            await context.SaveChangesAsync();
            return true;
        }

        // OrderItem Mutations
        public async Task<OrderItem> AddOrderItem(OrderItem input, [Service] AppDbContext context)
        {
            context.OrderItems.Add(input);
            await context.SaveChangesAsync();
            return input;
        }

        public async Task<OrderItem> UpdateOrderItem(OrderItem input, [Service] AppDbContext context)
        {
            context.OrderItems.Update(input);
            await context.SaveChangesAsync();
            return input;
        }

        public async Task<bool> DeleteOrderItem(int id, [Service] AppDbContext context)
        {
            var orderItem = await context.OrderItems.FindAsync(id);
            if (orderItem == null) return false;
            context.OrderItems.Remove(orderItem);
            await context.SaveChangesAsync();
            return true;
        }
    }
} 