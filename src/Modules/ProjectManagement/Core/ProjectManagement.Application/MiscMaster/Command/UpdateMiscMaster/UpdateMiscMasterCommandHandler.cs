#nullable disable
using AutoMapper;
using Contracts.Common;
using ProjectManagement.Application.Common.Interfaces.IMiscMaster;
using ProjectManagement.Domain.Events;
using MediatR;

namespace ProjectManagement.Application.MiscMaster.Command.UpdateMiscMaster
{
    public class UpdateMiscMasterCommandHandler  : IRequestHandler<UpdateMiscMasterCommand, bool>
    {

         private readonly IMiscMasterCommandRepository _miscMasterCommandRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;

         private readonly IMapper _imapper;
        private readonly IMediator _mediator;

         public UpdateMiscMasterCommandHandler(IMiscMasterCommandRepository miscMasterCommandRepository ,IMiscMasterQueryRepository miscMasterQueryRepository ,IMapper imapper, IMediator mediator)
        {
            _miscMasterCommandRepository =miscMasterCommandRepository;
            _imapper =imapper;
            _mediator = mediator;
            _miscMasterQueryRepository = miscMasterQueryRepository;
        }


           public async Task<bool> Handle(UpdateMiscMasterCommand request, CancellationToken cancellationToken)
        {
                if (request.IsActive == 0)
                {
                    var isLinked = await _miscMasterQueryRepository.IsMiscMasterLinkedAsync(request.Id);
                    if (isLinked)
                        throw new ExceptionRules(
                            "This master is linked with other records. You cannot inactivate this record.");
                }

                 var miscmaster  = _imapper.Map<ProjectManagement.Domain.Entities.MiscMaster>(request);         
                var MiscMasterresult = await _miscMasterCommandRepository.UpdateAsync(request.Id, miscmaster);                

                    var domainEvent = new AuditLogsDomainEvent(
                        actionDetail: "Update",
                        actionCode: miscmaster.Code,
                        actionName: miscmaster.Description,
                        details: $"MiscMaster '{miscmaster.Id}' was updated.",
                        module:"MiscMaster"
                    );               
                    await _mediator.Publish(domainEvent, cancellationToken); 
              
               
                    return MiscMasterresult ? true : throw new ExceptionRules("MiscMaster updation failed.");
                
           
        }
    }
}