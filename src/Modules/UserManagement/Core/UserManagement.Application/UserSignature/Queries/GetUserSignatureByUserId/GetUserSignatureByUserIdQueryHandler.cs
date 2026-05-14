using AutoMapper;
using FluentValidation;
using MediatR;
using UserManagement.Application.Common.Interfaces.IUserSignature;
using UserManagement.Application.UserSignature.Queries.GetUserSignatureById;
using UserManagement.Domain.Events;

namespace UserManagement.Application.UserSignature.Queries.GetUserSignatureByUserId
{
    public class GetUserSignatureByUserIdQueryHandler : IRequestHandler<GetUserSignatureByUserIdQuery, UserSignatureByIdDto>
    {
        private readonly IUserSignatureQueryRepository _userSignatureQueryRepository;
        private readonly IUserSignatureFileStorage _fileStorage;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetUserSignatureByUserIdQueryHandler(
            IUserSignatureQueryRepository userSignatureQueryRepository,
            IUserSignatureFileStorage fileStorage,
            IMapper mapper,
            IMediator mediator)
        {
            _userSignatureQueryRepository = userSignatureQueryRepository;
            _fileStorage = fileStorage;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<UserSignatureByIdDto> Handle(GetUserSignatureByUserIdQuery request, CancellationToken cancellationToken)
        {
            var userSignature = await _userSignatureQueryRepository.GetUserSignatureByUserIdAsync(request.UserId);

            if (userSignature == null)
            {
                throw new ValidationException("UserSignature not found for the specified user.");
            }

            var dto = _mapper.Map<UserSignatureByIdDto>(userSignature);
            dto.ImageBase64 = await _fileStorage.ReadAsBase64Async(userSignature.FilePath, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetByUserId",
                actionCode: "USERSIGNATURE_GETBYUSERID",
                actionName: request.UserId.ToString(),
                details: $"UserSignature for User {request.UserId} was fetched.",
                module: "UserSignature");

            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
