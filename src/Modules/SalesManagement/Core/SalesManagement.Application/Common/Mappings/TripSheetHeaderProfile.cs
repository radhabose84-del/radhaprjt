using AutoMapper;
using SalesManagement.Application.TripSheet.Commands.CreateTripSheet;
using SalesManagement.Application.TripSheet.Commands.UpdateTripSheet;
using SalesManagement.Application.TripSheet.Dto;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class TripSheetHeaderProfile : Profile
    {
        public TripSheetHeaderProfile()
        {
            // Entity to DTO
            CreateMap<Domain.Entities.TripSheetHeader, TripSheetHeaderDto>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted == IsDelete.Deleted));

            CreateMap<Domain.Entities.TripSheetHeader, TripSheetLookupDto>();

            // Command to Entity
            CreateMap<CreateTripSheetCommand, Domain.Entities.TripSheetHeader>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted))
                .ForMember(dest => dest.TripSheetDetails, opt => opt.MapFrom(src =>
                    src.Details != null
                        ? src.Details.Select(d => new TripSheetDetail
                        {
                            DispatchAdviceHeaderId = d.DispatchAdviceHeaderId,
                            SequenceNo = d.SequenceNo
                        }).ToList()
                        : new List<TripSheetDetail>()));

            CreateMap<UpdateTripSheetCommand, Domain.Entities.TripSheetHeader>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
