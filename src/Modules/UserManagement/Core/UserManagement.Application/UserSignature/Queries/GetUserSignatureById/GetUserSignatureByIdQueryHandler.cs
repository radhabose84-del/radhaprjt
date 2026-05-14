using AutoMapper;
using FluentValidation;
using MediatR;
using UserManagement.Application.Common.Interfaces.IUserSignature;
using UserManagement.Domain.Events;

namespace UserManagement.Application.UserSignature.Queries.GetUserSignatureById
{
    public class GetUserSignatureByIdQueryHandler : IRequestHandler<GetUserSignatureByIdQuery, UserSignatureByIdDto>
    {
        private readonly IUserSignatureQueryRepository _userSignatureQueryRepository;
        private readonly IUserSignatureFileStorage _fileStorage;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetUserSignatureByIdQueryHandler(
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

        public async Task<UserSignatureByIdDto> Handle(GetUserSignatureByIdQuery request, CancellationToken cancellationToken)
        {
            var userSignature = await _userSignatureQueryRepository.GetUserSignatureByIdAsync(request.Id);

            if (userSignature == null)
            {
                throw new ValidationException("UserSignature not found.");
            }

            var dto = _mapper.Map<UserSignatureByIdDto>(userSignature);
            dto.ImageBase64 = await _fileStorage.ReadAsBase64Async(userSignature.FilePath, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "USERSIGNATURE_GETBYID",
                actionName: dto.Id.ToString(),
                details: $"UserSignature with Id {dto.Id} was fetched.",
                module: "UserSignature");

            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
