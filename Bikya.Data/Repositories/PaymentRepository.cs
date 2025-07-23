using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bikya.Data.Models;
using Bikya.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bikya.Data.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly BikyaContext _context;
        public PaymentRepository(BikyaContext context)
        {
            _context = context;
        }

        public async Task<Payment> AddAsync(Payment payment)
        {
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task<Payment?> GetByIdAsync(int id)
        {
            return await _context.Payments.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Payment>> GetByUserIdAsync(int userId)
        {
            return await _context.Payments.Where(p => p.UserId == userId).ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
} 