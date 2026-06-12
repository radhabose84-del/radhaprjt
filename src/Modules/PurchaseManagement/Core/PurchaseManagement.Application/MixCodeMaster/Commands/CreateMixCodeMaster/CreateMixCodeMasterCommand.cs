using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.MixCodeMaster.Commands.CreateMixCodeMaster
{
    public class CreateMixCodeMasterCommand : IRequest<ApiResponseDTO<int>>
    {
        public string MixCode { get; set; } = default!;
        public string MixCodeDesc { get; set; } = default!;
    }
}
