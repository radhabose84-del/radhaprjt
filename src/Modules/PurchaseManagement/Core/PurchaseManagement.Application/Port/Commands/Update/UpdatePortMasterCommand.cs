
using PurchaseManagement.Application.Port.Dto;
using MediatR;

namespace PurchaseManagement.Application.Port.Commands;
public sealed record UpdatePortMasterCommand(
    int Id, string PortCode, string PortName, int CountryId,  int PortTypeId, int IsActive
) : IRequest<PortMasterDto>;
