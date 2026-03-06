using MediatR;
using SalesManagement.Application.DispatchAdvice.Dto;

namespace SalesManagement.Application.DispatchAdvice.Queries.GetDispatchAdvicePackNoValidation
{
    public class GetDispatchAdvicePackNoValidationQuery : IRequest<PackNoValidationDto>
    {
        public int ItemId { get; set; }
        public int LotId { get; set; }
        public int StartPackNo { get; set; }
        public int EndPackNo { get; set; }
    }
}
