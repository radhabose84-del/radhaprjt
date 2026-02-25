using AutoMapper;
using PurchaseManagement.Application.MiscMaster.Command.CreateMiscMaster;
using PurchaseManagement.Application.MiscMaster.Command.DeleteMiscMaster;
using PurchaseManagement.Application.MiscMaster.Command.UpdateMiscMaster;
using PurchaseManagement.Application.MiscMaster.Queries.GetMiscMaster;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Application.Common.Mappings
{
    public class MiscMasterProfile : Profile


    {
        public MiscMasterProfile()
        {
            CreateMap<PurchaseManagement.Domain.Entities.MiscMaster, GetMiscMasterDto>();

            CreateMap<PurchaseManagement.Domain.Entities.MiscMaster, GetMiscMasterAutoCompleteDto>();

            CreateMap<CreateMiscMasterCommand, PurchaseManagement.Domain.Entities.MiscMaster>()
           .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
           .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateMiscMasterCommand, PurchaseManagement.Domain.Entities.MiscMaster>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1 ? Status.Active : Status.Inactive));

            CreateMap<DeleteMiscMasterCommand, PurchaseManagement.Domain.Entities.MiscMaster>()
           .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));
        }
    }
}