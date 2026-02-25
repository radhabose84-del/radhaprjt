using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces.IServiceMaster;
using PurchaseManagement.Application.ServiceMaster.Queries.GetAllServices;
using PurchaseManagement.Domain.Events;
using MediatR;
using Contracts.Interfaces.Lookups.Inventory;

namespace PurchaseManagement.Application.ServiceMaster.Queries.GetServiceById
{

    public class GetServiceByIdQueryHandler : IRequestHandler< GetServiceByIdQuery,  GetServiceMasterDto>
    {

        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        public readonly IServiceQueryRepository _serviceQueryRepository;
        private readonly IUOMLookup _uomLookup;
        private readonly IHSNLookup   _hSNLookup;

        public GetServiceByIdQueryHandler(IMapper mapper, IMediator mediator, IServiceQueryRepository serviceQueryRepository, IUOMLookup uomLookup, IHSNLookup hSNLookup)
        {
            _mapper = mapper;
            _mediator = mediator;
            _serviceQueryRepository = serviceQueryRepository;
            _uomLookup = uomLookup;
            _hSNLookup = hSNLookup;
        }

        public async Task<GetServiceMasterDto> Handle(GetServiceByIdQuery  request, CancellationToken cancellationToken)
        {
            var result = await _serviceQueryRepository.GetServiceMasterByIdAsync(request.Id);
            if (result == null)
            {
                throw new KeyNotFoundException($" Service with Id {request.Id} not found.");
            }
            var service = _mapper.Map<GetServiceMasterDto>(result);

            var uomsTask = _uomLookup.GetAllAsync();
            var hsnsTask = _hSNLookup.GetAllAsync();
            await Task.WhenAll(uomsTask, hsnsTask);

            var uoms = await uomsTask;                 // List<UomDto> with Id/Code/UOMName
            var hsnList = await hsnsTask;

            var uom = uoms.FirstOrDefault(x => x.Id == service.UomId);
            if (uom != null)
                service.UomName = uom.UOMName ?? uom.Code ?? service.UomName;

            var hsn = hsnList.FirstOrDefault(x => x.Id == service.SacId);
            if (hsn != null)
                service.SacName = hsn.Description ?? hsn.HSNCode ?? service.SacName;

            var domainEvent = new AuditLogsDomainEvent(
               actionDetail: "GetById",
               actionCode: "GetServiceByIdQuery",
               actionName: service.Id.ToString(),
               details: $"Service {service.Id} was fetched.",
               module: "Service"
            );
             await _mediator.Publish(domainEvent, cancellationToken);
           return service;
           
        }
    }
}
