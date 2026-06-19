using FinanceManagement.Application.VoucherType.Dto;
using MediatR;

namespace FinanceManagement.Application.VoucherType.Queries.GetVoucherTypeById
{
    public class GetVoucherTypeByIdQuery : IRequest<VoucherTypeMasterDto?>
    {
        public int Id { get; set; }
    }
}
