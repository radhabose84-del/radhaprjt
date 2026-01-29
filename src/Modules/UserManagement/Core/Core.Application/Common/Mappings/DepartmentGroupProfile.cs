using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Application.DepartmentGroup.Command.CreateDepartmentGroup;
using Core.Application.DepartmentGroup.Command.DeleteDepartmentGroup;
using Core.Application.DepartmentGroup.Command.UpdateDepartmentGroup;
using Core.Application.DepartmentGroup.Queries.GetAllDepartmentGroup;
using Core.Application.DepartmentGroup.Queries.GetDepartmentGroupAutoSearch;
using Core.Application.DepartmentGroup.Queries.GetDepartmentGroupById;
using static Core.Domain.Enums.Common.Enums;

namespace Core.Application.Common.Mappings
{
    public class DepartmentGroupProfile : Profile
    {

        public DepartmentGroupProfile()
        {
            CreateMap<CreateDepartmentGroupCommand, Core.Domain.Entities.DepartmentGroup>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<Core.Domain.Entities.DepartmentGroup, DepartmentGroupByIdDto>();

            CreateMap<UpdateDepartmentGroupCommand, Core.Domain.Entities.DepartmentGroup>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

            CreateMap<Core.Domain.Entities.DepartmentGroup, GetAllDepartmentGroupDto>();

            CreateMap<DeleteDepartmentGroupCommand, Core.Domain.Entities.DepartmentGroup>()
             .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));
              CreateMap<Core.Domain.Entities.DepartmentGroup, DepartmentGroupAutoCompleteDto>(); 

        }

    }
}