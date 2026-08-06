using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitectureApp.Application.Extentions
{
    public static class ServiceCollectionExtention
    {
        public static void AddApplication(this IServiceCollection services)
        {
            services.AddScoped<I>
        }
    }
}
