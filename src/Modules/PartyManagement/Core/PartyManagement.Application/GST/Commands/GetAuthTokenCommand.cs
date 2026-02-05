using PartyManagement.Application.GST.DTOs;
using MediatR;

namespace PartyManagement.Application.GST.Commands
{
   public record GetAuthTokenCommand : IRequest<GSTAuthResponseDto>;
}