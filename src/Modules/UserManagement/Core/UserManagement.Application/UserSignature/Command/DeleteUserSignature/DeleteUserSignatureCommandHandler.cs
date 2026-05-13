using AutoMapper;
using FluentValidation;
using MediatR;
using UserManagement.Application.Common.Interfaces.IUserSignature;
using UserManagement.Domain.Events;

namespace UserManagement.Application.UserSignature.Command.DeleteUserSignature
{
    public class DeleteUserSignatureCommandHandler : IRequestHandler<DeleteUserSignatureCommand, bool>
    {
        private readonly IUserSignatureCommandRepository _userSignatureCommandRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public DeleteUserSignatureCommandHandler(
            IUserSignatureCommandRepository userSignatureCommandRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _userSignatureCommandRepository = userSignatureCommandRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteUserSignatureCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<UserManagement.Domain.Entities.UserSignature>(request);

            var result = await _userSignatureCommandRepository.DeleteAsync(request.Id, entity);

            if (!result)
            {
                throw new ValidationException("Failed to delete user signature.");
            }

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: "USERSIGNATURE_DELETE",
                actionName: request.Id.ToString(),
                details: $"UserSignature with Id {request.Id} was soft-deleted.",
                module: "UserSignature");

            await _mediator.Publish(domainEvent, cancellationToken);

            return true;
        }
    }
}
