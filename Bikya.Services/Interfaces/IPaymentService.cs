using System.Collections.Generic;
using System.Threading.Tasks;
using Bikya.DTOs.PaymentDTOs;
using Bikya.Data.Response;

namespace Bikya.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<ApiResponse<PaymentDto>> CreatePaymentAsync(CreatePaymentDto dto);
        Task<PaymentDto?> GetPaymentByIdAsync(int id);
        Task<IEnumerable<PaymentDto>> GetPaymentsByUserIdAsync(int userId);
    }
} 