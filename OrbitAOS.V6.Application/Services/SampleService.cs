using OrbitAOS.V6.Application.DTOs;
using OrbitAOS.V6.Application.Interfaces;
using OrbitAOS.V6.Domain.Entities;

namespace OrbitAOS.V6.Application.Services;

/// <summary>
/// Sample service implementation for business logic
/// </summary>
public class SampleService : ISampleService
{
    private readonly IRepository<SampleEntity> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public SampleService(IRepository<SampleEntity> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<SampleDto>> GetAllSamplesAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToDto);
    }

    public async Task<SampleDto?> GetSampleByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<SampleDto> CreateSampleAsync(SampleDto dto)
    {
        var entity = new SampleEntity
        {
            Name = dto.Name,
            Description = dto.Description,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(created);
    }

    public async Task UpdateSampleAsync(int id, SampleDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
        {
            throw new KeyNotFoundException($"Sample with ID {id} not found");
        }

        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(entity);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteSampleAsync(int id)
    {
        await _repository.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    private static SampleDto MapToDto(SampleEntity entity)
    {
        return new SampleDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
