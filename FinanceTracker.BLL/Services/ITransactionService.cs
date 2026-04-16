using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FinanceTracker.BLL.DTOs;

namespace FinanceTracker.BLL.Services;

public interface ITransactionService
{
    Task<IEnumerable<TransactionDto>> GetAllAsync();
    Task<TransactionDto?> GetByIdAsync(int id);
    Task<TransactionDto> CreateAsync(CreateTransactionDto dto);
    Task UpdateAsync(int id, UpdateTransactionDto dto);
    Task DeleteAsync(int id);
}
