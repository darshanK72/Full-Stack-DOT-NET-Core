using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitectureApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitectureApp.Infrastructure.Extentions
{
    public static class ServiceCollectionExtention
    {
        public static void AddInfrastructure(this IServiceCollection services,IConfiguration configuration)
        {
            string? connectionString = configuration.GetConnectionString("DefaultConnectionString");
            Console.WriteLine(connectionString);
            services.AddDbContext<CompanyDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });
        }
    }
}
