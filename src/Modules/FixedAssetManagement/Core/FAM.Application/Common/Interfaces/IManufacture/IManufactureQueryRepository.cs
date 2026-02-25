using FAM.Application.Manufacture.Queries.GetManufacture;

namespace FAM.Application.Common.Interfaces.IManufacture
{
    public interface IManufactureQueryRepository
    {
        Task<ManufactureDTO> GetByIdAsync(int Id);
        Task<(List<ManufactureDTO>, int)> GetAllManufactureAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<List<ManufactureDTO>> GetByManufactureNameAsync(string manufactureName);
        Task<List<FAM.Domain.Entities.MiscMaster>> GetManufactureTypeAsync();
        Task<bool> CountrySoftDeleteValidation(int countryId);
        Task<bool> CitySoftDeleteValidation(int cityId);
        Task<bool> StateSoftDeleteValidation(int stateId);
        Task<bool> IsManufactureLinkedAsync(int manufacturerId); //IsActive And Delete Validation 
    }
}