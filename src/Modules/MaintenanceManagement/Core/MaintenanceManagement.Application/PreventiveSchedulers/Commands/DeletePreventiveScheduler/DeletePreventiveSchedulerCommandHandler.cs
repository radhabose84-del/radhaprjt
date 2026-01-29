using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MaintenanceManagement.Application.Common.Exceptions;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Domain.Entities;
using MaintenanceManagement.Domain.Events;
using Hangfire;
using MediatR;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Commands.DeletePreventiveScheduler
{
    public class DeletePreventiveSchedulerCommandHandler : IRequestHandler<DeletePreventiveSchedulerCommand, bool>
    {
        private readonly IPreventiveSchedulerCommand _preventiveSchedulerCommand;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IPreventiveSchedulerQuery _preventiveSchedulerQuery;
        public DeletePreventiveSchedulerCommandHandler(IPreventiveSchedulerCommand preventiveSchedulerCommand, IMapper mapper, IMediator mediator, IPreventiveSchedulerQuery preventiveSchedulerQuery)
        {
            _preventiveSchedulerCommand = preventiveSchedulerCommand;
            _mapper = mapper;
            _mediator = mediator;
            _preventiveSchedulerQuery = preventiveSchedulerQuery;
        }
        public async Task<bool> Handle(DeletePreventiveSchedulerCommand request, CancellationToken cancellationToken)
        {
             var preventiveScheduler  = _mapper.Map<PreventiveSchedulerHeader>(request);
            var response = await _preventiveSchedulerCommand.DeleteAsync(request.Id,preventiveScheduler);
            var DetailResult = await _preventiveSchedulerQuery.GetPreventiveSchedulerDetail(request.Id);

            foreach (var detail in DetailResult)
            {
                     if (!string.IsNullOrEmpty(detail.HangfireJobId))
                     {
                         BackgroundJob.Delete(detail.HangfireJobId); 
                     }
        
            }
                   

                  //Domain Event  
                    var domainEvent = new AuditLogsDomainEvent(
                        actionDetail: "Delete",
                        actionCode: "delete",
                        actionName: "Delete Preventive Scheduler",
                        details: $"Delete Preventive Scheduler",
                        module:"Preventive Scheduler"
                    );               
                    await _mediator.Publish(domainEvent, cancellationToken);  

               
                    return response == true ? true : throw new ExceptionRules("Preventive Scheduler deletion failed.");
                

        }
    }
}