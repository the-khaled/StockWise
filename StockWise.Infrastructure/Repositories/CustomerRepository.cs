using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockWise.Domain.Interfaces;
using StockWise.Domain.Models;
using StockWise.Infrastructure.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockWise.Infrastructure.Repositories
{
    public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
    {
        public CustomerRepository(StockWiseDbContext context):base(context) { }
        public override async Task<Customer> GetByIdAsync(int id) 
        {
            return await _context.customers
            .Include(c => c.Invoices)
            .Include(c => c.Payments)
            .Include(c => c.Returns)
            .FirstOrDefaultAsync(c => c.Id == id);
        }

    }
}
