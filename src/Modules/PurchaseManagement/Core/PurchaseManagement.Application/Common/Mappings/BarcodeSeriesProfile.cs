using AutoMapper;
using PurchaseManagement.Application.BarcodeSeries.Command.CreateBarcodeSeries;
using PurchaseManagement.Application.BarcodeSeries.Command.UpdateBarcodeSeries;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Application.Common.Mappings
{
    public class BarcodeSeriesProfile : Profile
    {
        public BarcodeSeriesProfile()
        {
            // IsActive/IsDeleted set here so handlers never assign them manually.
            // StatusId (default "Open") and BarcodeSeriesNumber are resolved in the command repository.
            CreateMap<CreateBarcodeSeriesCommand, PurchaseManagement.Domain.Entities.BarcodeSeries>()
                .ForMember(d => d.IsActive, opt => opt.MapFrom(_ => Status.Active))
                .ForMember(d => d.IsDeleted, opt => opt.MapFrom(_ => IsDelete.NotDeleted));

            CreateMap<UpdateBarcodeSeriesCommand, PurchaseManagement.Domain.Entities.BarcodeSeries>()
                .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
