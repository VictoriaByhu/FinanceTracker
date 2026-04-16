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

public class CategoryService : ICategoryService
{
    private readonly IRepository<Category> _repository;
    private readonly IMapper _mapper;

    public CategoryService(IRepository<Category> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<CategoryDto>>(entities);
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity is null ? null : _mapper.Map<CategoryDto>(entity);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        var entity = _mapper.Map<Category>(dto);
        await _repository.AddAsync(entity);
        return _mapper.Map<CategoryDto>(entity);
    }

    public async Task UpdateAsync(int id, UpdateCategoryDto dto)
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
