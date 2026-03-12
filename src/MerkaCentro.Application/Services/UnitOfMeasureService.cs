using MerkaCentro.Application.Common;
using MerkaCentro.Domain.Ports.Output;

namespace MerkaCentro.Application.Services;

public class UnitOfMeasureService : IUnitOfMeasureService
{
    private readonly IUnitOfMeasureRepository _repository;

    public UnitOfMeasureService(IUnitOfMeasureRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IEnumerable<UnitOfMeasureDto>>> GetActiveAsync()
    {
        var units = await _repository.GetActiveAsync();
        var dtos = units.Select(u => new UnitOfMeasureDto(
            u.Id, u.Code, u.Name, u.Group, u.AllowsFractions, u.DisplayOrder));
        return Result<IEnumerable<UnitOfMeasureDto>>.Success(dtos);
    }

    public async Task<Result<IEnumerable<UnitOfMeasureGroupDto>>> GetActiveGroupedAsync()
    {
        var units = await _repository.GetActiveAsync();
        var groups = units
            .GroupBy(u => u.Group)
            .Select(g => new UnitOfMeasureGroupDto(
                g.Key,
                g.Select(u => new UnitOfMeasureDto(
                    u.Id, u.Code, u.Name, u.Group, u.AllowsFractions, u.DisplayOrder))
                .ToList()))
            .ToList();
        return Result<IEnumerable<UnitOfMeasureGroupDto>>.Success(groups);
    }
}
