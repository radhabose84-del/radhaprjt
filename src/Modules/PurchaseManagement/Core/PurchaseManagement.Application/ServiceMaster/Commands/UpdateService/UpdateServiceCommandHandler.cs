#nullable disable
using AutoMapper;
using Contracts.Common;
using PurchaseManagement.Application.Common.Interfaces.IServiceMaster;
using PurchaseManagement.Application.ServiceMaster.Queries.GetAllServices;
using PurchaseManagement.Domain.Events;
using MediatR;

namespace PurchaseManagement.Application.ServiceMaster.Commands.UpdateService
{
    public class UpdateServiceCommandHandler : IRequestHandler<UpdateServiceCommand, GetServiceMasterDto>
    {
        private readonly IServiceQueryRepository _serviceQueryRepository;
        private readonly IServiceCommandRepository _serviceCommandRepository;        
        private readonly IMapper _imapper;
        private readonly IMediator _mediator;

        public UpdateServiceCommandHandler( IServiceQueryRepository serviceQueryRepository, IServiceCommandRepository serviceCommandRepository, IMapper imapper, IMediator mediator)
        {
            _serviceQueryRepository = serviceQueryRepository;
            _serviceCommandRepository = serviceCommandRepository;
            _imapper = imapper;
            _mediator = mediator;
        }

        public async Task<GetServiceMasterDto> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
        {
            if (request.IsActive == 0)
            {
                var isLinked = await _serviceQueryRepository.IsServiceMasterLinkedAsync(request.Id, cancellationToken);
                if (isLinked)
                    throw new ExceptionRules(
                        "This master is linked with other records. You cannot inactivate this record.");
            }

            var serviceMaster = _imapper.Map<PurchaseManagement.Domain.Entities.ServiceMaster>(request);
            var serviceMasterresult = await _serviceCommandRepository.UpdateAsync(request.Id, serviceMaster, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: serviceMaster.ServiceCode,
                actionName: serviceMaster.ServiceDescription,
                details: $"Service Master '{serviceMaster.Id}' was updated.",
                module: "ServiceMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            var dto = _imapper.Map<GetServiceMasterDto>(serviceMasterresult);
                    return dto;
               
                  //  return serviceMasterresult ? true : throw new ExceptionRules("MiscMaster updation failed.");


        }
    }
}