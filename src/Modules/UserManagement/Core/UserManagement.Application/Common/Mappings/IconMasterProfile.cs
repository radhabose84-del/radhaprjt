using System.Text.Json;
using AutoMapper;
using UserManagement.Application.IconMaster.Commands.CreateIconMaster;
using UserManagement.Application.IconMaster.Commands.DeleteIconMaster;
using UserManagement.Application.IconMaster.Commands.UpdateIconMaster;
using UserManagement.Application.IconMaster.Queries.GetIconMaster;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Application.Common.Mappings
{
    public class IconMasterProfile : Profile
    {
        public IconMasterProfile()
        {
            // Reads: stored JSON string -> nested JSON object
            CreateMap<UserManagement.Domain.Entities.IconMaster, IconMasterDto>()
                .ForMember(dest => dest.Style, opt => opt.MapFrom(src => IconStyleJson.ToElement(src.Style)));

            CreateMap<UserManagement.Domain.Entities.IconMaster, IconMasterAutoCompleteDto>()
                .ForMember(dest => dest.Style, opt => opt.MapFrom(src => IconStyleJson.ToElement(src.Style)));

            // Writes: nested JSON object -> stored JSON string
            CreateMap<CreateIconMasterCommand, UserManagement.Domain.Entities.IconMaster>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Style, opt => opt.MapFrom(src => IconStyleJson.ToJsonString(src.Style)))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            // IsActive deliberately not mapped — update must not change active state
            CreateMap<UpdateIconMasterCommand, UserManagement.Domain.Entities.IconMaster>()
                .ForMember(dest => dest.Style, opt => opt.MapFrom(src => IconStyleJson.ToJsonString(src.Style)));

            CreateMap<DeleteIconMasterCommand, UserManagement.Domain.Entities.IconMaster>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));
        }
    }

    // JSON <-> string helpers for the IconMaster.Style column
    public static class IconStyleJson
    {
        public static string? ToJsonString(JsonElement? value)
            => value.HasValue ? value.Value.GetRawText() : null;

        public static JsonElement? ToElement(string? json)
            => string.IsNullOrWhiteSpace(json) ? null : JsonDocument.Parse(json).RootElement.Clone();
    }
}
