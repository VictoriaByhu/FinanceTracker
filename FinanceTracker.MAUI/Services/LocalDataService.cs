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
            var transactions = await _context.Transactions
                .AsNoTracking()
                .Include(t => t.Category)
                .ToListAsync();

            foreach (var transaction in transactions.Where(t => t.Category is not null))
                transaction.IsIncome = transaction.Category.IsIncome;

            return transactions;
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
            var transaction = await _context.Transactions
                .AsNoTracking()
                .Include(t => t.Category)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transaction?.Category is not null)
                transaction.IsIncome = transaction.Category.IsIncome;

            return transaction;
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
            var category = await _categoryRepository.GetByIdAsync(transaction.CategoryId);
            if (category is not null)
                transaction.IsIncome = category.IsIncome;

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
            var category = await _categoryRepository.GetByIdAsync(transaction.CategoryId);
            if (category is not null)
                transaction.IsIncome = category.IsIncome;

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
            return await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();
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
            return await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
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
            var existing = await _context.Categories.FindAsync(category.Id);
            if (existing is null)
                return;

            existing.Name = category.Name;
            existing.Color = category.Color;
            existing.IsIncome = category.IsIncome;

            var transactions = await _context.Transactions
                .Where(t => t.CategoryId == category.Id)
                .ToListAsync();

            foreach (var transaction in transactions)
                transaction.IsIncome = category.IsIncome;

            await _context.SaveChangesAsync();
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
