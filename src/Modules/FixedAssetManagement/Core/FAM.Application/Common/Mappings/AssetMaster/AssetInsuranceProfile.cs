using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FAM.Application.AssetMaster.AssetInsurance.Commands.CreateAssetInsurance;
using FAM.Application.AssetMaster.AssetInsurance.Commands.DeleteAssetInsurance;
using FAM.Application.AssetMaster.AssetInsurance.Commands.UpdateAssetInsurance;
using FAM.Application.AssetMaster.AssetInsurance.Queries.GetAssetInsurance;
using static FAM.Domain.Common.BaseEntity;

namespace FAM.Application.Common.Mappings.AssetMaster
{
    public class AssetInsuranceProfile : Profile
    {

        public AssetInsuranceProfile()
        {
            CreateMap<FAM.Domain.Entities.AssetMaster.AssetInsurance, GetAssetInsuranceDto>(); 

            CreateMap<CreateAssetInsuranceCommand, FAM.Domain.Entities.AssetMaster.AssetInsurance>()
             .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive))
             .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateAssetInsuranceCommand , FAM.Domain.Entities.AssetMaster.AssetInsurance>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())                
                .ForMember(dest => dest.PolicyNo, opt => opt.MapFrom(src => src.PolicyNo))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.Insuranceperiod, opt => opt.MapFrom(src => src.Insuranceperiod))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.PolicyAmount, opt => opt.MapFrom(src => src.PolicyAmount))
                .ForMember(dest => dest.VendorCode, opt => opt.MapFrom(src => src.VendorCode))
                .ForMember(dest => dest.RenewalStatus, opt => opt.MapFrom(src => src.RenewalStatus))
                .ForMember(dest => dest.RenewedDate, opt => opt.MapFrom(src => src.RenewedDate))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));

            CreateMap<DeleteAssetInsuranceCommand, FAM.Domain.Entities.AssetMaster.AssetInsurance>()                
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));    



        }
        
    }
}