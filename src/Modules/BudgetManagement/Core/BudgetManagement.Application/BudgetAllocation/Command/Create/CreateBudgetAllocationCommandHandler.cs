using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BudgetManagement.Application.Common.Exceptions;
using BudgetManagement.Application.Common.Interfaces;
using BudgetManagement.Application.Common.Interfaces.IBudgetAllocation;
using BudgetManagement.Domain.Events;
using MediatR;

namespace BudgetManagement.Application.BudgetAllocation.Command.Create
{
    public class CreateBudgetAllocationCommandHandler : IRequestHandler<CreateBudgetAllocationCommand, int>
    {
        private readonly IBudgetAllocationCommandRepository _ibudgetAllocationCommandRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IIPAddressService _ipAddressService;

        public CreateBudgetAllocationCommandHandler(IBudgetAllocationCommandRepository ibudgetAllocationCommandRepository, IMapper mapper, IMediator mediator, IIPAddressService ipAddressService)
        {
            _ibudgetAllocationCommandRepository = ibudgetAllocationCommandRepository;
            _mapper = mapper;
            _mediator = mediator;
            _ipAddressService = ipAddressService;
        }

        public async Task<int> Handle(CreateBudgetAllocationCommand request, CancellationToken cancellationToken)
        {
            if (request.createBudgetAllocations == null || !request.createBudgetAllocations.Any())
                    throw new ExceptionRules("No Budget Allocations provided.");

                int lastId = 0;

                foreach (var dto in request.createBudgetAllocations)
                {
                    var entity = _mapper.Map<BudgetManagement.Domain.Entities.BudgetAllocation>(dto);

                    lastId = await _ibudgetAllocationCommandRepository.CreateAsync(entity);

                    // Domain event for each record
                    var domainEvent = new AuditLogsDomainEvent(
                        actionDetail: "Create",
                        actionCode: entity.Id.ToString(),
                        actionName: entity.BudgetGroupId.ToString(),
                        details: $"Budget Allocation Created",
                        module: "BudgetAllocation"
                    );

                    await _mediator.Publish(domainEvent, cancellationToken);
                }

                return lastId;
            

        }
    }
}