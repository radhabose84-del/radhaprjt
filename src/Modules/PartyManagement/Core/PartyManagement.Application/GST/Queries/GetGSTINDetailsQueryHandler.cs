using PartyManagement.Application.GST.DTOs;
using PartyManagement.Application.Interfaces.GST;
using MediatR;

namespace PartyManagement.Application.GST.Queries
{
    public class GetGSTINDetailsQueryHandler : IRequestHandler<GetGSTINDetailsQuery, GSTINDetailsDto>
    {
        private readonly IGSTAuthService _gstAuthService;
        public GetGSTINDetailsQueryHandler(IGSTAuthService gstAuthService)
        {
            _gstAuthService = gstAuthService;
        }

        public async Task<GSTINDetailsDto> Handle(GetGSTINDetailsQuery request, CancellationToken cancellationToken)
        {
            return await _gstAuthService.GetGSTINDetailsAsync(request.Gstin);
        }
    }
}