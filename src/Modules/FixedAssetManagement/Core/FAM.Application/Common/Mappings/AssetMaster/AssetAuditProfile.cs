using AutoMapper;
using FAM.Application.ExcelImport.PhysicalStockVerification;
using FAM.Domain.Entities.AssetMaster;

namespace FAM.Application.Common.Mappings.AssetMaster
{
    public class AssetAuditProfile : Profile
    {
        public AssetAuditProfile()
        {
            CreateMap<AssetAuditDto, AssetAudit>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
  
        }
    }
}