using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using InventoryManagement.Application.Common.HttpResponse;
using InventoryManagement.Application.Common.Interfaces.IUOMConversion;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.UOMConversion.Queries.GetAllUOMConversion
{
    public class GetAllUOMConversionsQueryHandler  : IRequestHandler<GetAllUOMConversionsQuery, ApiResponseDTO<List<UOMConversionDto>>>
    {

         private readonly IUOMConversionQueryRepository  _iUOMConversionQueryRepository;
         private readonly IMapper _mapper;
        private readonly IMediator _mediator;


        public GetAllUOMConversionsQueryHandler(IUOMConversionQueryRepository iUOMConversionQueryRepository, IMapper mapper, IMediator mediator)
        {
            _iUOMConversionQueryRepository = iUOMConversionQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

         public async Task<ApiResponseDTO<List<UOMConversionDto>>> Handle(GetAllUOMConversionsQuery request, CancellationToken cancellationToken)
        {
            
            var (uomConversions, totalCount) = await _iUOMConversionQueryRepository.GetAllUOMConversionAsync(
                request.PageNumber,
                request.PageSize,
                request.SearchTerm
            );

          
            var conversionList = _mapper.Map<List<UOMConversionDto>>(uomConversions);

          
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetUOMConversions",
                actionCode: "",
                actionName: "",
                details: $"UOM Conversion details were fetched.",
                module: "UOMConversion"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

           
            return new ApiResponseDTO<List<UOMConversionDto>>
            {
                IsSuccess = true,
                Message = conversionList.Any() ? "Success" : "No records found",
                Data = conversionList,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}