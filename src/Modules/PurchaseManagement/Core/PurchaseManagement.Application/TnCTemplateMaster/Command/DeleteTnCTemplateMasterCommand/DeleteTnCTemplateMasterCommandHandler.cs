using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PurchaseManagement.Application.Common.Exceptions;
using PurchaseManagement.Application.Common.Interfaces.ITnCTemplateMaster;
using PurchaseManagement.Domain.Events;
using MediatR;

namespace PurchaseManagement.Application.TnCTemplateMaster.Command.DeleteTnCTemplateMasterCommand
{
    public class DeleteTnCTemplateMasterCommandHandler : IRequestHandler<DeleteTnCTemplateMasterCommand, bool>
    {
        private readonly ITnCTemplateMasterQueryRepository _tnCTemplateMasterQueryRepository;
        private readonly ITnCTemplateMasterCommandRepository _tnCTemplateMasterCommandRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public DeleteTnCTemplateMasterCommandHandler(ITnCTemplateMasterQueryRepository tnCTemplateMasterQueryRepository, ITnCTemplateMasterCommandRepository tnCTemplateMasterCommandRepository, IMapper mapper, IMediator mediator)
        {
            _tnCTemplateMasterQueryRepository = tnCTemplateMasterQueryRepository;
            _tnCTemplateMasterCommandRepository = tnCTemplateMasterCommandRepository;           
            _mapper = mapper;
            _mediator = mediator;
        }

        public  async Task<bool> Handle(DeleteTnCTemplateMasterCommand request , CancellationToken cancellationToken)
        {
                var existing = await _tnCTemplateMasterQueryRepository.GetByIdAsync(request.Id);
            if (existing is null)
                throw new ExceptionRules($"T&C Template not found for Id={request.Id}.");

            var ok = await _tnCTemplateMasterCommandRepository.SoftDeleteAsync(request.Id);
            if (!ok) throw new ExceptionRules("Delete failed.");

            // publish audit log
            var evt = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: existing.TemplateCode ?? string.Empty,
                actionName: existing.TemplateName,
                details: $"TnCTemplateMaster '{existing.TemplateName}' deleted (Id={existing.Id}).",
                module: "TnCTemplateMaster"
            );
            await _mediator.Publish(evt);

            return true;
        }
    }
}