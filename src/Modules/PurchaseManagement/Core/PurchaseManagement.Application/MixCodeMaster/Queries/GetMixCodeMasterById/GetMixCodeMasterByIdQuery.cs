using MediatR;
using PurchaseManagement.Application.MixCodeMaster.Dto;

namespace PurchaseManagement.Application.MixCodeMaster.Queries.GetMixCodeMasterById
{
    public class GetMixCodeMasterByIdQuery : IRequest<MixCodeMasterDto?>
    {
        public int Id { get; set; }
    }
}
