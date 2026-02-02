using MediatR;

namespace PurchaseManagement.Application.DutyMaster.Queries.GetDutyMasterById
{
     public record GetDutyMasterByIdQuery(int Id) : IRequest<DutyMasterViewDto?>;
}
