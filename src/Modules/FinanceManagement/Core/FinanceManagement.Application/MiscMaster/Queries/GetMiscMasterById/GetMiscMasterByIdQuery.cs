using FinanceManagement.Application.MiscMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.MiscMaster.Queries.GetMiscMasterById
{
    public class GetMiscMasterByIdQuery : IRequest<MiscMasterDto?>
    {
        public int Id { get; set; }
    }
}
