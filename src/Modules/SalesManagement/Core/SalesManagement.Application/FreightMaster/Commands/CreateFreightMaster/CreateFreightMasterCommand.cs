using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.FreightMaster.Commands.CreateFreightMaster
{
    public class CreateFreightMasterCommand : IRequest<ApiResponseDTO<int>>
    {
        public int FreightModeId { get; set; }
        public int RateMethodId { get; set; }
        public decimal Rate { get; set; }
    }
}
