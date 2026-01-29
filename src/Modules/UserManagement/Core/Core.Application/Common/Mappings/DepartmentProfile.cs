using AutoMapper;
using Core.Application.Departments.Queries.GetDepartments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Core.Application.Departments.Commands.CreateDepartment;
using Core.Application.Departments.Commands.DeleteDepartment;
using Core.Application.Departments.Commands.UpdateDepartment;
using static Core.Domain.Enums.Common.Enums;
using Core.Application.Departments.Queries.GetDepartmentAutoCompleteSearch;

namespace Core.Application.Common.Mappings
{
    public class DepartmentProfile : Profile
    {
        public DepartmentProfile()
        {           

            CreateMap<CreateDepartmentCommand, Department>()
            .ForMember(dest => dest.ShortName, opt => opt.MapFrom(src => src.ShortName))
            .ForMember(dest => dest.DeptName, opt => opt.MapFrom(src => src.DeptName))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted))
            .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.CompanyId));

            CreateMap<Department, DepartmentDto>()
           .ForMember(dest => dest.ShortName, opt => opt.MapFrom(src => src.ShortName))
           .ForMember(dest => dest.DeptName, opt => opt.MapFrom(src => src.DeptName))
           .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.CompanyId));

            CreateMap<DeleteDepartmentCommand, Department>()
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));

          
            CreateMap<Department, DepartmentAutocompleteDto>();

            CreateMap<DepartmentDto, GetDepartmentDto>();

            CreateMap<DepartmentStatusDto, Department>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

           CreateMap<UpdateDepartmentCommand, Department>();                  

           CreateMap<Department, GetDepartmentDto>()            
            .ForMember(dest => dest.IsActive,opt => opt.MapFrom(src => src.IsActive)) 
            .ForMember(dest => dest.IsDeleted,opt => opt.MapFrom(src => src.IsDeleted));

        }
    }
}