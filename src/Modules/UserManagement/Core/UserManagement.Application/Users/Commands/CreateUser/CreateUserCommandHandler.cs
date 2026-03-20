using UserManagement.Application.Users.Queries.GetUsers;
using UserManagement.Domain.Entities;
using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.IUser;
using UserManagement.Domain.Events;
using Contracts.Common;
using Microsoft.Extensions.Logging;
// using Contracts.Events.Users;


namespace UserManagement.Application.Users.Commands.CreateUser
{

    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, ApiResponseDTO<UserDto>>
    {
        private readonly IUserCommandRepository _userRepository;
        private readonly IMapper _mapper;

        private readonly IMediator _mediator;
        private readonly ILogger<CreateUserCommandHandler> _logger;


        public CreateUserCommandHandler(IUserCommandRepository userRepository, IMapper mapper, IMediator mediator, ILogger<CreateUserCommandHandler> logger)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _mediator = mediator;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        public async Task<ApiResponseDTO<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting user creation process for Username: {Username}", request.UserName);



            // Use AutoMapper to map CreateUserCommand to User entity
            var userEntity = _mapper.Map<User>(request);

               userEntity.UserType = await _userRepository.GetMiscmasterByIdAsync(
                Domain.Enums.Common.MiscEnumEntity.UserType.MiscTypeCode,
                Domain.Enums.Common.MiscEnumEntity.UserType.Internal
                );

            // Save the user to the repository
            var createdUser = await _userRepository.CreateAsync(userEntity);

            if (createdUser == null)
            {
                _logger.LogError("Failed to create user for Username: {Username}", request.UserName);

                // Optional: publish failure event for saga
                // var failedEvent = new UserCreationFailedEvent
                // {
                //     CorrelationId = Guid.NewGuid(),
                //     Reason = $"Repository returned null for user '{request.UserName}'"
                // };
                return new ApiResponseDTO<UserDto>
                {
                    IsSuccess = false,
                    Message = "Failed to create user. Please try again."
                };

            }
            _logger.LogInformation("User successfully created for Username: {Username}", createdUser.UserName);

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: createdUser.UserName ?? string.Empty,
                actionName: createdUser.FirstName + " " + createdUser.LastName,
                details: $"User '{createdUser.UserName}' was created. FirstName: {createdUser.FirstName}, {createdUser.LastName}",

                module: "User"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            // 🔥 Publish UserCreatedEvent to RabbitMQ
            // // Use the ID generated from the database
            // var userid = userEntity.UserId;
            // var userCreatedEvent = new UserCreatedEvent
            // {
            //     CorrelationId = Guid.NewGuid(),
            //     UserId = userid,
            //     UserName = createdUser.UserName,
            //     Email = createdUser.EmailId
            // };

            // Map the created user entity to DTO
            var userDto = _mapper.Map<UserDto>(createdUser);
            _logger.LogError("An exception occurred while creating user for Username: {Username}", request.UserName);
            return new ApiResponseDTO<UserDto>
            {
                IsSuccess = true,
                Message = "User created successfully",
                Data = userDto
            };


        }
    }
}
