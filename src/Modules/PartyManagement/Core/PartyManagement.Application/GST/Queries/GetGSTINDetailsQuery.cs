using PartyManagement.Application.GST.DTOs;
using MediatR;

namespace PartyManagement.Application.GST.Queries
{
    public record GetGSTINDetailsQuery(string Gstin) : IRequest<GSTINDetailsDto>;
}