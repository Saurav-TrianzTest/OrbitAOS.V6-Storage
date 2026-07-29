using OrbitAOS.V6.Application.DTOs;

namespace OrbitAOS.V6.Application.Interfaces;

/// <summary>
/// Sample service interface for business logic
/// Replace with actual business service interfaces
/// </summary>
public interface ISampleService
{
    Task<IEnumerable<SampleDto>> GetAllSamplesAsync();
    Task<SampleDto?> GetSampleByIdAsync(int id);
    Task<SampleDto> CreateSampleAsync(SampleDto dto);
    Task UpdateSampleAsync(int id, SampleDto dto);
    Task DeleteSampleAsync(int id);
}
