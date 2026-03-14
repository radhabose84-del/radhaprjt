using Contracts.Common;
using MediatR;

namespace ProductionManagement.Application.CountMaster.Commands.CreateCountMaster
{
    public class CreateCountMasterCommand : IRequest<ApiResponseDTO<int>>
    {
        public decimal CountValue { get; set; }
        public string? ShortName { get; set; }
        public int? CountCategoryId { get; set; }
        public int CountTypeId { get; set; }
        public string? CountDescription { get; set; }
        public int UOMId { get; set; }
    }
}
