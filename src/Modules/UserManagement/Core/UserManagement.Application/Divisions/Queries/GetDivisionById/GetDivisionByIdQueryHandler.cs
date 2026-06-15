using AutoMapper;
using MediatR;
using UserManagement.Application.Divisions.Queries.GetDivisions;
using UserManagement.Application.Common.Interfaces.IDivision;
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
        if (result == null)
            return null;

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