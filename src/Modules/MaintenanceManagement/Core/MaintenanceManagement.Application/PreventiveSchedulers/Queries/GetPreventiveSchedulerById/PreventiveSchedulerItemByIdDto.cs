using AutoMapper;
using MaintenanceManagement.Application.Common.Mappings;
using MaintenanceManagement.Domain.Entities;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Queries.GetPreventiveSchedulerById
{
    public class PreventiveSchedulerItemByIdDto : IMapFrom<PreventiveSchedulerItems>
    {
        public int Id { get; set; }
        public int PreventiveSchedulerHdrId { get; set; }
        public string OldItemId { get; set; }
        public int RequiredQty { get; set; }
        public string OldCategoryDescription { get; set; }
        public string OldGroupName { get; set; }
        public string? OldItemName { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<PreventiveSchedulerItems, PreventiveSchedulerItemByIdDto>()
                .ForMember(dest => dest.PreventiveSchedulerHdrId, opt => opt.MapFrom(src => src.PreventiveSchedulerHeaderId));
        }
    }
}
