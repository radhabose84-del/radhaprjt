using AutoMapper;
using UserManagement.Application.DepartmentGroup.Command.CreateDepartmentGroup;
using UserManagement.Application.DepartmentGroup.Command.DeleteDepartmentGroup;
using UserManagement.Application.DepartmentGroup.Command.UpdateDepartmentGroup;
using UserManagement.Application.DepartmentGroup.Queries.GetAllDepartmentGroup;
using UserManagement.Application.DepartmentGroup.Queries.GetDepartmentGroupAutoSearch;
using UserManagement.Application.DepartmentGroup.Queries.GetDepartmentGroupById;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Application.Common.Mappings
{
    public class DepartmentGroupProfile : Profile
    {

        public DepartmentGroupProfile()
        {
            CreateMap<CreateDepartmentGroupCommand, UserManagement.Domain.Entities.DepartmentGroup>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UserManagement.Domain.Entities.DepartmentGroup, DepartmentGroupByIdDto>();

            CreateMap<UpdateDepartmentGroupCommand, UserManagement.Domain.Entities.DepartmentGroup>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

            CreateMap<UserManagement.Domain.Entities.DepartmentGroup, GetAllDepartmentGroupDto>();

            CreateMap<DeleteDepartmentGroupCommand, UserManagement.Domain.Entities.DepartmentGroup>()
             .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));
              CreateMap<UserManagement.Domain.Entities.DepartmentGroup, DepartmentGroupAutoCompleteDto>(); 

        }

    }
}