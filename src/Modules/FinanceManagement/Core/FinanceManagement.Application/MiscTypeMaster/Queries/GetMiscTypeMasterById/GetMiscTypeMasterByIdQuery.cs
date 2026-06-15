using FinanceManagement.Application.MiscTypeMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterById
{
    public class GetMiscTypeMasterByIdQuery : IRequest<MiscTypeMasterDto?>
    {
        public int Id { get; set; }
    }
}
