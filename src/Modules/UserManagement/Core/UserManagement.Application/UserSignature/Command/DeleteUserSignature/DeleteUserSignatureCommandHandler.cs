using FluentValidation;
using MediatR;
using UserManagement.Application.Common.Interfaces.IUserSignature;
using UserManagement.Domain.Events;

namespace UserManagement.Application.UserSignature.Command.DeleteUserSignature
{
    public class DeleteUserSignatureCommandHandler : IRequestHandler<DeleteUserSignatureCommand, bool>
    {
        private readonly IUserSignatureCommandRepository _userSignatureCommandRepository;
        private readonly IUserSignatureQueryRepository _userSignatureQueryRepository;
        private readonly IUserSignatureFileStorage _fileStorage;
        private readonly IMediator _mediator;

        public DeleteUserSignatureCommandHandler(
            IUserSignatureCommandRepository userSignatureCommandRepository,
            IUserSignatureQueryRepository userSignatureQueryRepository,
            IUserSignatureFileStorage fileStorage,
            IMediator mediator)
        {
            _userSignatureCommandRepository = userSignatureCommandRepository;
            _userSignatureQueryRepository = userSignatureQueryRepository;
            _fileStorage = fileStorage;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteUserSignatureCommand request, CancellationToken cancellationToken)
        {
            var existing = await _userSignatureQueryRepository.GetUserSignatureByIdAsync(request.Id)
                ?? throw new ValidationException("UserSignature not found.");

            var entity = new UserManagement.Domain.Entities.UserSignature
            {
                Id = request.Id,
                IsDeleted = Domain.Enums.Common.Enums.IsDelete.Deleted
            };

            var result = await _userSignatureCommandRepository.DeleteAsync(request.Id, entity);

            if (!result)
            {
                throw new ValidationException("Failed to delete user signature.");
            }

            // Remove the file from disk after successful soft delete
            if (!string.IsNullOrWhiteSpace(existing.FilePath))
            {
                await _fileStorage.DeleteAsync(existing.FilePath, cancellationToken);
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
