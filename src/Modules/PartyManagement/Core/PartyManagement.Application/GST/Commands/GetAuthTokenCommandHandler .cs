using PartyManagement.Application.GST.Commands;
using PartyManagement.Application.GST.DTOs;
using PartyManagement.Application.Interfaces.GST;
using MediatR;

namespace PartyManagement.Application.GST.Commands
{
   public class GetAuthTokenCommandHandler  : IRequestHandler<GetAuthTokenCommand, GSTAuthResponseDto>
    {
        private readonly IGSTAuthService _gstAuthService;
        public GetAuthTokenCommandHandler (IGSTAuthService gstAuthService)
        {
            _gstAuthService = gstAuthService;
        }

        public async Task<GSTAuthResponseDto> Handle(GetAuthTokenCommand request, CancellationToken cancellationToken)
        {
            return await _gstAuthService.GetAuthTokenAsync();
        }
    }
}
