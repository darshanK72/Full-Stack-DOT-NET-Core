using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitectureApp.Domain.Contracts.Repositories;
using CleanArchitectureApp.Domain.Entities;
using CleanArchitectureApp.Infrastructure.Data;

namespace CleanArchitectureApp.Infrastructure.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        public readonly CompanyDbContext _context;
        public EmployeeRepository(CompanyDbContext dbContext) 
        { 
            _context = dbContext;
        }

        public Task<Employee> Create(Employee employee)
        {
            throw new NotImplementedException();
        }

        public Task<Employee> Delete(string employeeId)
        {
            throw new NotImplementedException();
        }

        public Task<Employee> Get(string employeeId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Employee>> GetAll()
        {
            throw new NotImplementedException();
        }

        public Task<Employee> Update(Employee employee)
        {
            throw new NotImplementedException();
        }
    }
}
