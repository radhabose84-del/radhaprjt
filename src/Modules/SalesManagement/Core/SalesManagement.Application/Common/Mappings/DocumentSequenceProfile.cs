using AutoMapper;
using SalesManagement.Application.DocumentSequence.Commands.CreateDocumentSequence;
using SalesManagement.Application.DocumentSequence.Commands.UpdateDocumentSequence;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Common.Mappings
{
    public class DocumentSequenceProfile : Profile
    {
        public DocumentSequenceProfile()
        {
            CreateMap<CreateDocumentSequenceCommand, Domain.Entities.DocumentSequence>()
                .ForMember(dest => dest.IsActive,  opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            CreateMap<UpdateDocumentSequenceCommand, Domain.Entities.DocumentSequence>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                    src.IsActive == 1 ? Status.Active : Status.Inactive));
        }
    }
}
