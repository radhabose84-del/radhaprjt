using AutoMapper;
using Core.Application.Common.HttpResponse;
using Core.Application.Common.Interfaces.IUser;
using Core.Domain.Entities;
using Core.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core.Application.Users.Commands.DeleteUser
{
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, ApiResponseDTO<bool>>
    {
        private readonly IUserCommandRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 
        private readonly ILogger<DeleteUserCommandHandler> _logger;



        public DeleteUserCommandHandler(IUserCommandRepository userRepository, IMapper mapper, IMediator mediator,ILogger<DeleteUserCommandHandler> logger)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _mediator = mediator;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }
        public async Task<ApiResponseDTO<bool>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
             _logger.LogInformation("Starting user deletion process for UserId: {UserId}", request.UserId);

            var userToDelete = _mapper.Map<User>(request);
                // userToDelete.IsActive = request.IsActive;
            //Log Domain Event  
                    var domainEvent = new AuditLogsDomainEvent(
                        actionDetail: "Delete",
                        actionCode: userToDelete.UserName,
                        actionName: userToDelete.FirstName + " " + userToDelete.LastName,
                        details: $"User '{userToDelete.UserName}' was deleted. FirstName: {userToDelete.FirstName}, {userToDelete.LastName}",

                        module:"User"
                    );               
            await _mediator.Publish(domainEvent, cancellationToken); 

          // Perform the delete operation
            var RowsDeleted = await _userRepository.DeleteAsync(request.UserId, userToDelete);
            // bool isDeleted = RowsDeleted > 0; 

            if (RowsDeleted)
            {
                _logger.LogInformation("User with UserId: {UserId} deleted successfully.", request.UserId);
                return new ApiResponseDTO<bool>
                {
                    IsSuccess = true,
                    Message = "User deleted successfully.",
                    Data = true
                };
            }
            _logger.LogWarning("Failed to delete user with UserId: {UserId}.", request.UserId);
                return new ApiResponseDTO<bool>
                {
                    IsSuccess = false,
                    Message = "User could not be deleted.",
                    Data = false
                };          

        }
    }
}