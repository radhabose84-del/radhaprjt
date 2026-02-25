using AutoMapper;
using MaintenanceManagement.Application.ShiftMasterDetails.Commands.CreateShiftMasterDetail;
using MaintenanceManagement.Application.ShiftMasterDetails.Commands.DeleteShiftMasterDetail;
using MaintenanceManagement.Application.ShiftMasterDetails.Commands.UpdateShiftMasterDetail;
using MaintenanceManagement.Application.ShiftMasterDetails.Queries.GetShiftMasterDetailById;
using MaintenanceManagement.Domain.Entities;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.Application.Common.Mappings
{
    public class ShiftMasterDetailProfile : Profile
    {
        public ShiftMasterDetailProfile()
        {
            CreateMap<CreateShiftMasterDetailCommand, ShiftMasterDetail>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted))
            .ForMember(dest => dest.DurationInHours, opt => opt.MapFrom(src => 
            (src.EndTime - src.StartTime).TotalHours));
            
            CreateMap<UpdateShiftMasterDetailCommand, ShiftMasterDetail>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive))
            .ForMember(dest => dest.DurationInHours, opt => opt.MapFrom(src => 
            (src.EndTime - src.StartTime).TotalHours));

            CreateMap<DeleteShiftMasterDetailCommand, ShiftMasterDetail>()
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));
            
            
            CreateMap<ShiftMasterDetail, ShiftMasterDetailByIdDto>();
        }
    }
}