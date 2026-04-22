using FinanceTracker.DAL;
using FinanceTracker.DAL.Entities;
using FinanceTracker.DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.MAUI.Services;

public class LocalDataService : IDataService
{
    private readonly IRepository<Transaction> _transactionRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly AppDbContext _context;

    public LocalDataService(
        IRepository<Transaction> transactionRepository,
        IRepository<Category> categoryRepository,
        AppDbContext context)
    {
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
        _context = context;
    }

    // Transactions
    public async Task<IEnumerable<Transaction>> GetAllTransactionsAsync()
    {
        try
        {
            return await _context.Transactions
                .Include(t => t.Category)
                .ToListAsync();
        }
        catch (Exception)
        {
            return Enumerable.Empty<Transaction>();
        }
    }

    public async Task<Transaction?> GetTransactionByIdAsync(int id)
    {
        try
        {
            return await _context.Transactions
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.Id == id);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<Transaction> CreateTransactionAsync(Transaction transaction)
    {
        try
        {
            await _transactionRepository.AddAsync(transaction);
            return transaction;
        }
        catch (Exception)
        {
            return transaction;
        }
    }

    public async Task UpdateTransactionAsync(Transaction transaction)
    {
        try
        {
            await _transactionRepository.UpdateAsync(transaction);
        }
        catch (Exception) { }
    }

    public async Task DeleteTransactionAsync(int id)
    {
        try
        {
            await _transactionRepository.DeleteAsync(id);
        }
        catch (Exception) { }
    }

    // Categories
    public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
    {
        try
        {
            return await _categoryRepository.GetAllAsync();
        }
        catch (Exception)
        {
            return Enumerable.Empty<Category>();
        }
    }

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        try
        {
            return await _categoryRepository.GetByIdAsync(id);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<Category> CreateCategoryAsync(Category category)
    {
        try
        {
            await _categoryRepository.AddAsync(category);
            return category;
        }
        catch (Exception)
        {
            return category;
        }
    }

    public async Task UpdateCategoryAsync(Category category)
    {
        try
        {
            await _categoryRepository.UpdateAsync(category);
        }
        catch (Exception) { }
    }

    public async Task DeleteCategoryAsync(int id)
    {
        try
        {
            await _categoryRepository.DeleteAsync(id);
        }
        catch (Exception) { }
    }
}