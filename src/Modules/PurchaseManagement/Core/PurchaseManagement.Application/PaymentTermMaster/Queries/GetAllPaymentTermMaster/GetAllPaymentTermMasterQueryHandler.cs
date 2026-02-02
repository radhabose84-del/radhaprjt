using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PurchaseManagement.Application.Common.HttpResponse;
using PurchaseManagement.Application.Common.Interfaces.IPaymentTermMaster;
using MediatR;

namespace PurchaseManagement.Application.PaymentTermMaster.Queries.GetAllPaymentTermMaster
{
    public class GetAllPaymentTermMasterQueryHandler : IRequestHandler<GetAllPaymentTermMasterQuery, ApiResponseDTO<List<PaymentTermMasterDto>>>
    {

        private readonly IPaymentTermMasterQueryRepository _paymentTermMasterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllPaymentTermMasterQueryHandler(IPaymentTermMasterQueryRepository paymentTermMasterQueryRepository, IMapper mapper, IMediator mediator)
        {
            _paymentTermMasterQueryRepository = paymentTermMasterQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }


        
         public async Task<ApiResponseDTO<List<PaymentTermMasterDto>>> Handle( GetAllPaymentTermMasterQuery request,  CancellationToken cancellationToken)
        {
            // Fetch page from Dapper repo
            var (items, totalCount) = await _paymentTermMasterQueryRepository
                .GetAllPaymentTermMasterAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            
            return new ApiResponseDTO<List<PaymentTermMasterDto>>
            {
                StatusCode = 200,
                Message = items.Count > 0 ? "Payment terms fetched successfully." : "No payment terms found.",
                Data = items,
                // The following are optional—include only if your ApiResponseDTO supports them:
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}