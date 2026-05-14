using Contracts.Interfaces.Lookups.Users;
using FluentValidation;
using MediatR;
using UserManagement.Application.Common.Interfaces.IUserSignature;
using UserManagement.Domain.Events;

namespace UserManagement.Application.UserSignature.Command.CreateUserSignature
{
    public class CreateUserSignatureCommandHandler : IRequestHandler<CreateUserSignatureCommand, int>
    {
        private readonly IUserSignatureCommandRepository _userSignatureCommandRepository;
        private readonly IUserSignatureFileStorage _fileStorage;
        private readonly IUserLookup _userLookup;
        private readonly IMediator _mediator;

        public CreateUserSignatureCommandHandler(
            IUserSignatureCommandRepository userSignatureCommandRepository,
            IUserSignatureFileStorage fileStorage,
            IUserLookup userLookup,
            IMediator mediator)
        {
            _userSignatureCommandRepository = userSignatureCommandRepository;
            _fileStorage = fileStorage;
            _userLookup = userLookup;
            _mediator = mediator;
        }

        public async Task<int> Handle(CreateUserSignatureCommand request, CancellationToken cancellationToken)
        {
            if (request.File == null || request.File.Length == 0)
            {
                throw new ValidationException("Signature file is required.");
            }

            var user = await _userLookup.GetByIdAsync(request.UserId, cancellationToken)
                ?? throw new ValidationException("UserId is inactive/deleted.");

            var userNameForFile = !string.IsNullOrWhiteSpace(user.UserName) ? user.UserName : user.FirstName ?? "user";

            var saved = await _fileStorage.SaveAsync(request.File, userNameForFile, request.UserId, cancellationToken);

            var entity = new UserManagement.Domain.Entities.UserSignature
            {
                UserId = request.UserId,
                FileName = saved.FileName,
                OriginalFileName = saved.OriginalFileName,
                FilePath = saved.FilePath,
                FileType = saved.FileType,
                FileSize = saved.FileSize,
                IsActive = Domain.Enums.Common.Enums.Status.Active,
                IsDeleted = Domain.Enums.Common.Enums.IsDelete.NotDeleted
            };

            var newId = await _userSignatureCommandRepository.CreateAsync(entity);

            if (newId <= 0)
            {
                throw new Exception("UserSignature creation failed");
            }

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "USERSIGNATURE_CREATE",
                actionName: request.UserId.ToString(),
                details: $"UserSignature for User {request.UserId} created as {saved.FileName} (Id {newId}).",
                module: "UserSignature");

            await _mediator.Publish(domainEvent, cancellationToken);

            return newId;
        }
    }
}
