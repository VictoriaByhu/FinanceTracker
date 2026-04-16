using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using FinanceTracker.BLL.DTOs;
using FinanceTracker.DAL.Entities;
using FinanceTracker.DAL.Repositories;

namespace FinanceTracker.BLL.Services;

public class TransactionService : ITransactionService
{
    private readonly IRepository<Transaction> _repository;
    private readonly IMapper _mapper;

    public TransactionService(IRepository<Transaction> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TransactionDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<TransactionDto>>(entities);
    }

    public async Task<TransactionDto?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity is null ? null : _mapper.Map<TransactionDto>(entity);
    }

    public async Task<TransactionDto> CreateAsync(CreateTransactionDto dto)
    {
        var entity = _mapper.Map<Transaction>(dto);
        await _repository.AddAsync(entity);
        return _mapper.Map<TransactionDto>(entity);
    }

    public async Task UpdateAsync(int id, UpdateTransactionDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return;
        _mapper.Map(dto, entity);
        await _repository.UpdateAsync(entity);
    }

    public async Task DeleteAsync(int id)
    {
        await _repository.DeleteAsync(id);
    }
}
