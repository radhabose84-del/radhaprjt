using AutoMapper;
using InventoryManagement.Application.Item.Templates.DTOs;
using InventoryManagement.Domain.Common; // for BaseEntity.Status
using InventoryManagement.Domain.Entities.item.ItemDetail.Templates;
using InventoryManagement.Domain.Entities.Item.ItemDetail.Templates;

namespace InventoryManagement.Application.Item.Templates.Mapping
{
    public sealed class TemplateMappingProfile : Profile
    {
        public TemplateMappingProfile()
        {
            CreateMap<InspectionParameter, TemplateParameterDto>().ReverseMap();

            CreateMap<InspectionTemplate, InspectionTemplateDto>()
                .ForMember(d => d.Parameters,    o => o.MapFrom(s => s.Parameters))
                .ForMember(d => d.IsActive,      o => o.MapFrom(s => s.IsActive == BaseEntity.Status.Active));

            CreateMap<InspectionTemplate, TemplateListItemDto>()
                .ForMember(d => d.ParameterCount, o => o.MapFrom(s => s.Parameters != null ? s.Parameters.Count : 0))
                .ForMember(d => d.IsActive,       o => o.MapFrom(s => s.IsActive == BaseEntity.Status.Active));
        }
    }
}
