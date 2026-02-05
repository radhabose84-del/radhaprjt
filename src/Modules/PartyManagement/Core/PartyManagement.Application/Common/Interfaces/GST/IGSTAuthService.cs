using PartyManagement.Application.GST.DTOs;

namespace PartyManagement.Application.Interfaces.GST
{
    public interface IGSTAuthService
    {
        Task<GSTAuthResponseDto> GetAuthTokenAsync();
        Task<GSTINDetailsDto> GetGSTINDetailsAsync(string gstin);
    }
}