
using Contracts.Dtos.Common;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.Reports;
public sealed record QueuePoPdfEmailCommand(
    int UnitId,
    int PoId,  
    List<PartyRefDto> PartyContacts,  
    string RowsJson = ""
) : IRequest<Guid>;
