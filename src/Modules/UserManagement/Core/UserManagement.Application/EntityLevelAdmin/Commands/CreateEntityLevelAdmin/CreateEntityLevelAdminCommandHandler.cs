#nullable disable
using AutoMapper;
using UserManagement.Application.Common.Interfaces.IUser;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Events;
using FluentValidation;
using MediatR;

namespace UserManagement.Application.EntityLevelAdmin.Commands.CreateEntityLevelAdmin
{
    public class CreateEntityLevelAdminCommandHandler : IRequestHandler<CreateEntityLevelAdminCommand, int>
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IUserCommandRepository _userRepository;
        private readonly IUserQueryRepository _userQueryRepository;
        public CreateEntityLevelAdminCommandHandler(IMediator mediator, IUserCommandRepository userRepository, IMapper mapper, IUserQueryRepository userQueryRepository)
        {
            _mediator = mediator;
            _userRepository = userRepository;
            _mapper = mapper;
            _userQueryRepository = userQueryRepository;
        }
        public async Task<int> Handle(CreateEntityLevelAdminCommand request, CancellationToken cancellationToken)
        {
            var existingUser = await _userQueryRepository.GetByUsernameAsync(request.Email);
            if (existingUser != null)
            {
                throw new ValidationException("User already exists.");
             
            }
            

            var userEntity = _mapper.Map<User>(request);
            

            var createdUser = await _userRepository.CreateAsync(userEntity);

              if (createdUser == null)
            {
                throw new Exception("Failed to create user. Please try again.");
              
            }
             var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "Create Entity Level Admin",
                actionName: "Create Entity Level Admin",
                details: $"User '{createdUser.UserName}' was created.",
                module:"User"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

             return createdUser.UserId;
        }
    }
}