using AutoMapper;
using Core.Application.State.Commands.CreateState;
using Core.Application.State.Queries.GetStates;
using Core.Domain.Entities;
using Core.Application.State.Commands.UpdateState;
using Core.Application.State.Commands.DeleteState;
using static Core.Domain.Enums.Common.Enums;

namespace Core.Application.Common.Mappings
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
