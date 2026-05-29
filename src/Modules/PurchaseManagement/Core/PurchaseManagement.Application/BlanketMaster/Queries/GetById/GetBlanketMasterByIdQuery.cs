using MediatR;
using PurchaseManagement.Application.BlanketMaster.Dto;

namespace PurchaseManagement.Application.BlanketMaster.Queries.GetById;

public sealed record GetBlanketMasterByIdQuery(int Id) : IRequest<BlanketHeaderDto?>;
