using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Application.CustomFields.Commands.CreateCustomField;
using Core.Application.CustomFields.Commands.DeleteCustomField;
using Core.Application.CustomFields.Commands.UpdateCustomField;
using Core.Application.CustomFields.Queries.GetCustomField;
using Core.Application.CustomFields.Queries.GetCustomFieldById;
using Core.Domain.Entities;
using static Core.Domain.Enums.Common.Enums;

namespace Core.Application.Common.Mappings
{
    public class CustomFieldProfile : Profile
    {
        public CustomFieldProfile()
        {
             CreateMap<CreateCustomFieldCommand, CustomField>()
             .ForMember(dest => dest.CustomFieldMenu, opt => opt.MapFrom(src => src.Menu))
             .ForMember(dest => dest.CustomFieldUnits, opt => opt.MapFrom(src => src.Unit))
             .ForMember(dest => dest.CustomFieldOptionalValues, opt => opt.MapFrom(src => src.OptionalValues))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<CustomFieldMenuDto, CustomFieldMenu>();
            CreateMap<CustomFieldUnitDto, CustomFieldUnit>();
            CreateMap<CustomFieldOptionalValueDto, CustomFieldOptionalValue>();

            CreateMap<UpdateCustomFieldCommand, CustomField>()
            .ForMember(dest => dest.CustomFieldMenu, opt => opt.MapFrom(src => src.Menu))
             .ForMember(dest => dest.CustomFieldUnits, opt => opt.MapFrom(src => src.Unit))
             .ForMember(dest => dest.CustomFieldOptionalValues, opt => opt.MapFrom(src => src.OptionalValues))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));

            CreateMap<CustomFieldMenuUpdateDto, CustomFieldMenu>();
            CreateMap<CustomFieldUnitUpdateDto, CustomFieldUnit>();
            CreateMap<CustomFieldOptionalValueUpdateDto, CustomFieldOptionalValue>();

            CreateMap<DeleteCustomFieldCommand, CustomField>()
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));

            CreateMap<CustomField, CustomFieldDTO>()
            .ForMember(dest => dest.LabelType, opt => opt.MapFrom(src => src.LabelType.Code)) 
            .ForMember(dest => dest.DataType, opt => opt.MapFrom(src => src.DataType.Code));



        }
    }
}