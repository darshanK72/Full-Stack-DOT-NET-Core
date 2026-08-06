using GraphQLWebAPIApplication.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQLWebAPIApplication.GraphQL
{
    public class Query
    {
        public async Task<List<Product>> GetProducts([Service] AppDbContext context)
        {
            return await context.Products.Include(p => p.Category).ToListAsync();
        }

        public async Task<Product> GetProductById(int id, [Service] AppDbContext context)
        {
            return await context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Category>> GetCategories([Service] AppDbContext context)
        {
            return await context.Categories.Include(c => c.Products).ToListAsync();
        }

        public async Task<Category> GetCategoryById(int id, [Service] AppDbContext context)
        {
            return await context.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Customer>> GetCustomers([Service] AppDbContext context)
        {
            return await context.Customers.Include(c => c.Orders).ToListAsync();
        }

        public async Task<Customer> GetCustomerById(int id, [Service] AppDbContext context)
        {
            return await context.Customers.Include(c => c.Orders).FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Order>> GetOrders([Service] AppDbContext context)
        {
            return await context.Orders.Include(o => o.OrderItems).Include(o => o.Customer).ToListAsync();
        }

        public async Task<Order> GetOrderById(int id, [Service] AppDbContext context)
        {
            return await context.Orders.Include(o => o.OrderItems).Include(o => o.Customer).FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<List<OrderItem>> GetOrderItems([Service] AppDbContext context)
        {
            return await context.OrderItems.Include(oi => oi.Order).Include(oi => oi.Product).ToListAsync();
        }

        public async Task<OrderItem> GetOrderItemById(int id, [Service] AppDbContext context)
        {
            return await context.OrderItems.Include(oi => oi.Order).Include(oi => oi.Product).FirstOrDefaultAsync(oi => oi.Id == id);
        }
    }
} 