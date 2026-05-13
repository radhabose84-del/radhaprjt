using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.IUserSignature;
using UserManagement.Domain.Events;

namespace UserManagement.Application.UserSignature.Command.CreateUserSignature
{
    public class CreateUserSignatureCommandHandler : IRequestHandler<CreateUserSignatureCommand, int>
    {
        private readonly IUserSignatureCommandRepository _userSignatureCommandRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public CreateUserSignatureCommandHandler(
            IUserSignatureCommandRepository userSignatureCommandRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _userSignatureCommandRepository = userSignatureCommandRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<int> Handle(CreateUserSignatureCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<UserManagement.Domain.Entities.UserSignature>(request);

            var newId = await _userSignatureCommandRepository.CreateAsync(entity);

            if (newId <= 0)
            {
                throw new Exception("UserSignature creation failed");
            }

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "USERSIGNATURE_CREATE",
                actionName: request.UserId.ToString(),
                details: $"UserSignature for User {request.UserId} created successfully with Id {newId}.",
                module: "UserSignature");

            await _mediator.Publish(domainEvent, cancellationToken);

            return newId;
        }
    }
}
