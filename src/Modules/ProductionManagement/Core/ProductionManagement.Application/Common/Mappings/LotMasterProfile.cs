using AutoMapper;
using ProductionManagement.Application.LotMaster.Commands.CreateLotMaster;
using ProductionManagement.Application.LotMaster.Commands.UpdateLotMaster;
using ProductionManagement.Application.LotMaster.Dto;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.Application.Common.Mappings
{
    public class LotMasterProfile : Profile
    {
        public LotMasterProfile()
        {
            CreateMap<CreateLotMasterCommand, Domain.Entities.LotMaster>()
                .ForMember(dest => dest.IsActive,  opt => opt.MapFrom(_ => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(_ => IsDelete.NotDeleted))
                .ForMember(dest => dest.TotalProducedQty, opt => opt.MapFrom(_ => 0m))
                .ForMember(dest => dest.AvailableQty,     opt => opt.MapFrom(_ => 0m))
                .ForMember(dest => dest.RunOutDate,        opt => opt.Ignore())
                .ForMember(dest => dest.LotTypeMisc,       opt => opt.Ignore())
                .ForMember(dest => dest.StatusMisc,        opt => opt.Ignore());

            CreateMap<UpdateLotMasterCommand, Domain.Entities.LotMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive))
                .ForMember(dest => dest.LotTypeMisc,  opt => opt.Ignore())
                .ForMember(dest => dest.StatusMisc,   opt => opt.Ignore());

            CreateMap<Domain.Entities.LotMaster, LotMasterDto>();
        }
    }
}
