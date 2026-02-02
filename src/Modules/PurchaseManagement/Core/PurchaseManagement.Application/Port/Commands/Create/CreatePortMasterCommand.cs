using PurchaseManagement.Application.Port.Dto;
using MediatR;

namespace PurchaseManagement.Application.Port.Commands;
public sealed record CreatePortMasterCommand(
    string PortCode, string PortName, int CountryId,  int PortTypeId
) : IRequest<PortMasterDto>;
