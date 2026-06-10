using AutoMapper;
using PurchaseManagement.Application.Arrival.Commands.CreateArrival;
using PurchaseManagement.Application.Arrival.Commands.UpdateArrival;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Application.Common.Mappings
{
    public class ArrivalProfile : Profile
    {
        public ArrivalProfile()
        {
            // Detail lines + ArrivalNumber + UnitId + computed weights are set in the handler,
            // not mapped here.
            CreateMap<CreateArrivalCommand, Domain.Entities.Arrival.ArrivalHeader>()
                .ForMember(d => d.IsActive, o => o.MapFrom(_ => Status.Active))
                .ForMember(d => d.IsDeleted, o => o.MapFrom(_ => IsDelete.NotDeleted))
                // QcStatusId is not supplied on create — left null, set later by QC sign-off.
                .ForMember(d => d.QcStatusId, o => o.Ignore())
                .ForMember(d => d.ArrivalDetails, o => o.Ignore());

            CreateMap<UpdateArrivalCommand, Domain.Entities.Arrival.ArrivalHeader>()
                .ForMember(d => d.IsActive, o => o.MapFrom(s => s.IsActive == 1 ? Status.Active : Status.Inactive))
                .ForMember(d => d.ArrivalDetails, o => o.Ignore());
        }
    }
}
