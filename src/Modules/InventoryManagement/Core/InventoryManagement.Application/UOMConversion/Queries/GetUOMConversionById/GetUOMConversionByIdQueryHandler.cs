using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.IUOMConversion;
using InventoryManagement.Application.MiscMaster.Queries.GetMiscMasterById;
using InventoryManagement.Application.UOMConversion.Queries.GetAllUOMConversion;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.UOMConversion.Queries.GetUOMConversionById
{
    public class GetUOMConversionByIdQueryHandler : IRequestHandler<GetUOMConversionByIdQuery, UOMConversionDto>
    {
        private readonly IUOMConversionQueryRepository _iUOMConversionQueryRepository;
        private readonly IMapper _mapper;

         private readonly IMediator _mediator;

        public GetUOMConversionByIdQueryHandler(IUOMConversionQueryRepository iUOMConversionQueryRepository, IMapper mapper, IMediator mediator)
        {
            _iUOMConversionQueryRepository = iUOMConversionQueryRepository;
            _mapper = mapper;
            _mediator = mediator;

        }

        public async Task<UOMConversionDto> Handle(GetUOMConversionByIdQuery request, CancellationToken cancellationToken)
        {

            var result = await _iUOMConversionQueryRepository.GetByIdAsync(request.Id);
            var uomConversion = _mapper.Map<UOMConversionDto>(result);

               var domainEvent = new AuditLogsDomainEvent(
                        actionDetail: "GetById",
                        actionCode: "",        
                        actionName: "",
                        details: $"UOM Conversion details {uomConversion.Id} was fetched.",
                        module:"UOM Conversion"
                    );
                    await _mediator.Publish(domainEvent, cancellationToken);
            return  uomConversion;
       

        }
    }
}