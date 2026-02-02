// GetById
using PurchaseManagement.Application.Port.Dto;
using MediatR;

namespace PurchaseManagement.Application.Port.Queries.GetById;
public sealed record GetPortByIdQuery(int Id) : IRequest<PortMasterDto?>;
