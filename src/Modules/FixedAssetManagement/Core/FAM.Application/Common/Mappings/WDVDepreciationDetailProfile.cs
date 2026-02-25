using AutoMapper;
using FAM.Application.WDVDepreciation.Commands.CreateDepreciation;
using FAM.Application.WDVDepreciation.Commands.DeleteDepreciation;
using FAM.Application.WDVDepreciation.Commands.LockDepreciation;
using FAM.Application.WDVDepreciation.Queries.GetDepreciation;
using FAM.Domain.Entities;

namespace FAM.Application.Common.Mappings
{
    public class WDVDepreciationDetailProfile : Profile    
    {
        public WDVDepreciationDetailProfile()
        {             
            CreateMap<DeleteDepreciationCommand, CalculationDepreciationDto>()
                .ForMember(dest => dest.FinYear, opt => opt.MapFrom(src => src.FinYearId));
            CreateMap<LockDepreciationCommand, CalculationDepreciationDto>()
                .ForMember(dest => dest.FinYear, opt => opt.MapFrom(src => src.FinYearId));
            CreateMap<CreateDepreciationCommand, WDVDepreciationDetail>();
            CreateMap<WDVDepreciationDetail, CalculationDepreciationDto>();                         
            CreateMap<WDVDepreciationDetail, CalculationDepreciationDto>();                            
        }             
    }
}