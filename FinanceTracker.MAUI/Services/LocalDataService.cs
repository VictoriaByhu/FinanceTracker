using FinanceTracker.DAL.Entities;
using FinanceTracker.DAL.Repositories;

namespace FinanceTracker.MAUI.Services;

public class LocalDataService : IDataService
{
    private readonly IRepository<Transaction> _transactionRepository;
    private readonly IRepository<Category> _categoryRepository;

    public LocalDataService(
        IRepository<Transaction> transactionRepository,
        IRepository<Category> categoryRepository)
    {
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
    }

    // Transactions
    public async Task<IEnumerable<Transaction>> GetAllTransactionsAsync()
    {
        try
        {
            return await _transactionRepository.GetAllAsync();
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
            return await _transactionRepository.GetByIdAsync(id);
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