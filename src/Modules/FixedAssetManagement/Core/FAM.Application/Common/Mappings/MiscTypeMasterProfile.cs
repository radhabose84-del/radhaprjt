using AutoMapper;
using FAM.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using FAM.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using FAM.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using FAM.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using static FAM.Domain.Common.BaseEntity;

namespace FAM.Application.Common.Mappings
{
    public class MiscTypeMasterProfile : Profile
    {
        public MiscTypeMasterProfile()
        {
                CreateMap<FAM.Domain.Entities.MiscTypeMaster,GetMiscTypeMasterDto>();

                CreateMap<FAM.Domain.Entities.MiscTypeMaster,GetMiscTypeMasterAutocompleteDto>();

                CreateMap<CreateMiscTypeMasterCommand, FAM.Domain.Entities.MiscTypeMaster>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

                CreateMap<UpdateMiscTypeMasterCommand, FAM.Domain.Entities.MiscTypeMaster>()
                 .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));

                CreateMap<DeleteMiscTypeMasterCommand,  FAM.Domain.Entities.MiscTypeMaster>()
                 .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted)); 
        }

    }
}