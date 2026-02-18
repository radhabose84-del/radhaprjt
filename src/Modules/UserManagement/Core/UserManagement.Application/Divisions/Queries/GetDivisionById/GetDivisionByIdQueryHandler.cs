using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using UserManagement.Application.Common.Interfaces;
using MediatR;
using System.Text;
using UserManagement.Application.Divisions.Queries.GetDivisions;
using UserManagement.Application.Common.Interfaces.IDivision;
using Contracts.Common;
using UserManagement.Domain.Events;

namespace UserManagement.Application.Divisions.Queries.GetDivisionById
{
    public class GetDivisionByIdQueryHandler : IRequestHandler<GetDivisionByIdQuery,DivisionDTO>
    {
         private readonly IDivisionQueryRepository _divisionRepository;        
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

         public GetDivisionByIdQueryHandler(IDivisionQueryRepository divisionRepository, IMapper mapper, IMediator mediator)
        {
            _divisionRepository = divisionRepository;
            _mapper =mapper;
            _mediator = mediator;
        } 
        public async Task<DivisionDTO> Handle(GetDivisionByIdQuery request, CancellationToken cancellationToken)
        {
            
        var result = await _divisionRepository.GetByIdAsync(request.Id);
        var division = _mapper.Map<DivisionDTO>(result);

          //Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetById",
                    actionCode: "",        
                    actionName: "",
                    details: $"Division details {division.Id} was fetched.",
                    module:"Division"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
          return division;

        }
    }
}