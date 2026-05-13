using Contracts.Interfaces.Lookups.Users;
using FluentValidation;
using MediatR;
using UserManagement.Application.Common.Interfaces.IUserSignature;
using UserManagement.Domain.Events;

namespace UserManagement.Application.UserSignature.Command.UpdateUserSignature
{
    public class UpdateUserSignatureCommandHandler : IRequestHandler<UpdateUserSignatureCommand, int>
    {
        private readonly IUserSignatureCommandRepository _userSignatureCommandRepository;
        private readonly IUserSignatureQueryRepository _userSignatureQueryRepository;
        private readonly IUserSignatureFileStorage _fileStorage;
        private readonly IUserLookup _userLookup;
        private readonly IMediator _mediator;

        public UpdateUserSignatureCommandHandler(
            IUserSignatureCommandRepository userSignatureCommandRepository,
            IUserSignatureQueryRepository userSignatureQueryRepository,
            IUserSignatureFileStorage fileStorage,
            IUserLookup userLookup,
            IMediator mediator)
        {
            _userSignatureCommandRepository = userSignatureCommandRepository;
            _userSignatureQueryRepository = userSignatureQueryRepository;
            _fileStorage = fileStorage;
            _userLookup = userLookup;
            _mediator = mediator;
        }

        public async Task<int> Handle(UpdateUserSignatureCommand request, CancellationToken cancellationToken)
        {
            var existing = await _userSignatureQueryRepository.GetUserSignatureByIdAsync(request.Id)
                ?? throw new ValidationException("UserSignature not found.");

            var newFileName = existing.FileName;
            var newOriginalFileName = existing.OriginalFileName;
            var newFilePath = existing.FilePath;
            var newFileType = existing.FileType;
            var newFileSize = existing.FileSize;

            if (request.File != null && request.File.Length > 0)
            {
                var user = await _userLookup.GetByIdAsync(existing.UserId, cancellationToken)
                    ?? throw new ValidationException("Linked User no longer exists.");

                var userNameForFile = !string.IsNullOrWhiteSpace(user.UserName) ? user.UserName : user.FirstName ?? "user";
                var saved = await _fileStorage.SaveAsync(request.File, userNameForFile, existing.UserId, cancellationToken);

                newFileName = saved.FileName;
                newOriginalFileName = saved.OriginalFileName;
                newFilePath = saved.FilePath;
                newFileType = saved.FileType;
                newFileSize = saved.FileSize;
            }

            var entity = new UserManagement.Domain.Entities.UserSignature
            {
                Id = request.Id,
                UserId = existing.UserId, // immutable
                FileName = newFileName,
                OriginalFileName = newOriginalFileName,
                FilePath = newFilePath,
                FileType = newFileType,
                FileSize = newFileSize,
                IsActive = request.IsActive,
                IsDeleted = Domain.Enums.Common.Enums.IsDelete.NotDeleted
            };

            var rows = await _userSignatureCommandRepository.UpdateAsync(request.Id, entity);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "USERSIGNATURE_UPDATE",
                actionName: request.Id.ToString(),
                details: $"UserSignature with Id {request.Id} updated.",
                module: "UserSignature");

            await _mediator.Publish(domainEvent, cancellationToken);

            return rows ? 1 : 0;
        }
    }
}
