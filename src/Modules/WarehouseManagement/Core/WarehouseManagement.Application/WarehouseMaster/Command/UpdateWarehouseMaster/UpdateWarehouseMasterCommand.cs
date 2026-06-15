using Contracts.Common;
using MediatR;

namespace WarehouseManagement.Application.WarehouseMaster.Command.UpdateWarehouseMaster
{
    public class UpdateWarehouseMasterCommand : IRequest<ApiResponseDTO<bool>>, IRequirePermission
    {
    public int Id { get; set; }
    public string? WarehouseName { get; set; }
    public int UnitId { get; set; }
    public int? ParentWarehouseId { get; set; }
    public bool IsGroup { get; set; }
    public bool IsVirtualWarehouse { get; set; }
    public int WarehouseTypeId { get; set; }
    public int DepartmentId { get; set; }
    public int StorageTypeId { get; set; }
    public int AreaTypeId { get; set; }
    public int  OperationTypeId { get; set; }
    public int CapacityUOMId { get; set; }
    public int? AccountId { get; set; }
    public string? ContactPersonName { get; set; }
    public string? MobileNumber { get; set; }
    public string? Email { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }

    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public int CityId { get; set; }
    public int StateId { get; set; }
    public int CountryId { get; set; }
    public string? Pincode { get; set; }
    public bool IsScrapWarehouse { get; set; }
    public bool IsTransitWarehouse { get; set; }
    public double MaxCapacity { get; set; }
    public bool IsDefaultStockEntry { get; set; }
    public byte IsActive { get; set; }
    public List<int>? AllowedItemGroupIds { get; set; }

        
    public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
