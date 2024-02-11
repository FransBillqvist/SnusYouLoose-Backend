using DAL;
using DAL.Dto;

namespace Services.Interfaces;

public interface IProgressionService
{
    Task<Progression> AddNewProgression(string uid);
    Task<ProgressionDto> AddNewProgressionV2(string uid);
    Task<int> CalculateRemainingSnuff(string uid);
    Task<Progression> FindUserActiveProgression(string uid);
    Task<ProgressionDto> FindUserActiveProgressionDto(string uid);
    Task RemoveProgressionAsync(string pid);
    Task<Progression> UpdateProgressionStateAsync(Progression updatedProgression);
    Task<Progression> ProgressionHandler(string uid);
    Task<string> WhenIsTheNextDoseAvailable(string uid);
    Task<TimeSpan> WhenIsTheNextDoseAvailableV2(string uid);
    Task<string> LastConsumedSnuffAtUtc(string uid);
    ProgressionDto MapProgressionToDto(Progression progression);
    Task<Tuple<int, int>> GetUsedAndAvailableSnuff(string uid);
}