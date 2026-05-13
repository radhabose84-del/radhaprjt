using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.IUserSignature;
using UserManagement.Domain.Events;

namespace UserManagement.Application.UserSignature.Command.UpdateUserSignature
{
    public class UpdateUserSignatureCommandHandler : IRequestHandler<UpdateUserSignatureCommand, int>
    {
        private readonly IUserSignatureCommandRepository _userSignatureCommandRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public UpdateUserSignatureCommandHandler(
            IUserSignatureCommandRepository userSignatureCommandRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _userSignatureCommandRepository = userSignatureCommandRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<int> Handle(UpdateUserSignatureCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<UserManagement.Domain.Entities.UserSignature>(request);

            var rows = await _userSignatureCommandRepository.UpdateAsync(request.Id, entity);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "USERSIGNATURE_UPDATE",
                actionName: request.Id.ToString(),
                details: $"UserSignature with Id {request.Id} updated successfully.",
                module: "UserSignature");

            await _mediator.Publish(domainEvent, cancellationToken);

            return rows ? 1 : 0;
        }
    }
}
