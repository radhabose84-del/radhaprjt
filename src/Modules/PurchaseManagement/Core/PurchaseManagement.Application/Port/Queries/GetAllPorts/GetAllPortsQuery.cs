using PurchaseManagement.Application.Common;
using PurchaseManagement.Application.Port.Dto;
using MediatR;

namespace PurchaseManagement.Application.Port.Queries.GetAllPorts;
public sealed record GetAllPortsQuery(
    int PageNumber=1, int PageSize=10, string? Search=null, int? CountryId=null,  int? PortTypeId=null
) : IRequest<PagedResult<PortMasterDto>>;

