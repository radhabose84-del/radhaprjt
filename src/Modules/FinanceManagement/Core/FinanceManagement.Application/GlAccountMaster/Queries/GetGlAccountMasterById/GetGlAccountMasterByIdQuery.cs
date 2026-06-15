using FinanceManagement.Application.GlAccountMaster.Dto;
using MediatR;

namespace FinanceManagement.Application.GlAccountMaster.Queries.GetGlAccountMasterById
{
    public class GetGlAccountMasterByIdQuery : IRequest<GlAccountMasterDto?>
    {
        public int Id { get; set; }
    }
}
