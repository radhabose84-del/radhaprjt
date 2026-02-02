using AutoMapper;
using Contracts.Commands.Purchase;
using Contracts.Dtos.Purchase;
using PurchaseManagement.Application.IssueReturn.Command.CreateIssueReturn;
using PurchaseManagement.Application.IssueReturn.Command.UpdateIssueReturn;
using PurchaseManagement.Domain.Entities.IssueReturn;

namespace PurchaseManagement.Application.Common.Mappings.IssueReturn
{
    public class IssueReturnProfile : Profile
    {
        public IssueReturnProfile()
        {
            // DTO → Entity
            CreateMap<CreateIssueReturnDto, IssueReturnHeader>()
                .ForMember(dest => dest.IssueReturnDetailsHeaderName, opt => opt.MapFrom(src => src.IssueReturnDetails))
                .ForMember(dest => dest.IssueReturnNo, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<CreateIssueReturnDto.CreateIssueReturnDetailDto, IssueReturnDetail>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.IssueReturnHeaderDetails, opt => opt.Ignore());

            // Entity → DTO
            CreateMap<IssueReturnHeader, IssueReturnHeaderDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.IssueReturnNo, opt => opt.MapFrom(src => src.IssueReturnNo))
                .ForMember(dest => dest.IssueReturnDate, opt => opt.MapFrom(src => src.IssueReturnDate))
                .ForMember(dest => dest.UnitId, opt => opt.MapFrom(src => src.UnitId))
                .ForMember(dest => dest.RequestCategoryId, opt => opt.MapFrom(src => src.RequestCategoryId))
                .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.DepartmentId))
                .ForMember(dest => dest.Remarks, opt => opt.MapFrom(src => src.Remarks));

            CreateMap<IssueReturnDetail, IssueReturnDetailDto>();

            // Entity → ReverseMapDto (explicit header mapping)
            CreateMap<IssueReturnHeader, IssueReturnReverseMapDto>()
                .ForMember(dest => dest.Header, opt => opt.MapFrom(src => new IssueReturnHeaderDto
                {
                    Id = src.Id,
                    IssueReturnNo = src.IssueReturnNo,
                    IssueReturnDate = src.IssueReturnDate,
                    UnitId = src.UnitId,
                    RequestCategoryId = src.RequestCategoryId,
                    DepartmentId = src.DepartmentId,
                    Remarks = src.Remarks
                }))
                .ForMember(dest => dest.Lines, opt => opt.MapFrom(src => src.IssueReturnDetailsHeaderName));

            // ---------------- UPDATE MAPPINGS ----------------
            CreateMap<UpdateIssueReturnDto, IssueReturnHeader>()
                .ForMember(dest => dest.IssueReturnDetailsHeaderName, opt => opt.MapFrom(src => src.UpdateIssueReturnDetails))
                .ForMember(dest => dest.StatusId, opt => opt.Ignore())        // handled in repository
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())       // audit info handled separately
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IssueReturnNo, opt => opt.Ignore());  // don't overwrite auto-generated No

            CreateMap<UpdateIssueReturnDto.UpdateIssueReturnDetailDto, IssueReturnDetail>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())              // always insert new records
                .ForMember(dest => dest.IssueReturnHeaderDetails, opt => opt.Ignore()) // avoid circular ref
                .ForMember(dest => dest.StatusId, opt => opt.Ignore())        // assigned manually in repo
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())       // assigned manually in repo
                .ForMember(dest => dest.CreatedByName, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedIP, opt => opt.Ignore());

            CreateMap<UpdateApprovedRejectedPurchaseCommand, IssueReturnHeader>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ModuleTransactionId))
            .ForMember(dest => dest.IssueReturnDetailsHeaderName, opt => opt.MapFrom(src => src.LineStatus))
            .ForMember(dest => dest.StatusIssueHeader, opt => opt.Ignore())
            .ForSourceMember(src => src.DynamicFields, opt => opt.DoNotValidate()); 

            CreateMap<UpdateLineStatusDto, IssueReturnDetail>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ModuleLineId))
                .ForMember(dest => dest.StatusId, opt => opt.Ignore())
                .ForMember(dest => dest.IssueReturnHeaderId, opt => opt.Ignore())
                .ForMember(dest => dest.IssueReturnHeaderDetails, opt => opt.Ignore());
               
        }
    }
}