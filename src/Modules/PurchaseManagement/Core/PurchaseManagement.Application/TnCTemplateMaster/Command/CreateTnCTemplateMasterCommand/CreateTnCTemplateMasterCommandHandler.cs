using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using PurchaseManagement.Application.Common.Interfaces.ITnCTemplateMaster;
using PurchaseManagement.Domain.Events;
using MediatR;

namespace PurchaseManagement.Application.TnCTemplateMaster.Command.CreateTnCTemplateMasterCommand
{
    public class CreateTnCTemplateMasterCommandHandler : IRequestHandler<CreateTnCTemplateMasterCommand, int>
    {

          private readonly ITnCTemplateMasterQueryRepository _tnCTemplateMasterQueryRepository;
          private readonly ITnCTemplateMasterCommandRepository _tnCTemplateMasterCommandRepository;
          private readonly ITnCTemplateCodeGenerator _codeGen;
          private readonly IMapper _mapper;
          private readonly IMediator _mediator;
         public CreateTnCTemplateMasterCommandHandler( ITnCTemplateMasterQueryRepository tnCTemplateMasterQueryRepository, ITnCTemplateMasterCommandRepository tnCTemplateMasterCommandRepository, ITnCTemplateCodeGenerator codeGen, IMapper mapper, IMediator mediator)
        {
            _tnCTemplateMasterQueryRepository = tnCTemplateMasterQueryRepository;
            _tnCTemplateMasterCommandRepository = tnCTemplateMasterCommandRepository;
            _codeGen = codeGen;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<int> Handle(CreateTnCTemplateMasterCommand request, CancellationToken cancellationToken)
        {
      


             var entity = _mapper.Map<PurchaseManagement.Domain.Entities.TnCTemplateMaster>(request);
            
            // Generate TemplateCode here (since command no longer has it)
             entity.TemplateCode = await _codeGen.GenerateAsync(request.TemplateTypeId, request.TemplateName);


            // persist (repo should save master + children in a transaction)
            var newId = await _tnCTemplateMasterCommandRepository.CreateAsync(entity, cancellationToken).ConfigureAwait(false);
            if (newId <= 0)
                throw new ExceptionRules("Failed to create T&C Template.");

            // audit event (adjust names to your event type)
            var evt = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: entity.TemplateCode ?? string.Empty,
                actionName: entity.TemplateName,
                details: $"TnCTemplateMaster '{entity.TemplateName}' created (Id={newId}).",
                module: "TnCTemplateMaster"
            );
            await _mediator.Publish(evt, cancellationToken).ConfigureAwait(false);

            return newId;
        }
    }
}