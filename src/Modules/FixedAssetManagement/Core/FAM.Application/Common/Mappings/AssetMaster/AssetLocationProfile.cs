using AutoMapper;
using FAM.Application.AssetLocation.Commands.CreateAssetLocation;
using FAM.Application.AssetLocation.Queries.GetAssetLocation;
using FAM.Application.AssetMaster.AssetLocation.Commands.UpdateAssetLocation;
using FAM.Application.AssetMaster.AssetLocation.Queries.GetCustodian;
using FAM.Application.AssetMaster.AssetLocation.Queries.GetSubLocationById;

namespace FAM.Application.Common.Mappings
{
    public class AssetLocationProfile : Profile
    {

        public AssetLocationProfile()
        {
            CreateMap<FAM.Domain.Entities.AssetMaster.AssetLocation,AssetLocationDto>();

            CreateMap<CreateAssetLocationCommand, FAM.Domain.Entities.AssetMaster.AssetLocation>();
            CreateMap<UpdateAssetLocationCommand, FAM.Domain.Entities.AssetMaster.AssetLocation>() 
            .ForMember(dest => dest.AssetId, opt => opt.Ignore())  
            .ForMember(dest => dest.UnitId, opt => opt.MapFrom(src => src.UnitId))
            .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.DepartmentId))                
            .ForMember(dest => dest.LocationId, opt => opt.MapFrom(src => src.LocationId))
            .ForMember(dest => dest.SubLocationId, opt => opt.MapFrom(src => src.SubLocationId))
            .ForMember(dest => dest.CustodianId, opt => opt.MapFrom(src => src.CustodianId))
            .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.DepartmentId))
            .ForMember(dest => dest.UnitId, opt => opt.MapFrom(src => src.UnitId));
            
            CreateMap<FAM.Domain.Entities.AssetMaster.Employee,GetCustodianDto>()           
            .ForMember(dest => dest.CustodianId, opt => opt.MapFrom(src => src.Empcode))
            .ForMember(dest => dest.CustodianName, opt => opt.MapFrom(src => src.Empname));

            CreateMap<FAM.Domain.Entities.SubLocation, GetAssetSubLocationDto>();

           
                
        }
        
    }
}