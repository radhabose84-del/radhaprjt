using Contracts.Common;
using MediatR;

namespace PurchaseManagement.Application.MixCodeMaster.Commands.UpdateMixCodeMaster
{
    public class UpdateMixCodeMasterCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public string MixCodeDesc { get; set; } = default!;
        public int IsActive { get; set; }  // 1 = Active, 0 = Inactive
    }
}
