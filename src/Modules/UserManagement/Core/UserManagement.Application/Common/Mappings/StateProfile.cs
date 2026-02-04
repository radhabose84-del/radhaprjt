using AutoMapper;
using UserManagement.Application.State.Commands.CreateState;
using UserManagement.Application.State.Queries.GetStates;
using UserManagement.Domain.Entities;
using UserManagement.Application.State.Commands.UpdateState;
using UserManagement.Application.State.Commands.DeleteState;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Application.Common.Mappings
{
    public class StateProfile : Profile
    {
        
        public StateProfile()
        {            
            CreateMap<CreateStateCommand, States>()
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))            
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));                        
            CreateMap<UpdateStateCommand, States>()
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive ==1 ? Status.Active : Status.Inactive));    
            CreateMap<DeleteStateCommand, States>()            
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.Deleted));   
             CreateMap<States, StateAutoCompleteDTO>();             
        }
    }
}    
