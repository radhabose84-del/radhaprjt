using AutoMapper;
using PurchaseManagement.Application.VendorEvaluationHeader.Commands.CreateVendorEvaluationHeader;
using PurchaseManagement.Application.VendorEvaluationHeader.Commands.UpdateVendorEvaluationHeader;
using PurchaseManagement.Application.VendorEvaluationHeader.Dto;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Application.Common.Mappings
{
    public class VendorEvaluationHeaderProfile : Profile
    {
        public VendorEvaluationHeaderProfile()
        {
            // Entity → Full DTO (header)
            CreateMap<Domain.Entities.VendorEvaluation.VendorEvaluationHeader, VendorEvaluationHeaderDto>();

            // Entity → Detail DTO
            CreateMap<Domain.Entities.VendorEvaluation.VendorEvaluationDetail, VendorEvaluationDetailDto>();

            // Create Command → Header Entity
            CreateMap<CreateVendorEvaluationHeaderCommand, Domain.Entities.VendorEvaluation.VendorEvaluationHeader>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted))
                .ForMember(dest => dest.VendorEvaluationDetails, opt => opt.MapFrom(src => src.Details));

            // Create Detail Item → Detail Entity
            CreateMap<CreateVendorEvaluationDetailItem, Domain.Entities.VendorEvaluation.VendorEvaluationDetail>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.VendorEvaluationHeaderId, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));

            // Update Command → Header Entity
            CreateMap<UpdateVendorEvaluationHeaderCommand, Domain.Entities.VendorEvaluation.VendorEvaluationHeader>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive == 1 ? Status.Active : Status.Inactive))
                .ForMember(dest => dest.VendorEvaluationDetails, opt => opt.Ignore());

            // Update Detail Item → Detail Entity
            CreateMap<UpdateVendorEvaluationDetailItem, Domain.Entities.VendorEvaluation.VendorEvaluationDetail>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.VendorEvaluationHeaderId, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => Status.Active))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => IsDelete.NotDeleted));
        }
    }
}
