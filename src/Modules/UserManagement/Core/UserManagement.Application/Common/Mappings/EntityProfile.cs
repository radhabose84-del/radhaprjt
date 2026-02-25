using AutoMapper;
using UserManagement.Application.Entity.Queries.GetEntity;
using UserManagement.Application.Entity.Commands.CreateEntity;
using UserManagement.Application.Entity.Commands.UpdateEntity;
using UserManagement.Application.Entity.Commands.DeleteEntity;
using static UserManagement.Domain.Enums.Common.Enums;


namespace UserManagement.Application.Common.Mappings
{
    public class EntityProfile : Profile
    {
       public EntityProfile()
        {
            CreateMap<UserManagement.Domain.Entities.Entity,EntityAutoCompleteDto>();

            
            CreateMap<CreateEntityCommand, UserManagement.Domain.Entities.Entity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            
                .ForMember(dest => dest.EntityName, opt => opt.MapFrom(src => src.EntityName))
                .ForMember(dest => dest.EntityDescription, opt => opt.MapFrom(src => src.EntityDescription))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

              
                
            CreateMap<UpdateEntityCommand, UserManagement.Domain.Entities.Entity>()
             .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));

            CreateMap<DeleteEntityCommand, UserManagement.Domain.Entities.Entity>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.EntityId)) 
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));     

            CreateMap<UserManagement.Domain.Entities.Entity, GetEntityDTO>();
        }

    }
}