namespace Services.Interfaces;

public interface ISnuffService
{
    Task<int> GetSnuffAmountAsync(string snuffId);
}
