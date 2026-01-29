using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.Interfaces;
using Core.Domain.Entities;
using MediatR;
using AutoMapper;
using Core.Application.Common.Interfaces.IDivision;
using Core.Application.Common.HttpResponse;
using Core.Domain.Events;
using Core.Application.Divisions.Queries.GetDivisions;

namespace Core.Application.Divisions.Commands.DeleteDivision
{
    public class DeleteDivisionCommandHandler : IRequestHandler<DeleteDivisionCommand, bool>
    {
        private readonly IDivisionCommandRepository _divisionRepository;
        private readonly IMapper _imapper;
        private readonly IMediator _mediator;
        public DeleteDivisionCommandHandler(IDivisionCommandRepository divisionRepository, IMapper imapper , IMediator mediator)
        {
            _divisionRepository = divisionRepository;
            _imapper = imapper;
            _mediator = mediator;
        }
         public async Task<bool> Handle(DeleteDivisionCommand request, CancellationToken cancellationToken)
        {
            var division  = _imapper.Map<Division>(request);
            var divisionresult = await _divisionRepository.DeleteAsync(request.Id, division);


                  //Domain Event  
                    var domainEvent = new AuditLogsDomainEvent(
                        actionDetail: "Delete",
                        actionCode: division.Id.ToString(),
                        actionName: division.Id.ToString(),
                        details: $"Division '{division.Id}' was deleted.",
                        module:"Division"
                    );               
                    await _mediator.Publish(domainEvent, cancellationToken);  

                 if(divisionresult)
                {
                    return divisionresult;
                }
            throw new Exception("Division not deleted.");
                
        }
    }
}