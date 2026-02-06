using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using UserManagement.Application.Common.Interfaces.IEntity;
using UserManagement.Domain.Events;
using MediatR;

namespace UserManagement.Application.Entity.Queries.GetCompanyBasedUnit
{
    public class GetCompanyBasedUnitQueryHandler : IRequestHandler<GetCompanyBasedUnitQuery, List<GetCompanyBasedUnitDto>>
    {
        private readonly IEntityQueryRepository _ientityQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetCompanyBasedUnitQueryHandler(IEntityQueryRepository IentityQueryRepository, IMapper mapper, IMediator mediator)
        {
            _ientityQueryRepository = IentityQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

         public async Task<List<GetCompanyBasedUnitDto>> Handle(GetCompanyBasedUnitQuery request, CancellationToken cancellationToken)
        {
            // Fetch data from repository
            var result = await _ientityQueryRepository.GetCompanyBasedUnits(request.CompanyIds ?? new List<int>());

            // Map to DTO (optional if result already matches DTO)
            var units = _mapper.Map<List<GetCompanyBasedUnitDto>>(result);

            // Domain Event for auditing
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetCompanyBasedUnitQuery",
                actionName: units.Count.ToString(),
                details: $"Units details were fetched for CompanyIds: {string.Join(',', request.CompanyIds ?? new List<int>())}.",
                module: "Entity"
            );

            await _mediator.Publish(domainEvent, cancellationToken);

            return units;
        }
    }
}