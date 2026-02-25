#nullable disable
using AutoMapper;
using UserManagement.Application.Common.Interfaces.IDivision;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Events;
using FluentValidation;
using MediatR;

namespace UserManagement.Application.Divisions.Commands.UpdateDivision
{
    public class UpdateDivisionCommandHandler : IRequestHandler<UpdateDivisionCommand, bool>
    {
        private readonly IDivisionCommandRepository _divisionRepository;
        private readonly IMapper _imapper;
        private readonly IMediator _mediator;
        private readonly IDivisionQueryRepository _divisionQueryRepository;
        public UpdateDivisionCommandHandler(IDivisionCommandRepository divisionRepository,IMapper imapper, IMediator mediator, IDivisionQueryRepository divisionQueryRepository)
        {
            _divisionRepository =divisionRepository;
            _imapper =imapper;
            _mediator = mediator;
            _divisionQueryRepository = divisionQueryRepository;
        }
          public async Task<bool> Handle(UpdateDivisionCommand request, CancellationToken cancellationToken)
        {
                var existingDivision = await _divisionQueryRepository.GetByDivisionnameAsync(request.Name, request.Id);

                if (existingDivision != null)
                {
                    throw new ValidationException("Division already exists");
                    
                }
                 var division  = _imapper.Map<Division>(request);
         
                var divisionresult = await _divisionRepository.UpdateAsync(division);

                
                    var domainEvent = new AuditLogsDomainEvent(
                        actionDetail: "Update",
                        actionCode: division.ShortName,
                        actionName: division.Name,
                        details: $"Division '{division.Id}' was updated.",
                        module:"Division"
                    );               
                    await _mediator.Publish(domainEvent, cancellationToken); 
              
                if(divisionresult)
                {
                    return divisionresult;
                }
            throw new Exception("Division not updated.");
                
            
        }
        
    }
}