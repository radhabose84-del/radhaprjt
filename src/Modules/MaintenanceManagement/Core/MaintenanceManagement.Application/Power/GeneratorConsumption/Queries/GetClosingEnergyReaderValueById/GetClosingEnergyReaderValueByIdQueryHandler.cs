using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.Power.IGeneratorConsumption;
using MaintenanceManagement.Application.Common.Interfaces.Power.IPowerConsumption;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.Power.GeneratorConsumption.Queries.GetClosingEnergyReaderValueById
{
    public class GetClosingEnergyReaderValueByIdQueryHandler : IRequestHandler<GetClosingEnergyReaderValueByIdQuery, GetClosingEnergyReaderValueDto>
    {
        private readonly IGeneratorConsumptionQueryRepository _generatorConsumptionQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        public GetClosingEnergyReaderValueByIdQueryHandler(IGeneratorConsumptionQueryRepository generatorConsumptionQueryRepository, IMapper mapper, IMediator mediator)
        {
            _generatorConsumptionQueryRepository = generatorConsumptionQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<GetClosingEnergyReaderValueDto> Handle(GetClosingEnergyReaderValueByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _generatorConsumptionQueryRepository.GetOpeningReaderValueById(request.GeneratorId);


            // Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetClosingEnergyReaderValueByIdQuery",
                actionCode: "GetClosingEnergyReaderValueByIdQuery",
                actionName: "Closing Reader Value Load",
                details: "Closing Reader Value details was fetched.",
                module: "Generator"
            );

            await _mediator.Publish(domainEvent, cancellationToken);

            return  result;
        }
    }
}