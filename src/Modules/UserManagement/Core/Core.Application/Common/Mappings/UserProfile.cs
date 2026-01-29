using AutoMapper;
using Core.Application.Users.Queries.GetUsers;
using Core.Application.Users.Commands.CreateUser;
using Core.Application.Users.Commands.UpdateUser;
using Core.Application.Users.Commands.DeleteUser;

using Core.Domain.Entities;
using static Core.Domain.Enums.Common.Enums;
using Core.Application.Users.Queries.GetUserAutoComplete;
using Core.Application.Common.Interfaces;

public class UserProfile : Profile
{
    private readonly IIPAddressService _ipAddressService;
    public UserProfile()
    {
        CreateMap<CreateUserCommand, User>()
        .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
        .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => BCrypt.Net.BCrypt.HashPassword(src.Password)))
        .ForMember(dest => dest.UserCompanies, opt => opt.MapFrom(src => src.UserCompanies))
        .ForMember(dest => dest.UserRoleAllocations, opt => opt.MapFrom(src => src.userRoleAllocations))
        .ForMember(dest => dest.UserDivisions, opt => opt.MapFrom(src => src.userDivisions))
        .ForMember(dest => dest.UserDepartments, opt => opt.MapFrom(src => src.userDepartments))
        .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
        .ForMember(dest => dest.IsFirstTimeUser, opt => opt.MapFrom(src => FirstTimeUserStatus.Yes))
        .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));
        //.ForMember(dest => dest.UserType, opt => opt.MapFrom(src => 1));
        // .ForMember(dest => dest.EntityId, opt => opt.MapFrom(src => _ipAddressService.GetEntityId()));

        CreateMap<UserCompanyDTO, UserCompany>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.CompanyId))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

        CreateMap<UserRoleAllocationDTO, UserRoleAllocation>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.UserRoleId, opt => opt.MapFrom(src => src.UserRoleId))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

        CreateMap<UserUnitDTO, UserUnit>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.UnitId, opt => opt.MapFrom(src => src.UnitId))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

              CreateMap<UserDivisionDTO, UserDivision>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.DivisionId, opt => opt.MapFrom(src => src.DivisionId))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

             CreateMap<UserDepartmentDTO, UserDepartment>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.DepartmentId))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

        CreateMap<User, UserDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id));
        // .ForMember(dest => dest.UserCompanies, opt => opt.MapFrom(src => src.UserCompanies))
        // .ForMember(dest => dest.userRoleAllocations, opt => opt.MapFrom(src => src.UserRoleAllocations))
        // .ForMember(dest => dest.UserUnits, opt => opt.MapFrom(src => src.UserUnits));

        CreateMap<UpdateUserCommand, User>()
        .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
        .ForMember(dest => dest.UserCompanies, opt => opt.MapFrom(src => src.UserCompanies))
        .ForMember(dest => dest.UserRoleAllocations, opt => opt.MapFrom(src => src.userRoleAllocations))
        .ForMember(dest => dest.UserDivisions, opt => opt.MapFrom(src => src.userDivisions))
        .ForMember(dest => dest.UserUnits, opt => opt.MapFrom(src => src.userUnits))
        .ForMember(dest => dest.UserDepartments, opt => opt.MapFrom(src => src.userDepartments))
        .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
        .ForMember(dest => dest.IsFirstTimeUser, opt => opt.Ignore())
        .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1 ? Status.Active : Status.Inactive));
       // .ForMember(dest => dest.UserType, opt => opt.MapFrom(src => 1));            

        CreateMap<DeleteUserCommand, User>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId)) // Map UserRoleId to Id
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));

             CreateMap<UserCompany, UserCompanyDTO>()
            .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.CompanyId));

        CreateMap<UserRoleAllocation, UserRoleAllocationDTO>()
            .ForMember(dest => dest.UserRoleId, opt => opt.MapFrom(src => src.UserRoleId));

        CreateMap<UserUnit, UserUnitDTO>()
            .ForMember(dest => dest.UnitId, opt => opt.MapFrom(src => src.UnitId));

            CreateMap<UserDivision, UserDivisionDTO>()
            .ForMember(dest => dest.DivisionId, opt => opt.MapFrom(src => src.DivisionId));

            CreateMap<UserDepartment, UserDepartmentDTO>()
            .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.DepartmentId));

            CreateMap<User, UserByIdDTO>()
            .ForMember(dest => dest.UserId, opt => opt.Ignore()) 
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.UserCompanies, opt => opt.MapFrom(src => src.UserCompanies))
            .ForMember(dest => dest.userRoleAllocations, opt => opt.MapFrom(src => src.UserRoleAllocations))
            .ForMember(dest => dest.UserUnits, opt => opt.MapFrom(src => src.UserUnits))
            .ForMember(dest => dest.userDivisions, opt => opt.MapFrom(src => src.UserDivisions))
            .ForMember(dest => dest.userDepartments, opt => opt.MapFrom(src => src.UserDepartments));
            CreateMap<User, UserAutoCompleteDto>();

            CreateMap<PasswordLogDTO, PasswordLog>();
            
    }
}
