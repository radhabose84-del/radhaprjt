using AutoMapper;
using Contracts.Common;
using PurchaseManagement.Application.Common.Interfaces.ITnCTemplateMaster;
using PurchaseManagement.Domain.Events;
using MediatR;

namespace PurchaseManagement.Application.TnCTemplateMaster.Command.UpdateTnCTemplateMasterCommand
{
    public class UpdateTnCTemplateMasterCommandHandler : IRequestHandler<UpdateTnCTemplateMasterCommand, bool>
    {
        private readonly ITnCTemplateMasterQueryRepository _tnCTemplateMasterQueryRepository;
        private readonly ITnCTemplateMasterCommandRepository _tnCTemplateMasterCommandRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public UpdateTnCTemplateMasterCommandHandler(ITnCTemplateMasterQueryRepository tnCTemplateMasterQueryRepository, ITnCTemplateMasterCommandRepository tnCTemplateMasterCommandRepository, IMapper mapper, IMediator mediator)
        {
            _tnCTemplateMasterQueryRepository = tnCTemplateMasterQueryRepository;
            _tnCTemplateMasterCommandRepository = tnCTemplateMasterCommandRepository;
            _mapper = mapper;
            _mediator = mediator;
        }
        
        public async Task<bool> Handle(UpdateTnCTemplateMasterCommand request, CancellationToken cancellationToken)
        {
           
            var existing = await _tnCTemplateMasterQueryRepository.GetByIdAsync(request.Id);

            if (existing is null)
                throw new ExceptionRules($"T&C Template not found for Id={request.Id}.");

          
            var incoming = _mapper.Map<PurchaseManagement.Domain.Entities.TnCTemplateMaster>(request);

          
            incoming.TemplateCode = existing.TemplateCode;

           
            var desiredApplicabilities = (request.Applicabilities is { Count: > 0 })
                ? _mapper.Map<List<PurchaseManagement.Domain.Entities.TnCTemplateApplicability>>(request.Applicabilities)
                : new List<PurchaseManagement.Domain.Entities.TnCTemplateApplicability>();

            
            foreach (var child in desiredApplicabilities)
            {
                child.TnCTemplateMasterId = request.Id;
               
            }

           
            var ok = await _tnCTemplateMasterCommandRepository.UpdateAsync(incoming, desiredApplicabilities);
            if (!ok)
                throw new ExceptionRules("T&C Template update failed.");

           
            var evt = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: incoming.TemplateCode ?? string.Empty,
                actionName: incoming.TemplateName,
                details: $"TnCTemplateMaster '{incoming.TemplateName}' updated (Id={incoming.Id}).",
                module: "TnCTemplateMaster"
            );

            await _mediator.Publish(evt, cancellationToken);

            return true;
        }


        // public async Task<bool> Handle(UpdateTnCTemplateMasterCommand request, CancellationToken cancellationToken)
        // {
        //        // Ensure it exists (read via query repo)
        //     var existing = await _tnCTemplateMasterQueryRepository.GetByIdAsync(request.Id);
        //     if (existing is null)
        //         throw new ExceptionRules($"T&C Template not found for Id={request.Id}.");

        //     // Prepare incoming master (like your PaymentTerm example)
        //     var incoming = new PurchaseManagement.Domain.Entities.TnCTemplateMaster
        //     {
        //         Id = request.Id,
        //         //   TemplateCode   = request.TemplateCode,
        //         TemplateName = request.TemplateName.Trim(),
        //         TemplateTypeId = request.TemplateTypeId,
        //         TermsHtml = request.TermsHtml,
        //         ApprovalFlag = request.ApprovalFlag

        //     };

        //     // Rebuild desired child list from command (simple DTO -> entity projection)
        //     var desiredApplicabilities = request.Applicabilities?
        //         .Select(a => new PurchaseManagement.Domain.Entities.TnCTemplateApplicability
        //         {
        //             TnCTemplateMasterId = request.Id,          // for repo convenience
        //             ApplicabilityId     = a.ApplicabilityId
        //         })
        //         .ToList() ?? new();

        //     // Persist via command repo (repo will do set-diff add/revive/soft-delete)
        //     var ok = await _tnCTemplateMasterCommandRepository.UpdateAsync(incoming, desiredApplicabilities);
        //     if (!ok) throw new ExceptionRules("T&C Template update failed.");

        //     // Audit
        //     var evt = new AuditLogsDomainEvent(
        //         actionDetail: "Update",
        //         actionCode: incoming.TemplateCode ?? string.Empty,
        //         actionName: incoming.TemplateName,
        //         details: $"TnCTemplateMaster '{incoming.TemplateName}' updated (Id={incoming.Id}).",
        //         module: "TnCTemplateMaster"
        //     );
        //     await _mediator.Publish(evt, cancellationToken);

        //     return true;
        // }
    }
}