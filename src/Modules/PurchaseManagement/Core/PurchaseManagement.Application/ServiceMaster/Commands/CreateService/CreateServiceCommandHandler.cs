using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PurchaseManagement.Application.Common.Exceptions;
using PurchaseManagement.Application.Common.Interfaces.IServiceMaster;
using PurchaseManagement.Application.ServiceMaster.Queries.GetAllServices;
using PurchaseManagement.Domain.Events;
using MediatR;

namespace PurchaseManagement.Application.ServiceMaster.Commands.CreateService
{
    public class CreateServiceCommandHandler : IRequestHandler<CreateServiceCommand, GetServiceMasterDto>
    {

        private readonly IServiceQueryRepository _serviceQueryRepository;
        private readonly IServiceCommandRepository _serviceCommandRepository;
        
        private readonly IMapper _imapper;
        private readonly IMediator _mediator;
        
        
     //   private readonly ICodeGenerator _codeGenerator;

        public CreateServiceCommandHandler(IServiceQueryRepository serviceQueryRepository, IServiceCommandRepository serviceCommandRepository, IMapper imapper, IMediator mediator)
        {
            _serviceQueryRepository = serviceQueryRepository;
            _serviceCommandRepository = serviceCommandRepository;
            _imapper = imapper;
            _mediator = mediator;
        }
         public async Task<GetServiceMasterDto> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
        {

            // 🔹 Map request -> entity
            var entity = _imapper.Map<PurchaseManagement.Domain.Entities.ServiceMaster>(request);

            // 🔹 Insert
            var insertResult = await _serviceCommandRepository.CreateAsync(entity , cancellationToken);

            // 🔹 Fetch the newly created record (ensures DB values are reflected)
            var created = await _serviceQueryRepository.GetServiceMasterByIdAsync(insertResult.Id);
            if (created is null)
                throw new ExceptionRules("Failed to read newly created Service Master.");

            var dto = _imapper.Map<GetServiceMasterDto>(created);

            // 🔹 Audit / Domain event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode   : dto.ServiceCode,                 // SRV0001
                actionName   : dto.ServiceDescription,
                details      : $"Service Master '{dto.ServiceCode}' was created.",
                module       : "ServiceMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            // 🔹 Return success (or throw on failure)
            return insertResult.Id <= 0
                ? throw new ExceptionRules("Failed to create Service Master.")
                : dto;
        }
    }
}