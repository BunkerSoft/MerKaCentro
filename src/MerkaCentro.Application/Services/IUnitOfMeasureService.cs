using MerkaCentro.Application.Common;

namespace MerkaCentro.Application.Services;

public interface IUnitOfMeasureService
{
    Task<Result<IEnumerable<UnitOfMeasureDto>>> GetActiveAsync();
    Task<Result<IEnumerable<UnitOfMeasureGroupDto>>> GetActiveGroupedAsync();
}

public record UnitOfMeasureDto(
    Guid Id,
    string Code,
    string Name,
    string Group,
    bool AllowsFractions,
    int DisplayOrder);

public record UnitOfMeasureGroupDto(
    string Group,
    IReadOnlyList<UnitOfMeasureDto> Units);
