using AutoMapper;
using PurchaseManagement.Application.BarcodeAllocation.Command.CreateBarcodeAllocation;
using PurchaseManagement.Application.BarcodeAllocation.Command.UpdateBarcodeAllocation;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Application.Common.Mappings
{
    public class BarcodeAllocationProfile : Profile
    {
        public BarcodeAllocationProfile()
        {
            // AllocationNumber + StatusId (default "Open") are resolved in the command repository.
            CreateMap<CreateBarcodeAllocationCommand, PurchaseManagement.Domain.Entities.BarcodeAllocation>()
                .ForMember(d => d.IsActive, opt => opt.MapFrom(_ => Status.Active))
                .ForMember(d => d.IsDeleted, opt => opt.MapFrom(_ => IsDelete.NotDeleted));

            CreateMap<UpdateBarcodeAllocationCommand, PurchaseManagement.Domain.Entities.BarcodeAllocation>()
                .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
