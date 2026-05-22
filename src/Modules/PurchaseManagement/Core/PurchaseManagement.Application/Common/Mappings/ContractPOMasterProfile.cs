using AutoMapper;
using PurchaseManagement.Application.ContractPOMaster.Commands.Create;
using PurchaseManagement.Application.ContractPOMaster.Commands.Update;
using PurchaseManagement.Application.ContractPOMaster.Dto;
using PurchaseManagement.Domain.Entities.ContractPOMaster;

namespace PurchaseManagement.Application.Common.Mappings;

public sealed class ContractPOMasterProfile : Profile
{
    public ContractPOMasterProfile()
    {
        // Header mappings
        CreateMap<CreateContractPOMasterCommand, ContractPOHeader>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UnitId, o => o.Ignore())
            .ForMember(d => d.ContractPONumber, o => o.Ignore())
            .ForMember(d => d.TotalContractValue, o => o.Ignore())
            .ForMember(d => d.UtilizedValue, o => o.Ignore())
            .ForMember(d => d.BalanceValue, o => o.Ignore())
            .ForMember(d => d.ContractPODetails, o => o.Ignore())
            .ForMember(d => d.StatusId, o => o.Ignore())
            .ForMember(d => d.ContractPOReleaseHistories, o => o.Ignore())
            .ForMember(d => d.MiscStatus, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedDate, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore());

        CreateMap<UpdateContractPOMasterCommand, ContractPOHeader>()
            .ForMember(d => d.ContractPONumber, o => o.Ignore())
            .ForMember(d => d.UnitId, o => o.Ignore())
            .ForMember(d => d.ContractDate, o => o.Ignore())
            .ForMember(d => d.TotalContractValue, o => o.Ignore())
            .ForMember(d => d.UtilizedValue, o => o.Ignore())
            .ForMember(d => d.BalanceValue, o => o.Ignore())
            .ForMember(d => d.ContractPODetails, o => o.Ignore())
            .ForMember(d => d.ContractPOReleaseHistories, o => o.Ignore())
            .ForMember(d => d.MiscStatus, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore())
            .ForMember(d => d.IsActive, o => o.MapFrom(s => s.IsActive == 1 ? 1 : 0));

        // Detail mappings
        CreateMap<CreateContractPODetailItem, ContractPODetail>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.ContractPOHeaderId, o => o.Ignore())
            .ForMember(d => d.ContractPOHeader, o => o.Ignore())
            .ForMember(d => d.ContractValue, o => o.Ignore())
            .ForMember(d => d.UtilizedQuantity, o => o.Ignore())
            .ForMember(d => d.BalanceQuantity, o => o.Ignore())
            .ForMember(d => d.UtilizedValue, o => o.Ignore())
            .ForMember(d => d.BalanceValue, o => o.Ignore())
            .ForMember(d => d.ContractPOReleaseHistories, o => o.Ignore())
            .ForMember(d => d.CreatedDate, o => o.Ignore())
            .ForMember(d => d.ModifiedBy, o => o.Ignore())
            .ForMember(d => d.ModifiedDate, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore());

        // DTO mappings (entity/raw → DTO for read-back)
        CreateMap<ContractPOHeader, ContractPOHeaderDto>();
        CreateMap<ContractPODetail, ContractPODetailDto>();
        CreateMap<ContractPOReleaseHistory, ContractPOReleaseHistoryDto>();
    }
}
