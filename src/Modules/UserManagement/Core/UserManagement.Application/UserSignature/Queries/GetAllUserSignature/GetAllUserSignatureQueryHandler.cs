using AutoMapper;
using Contracts.Common;
using MediatR;
using UserManagement.Application.Common.Interfaces.IUserSignature;
using UserManagement.Domain.Events;

namespace UserManagement.Application.UserSignature.Queries.GetAllUserSignature
{
    public class GetAllUserSignatureQueryHandler : IRequestHandler<GetAllUserSignatureQuery, ApiResponseDTO<List<GetAllUserSignatureDto>>>
    {
        private readonly IUserSignatureQueryRepository _userSignatureQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllUserSignatureQueryHandler(
            IUserSignatureQueryRepository userSignatureQueryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _userSignatureQueryRepository = userSignatureQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<GetAllUserSignatureDto>>> Handle(GetAllUserSignatureQuery request, CancellationToken cancellationToken)
        {
            var (userSignatures, totalCount) = await _userSignatureQueryRepository
                .GetAllUserSignatureAsync(request.PageNumber, request.PageSize, request.SearchTerm);

            if (userSignatures == null || userSignatures.Count == 0)
            {
                return new ApiResponseDTO<List<GetAllUserSignatureDto>>
                {
                    IsSuccess = false,
                    Message = "No Record Found"
                };
            }

            var dtoList = _mapper.Map<List<GetAllUserSignatureDto>>(userSignatures);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "",
                actionName: "",
                details: "UserSignature details were fetched.",
                module: "UserSignature");

            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<GetAllUserSignatureDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = dtoList,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
