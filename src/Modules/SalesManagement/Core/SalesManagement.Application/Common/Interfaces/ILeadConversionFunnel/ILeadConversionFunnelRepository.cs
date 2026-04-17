using SalesManagement.Application.LeadConversionFunnel.Dto;

namespace SalesManagement.Application.Common.Interfaces.ILeadConversionFunnel
{
    public interface ILeadConversionFunnelRepository
    {
        Task<LeadConversionFunnelDto> GetFunnelAsync(CancellationToken ct = default);
    }
}
