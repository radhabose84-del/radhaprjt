using Core.Application.Divisions.Queries.GetDivisions;
using Core.Domain.Entities;
using Core.Application.Common.Interfaces;
using AutoMapper;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Application.Common.Interfaces.IDivision;
using Core.Application.Common.HttpResponse;
using Core.Domain.Events;
using FluentValidation;

namespace Core.Application.Divisions.Commands.CreateDivision
{
    public class CreateDivisionCommandHandler : IRequestHandler<CreateDivisionCommand, DivisionDTO>
    {
         private readonly IDivisionCommandRepository _divisionRepository;
        private readonly IMapper _imapper;
        private readonly IMediator _mediator;
        private readonly IDivisionQueryRepository _divisionQueryRepository;

        public CreateDivisionCommandHandler(IDivisionCommandRepository divisionRepository, IMapper imapper, IMediator mediator, IDivisionQueryRepository divisionQueryRepository)
        {
            _divisionRepository = divisionRepository;
            _imapper = imapper;
            _mediator = mediator;
            _divisionQueryRepository = divisionQueryRepository;
        }

        public async Task<DivisionDTO> Handle(CreateDivisionCommand request, CancellationToken cancellationToken)
        {
              var existingDivision = await _divisionQueryRepository.GetByDivisionnameAsync(request.Name);

               if (existingDivision != null)
               {
                throw new ValidationException("Division already exists");
                   
               }
           
                 var division  = _imapper.Map<Division>(request);

                var divisionresult = await _divisionRepository.CreateAsync(division);
                
                var divisionMap = _imapper.Map<DivisionDTO>(divisionresult);
                if (divisionresult.Id > 0)
                {
                    var domainEvent = new AuditLogsDomainEvent(
                     actionDetail: "Create",
                     actionCode: divisionresult.ShortName,
                     actionName: divisionresult.Name,
                     details: $"Division '{divisionresult.Name}' was created. Shortname: {divisionresult.ShortName}",
                     module:"Division"
                 );
                 await _mediator.Publish(domainEvent, cancellationToken);
                 
                    return divisionMap;
                }
               throw new Exception("Division not created");
                    
           
        }
    }
}