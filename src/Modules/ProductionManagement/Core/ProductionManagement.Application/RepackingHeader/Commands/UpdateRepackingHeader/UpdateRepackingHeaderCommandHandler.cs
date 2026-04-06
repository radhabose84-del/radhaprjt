using AutoMapper;
using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IRepackingHeader;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.RepackingHeader.Commands.UpdateRepackingHeader
{
    public class UpdateRepackingHeaderCommandHandler
        : IRequestHandler<UpdateRepackingHeaderCommand, ApiResponseDTO<int>>
    {
        private readonly IRepackingHeaderCommandRepository _commandRepository;
        private readonly IRepackingHeaderQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateRepackingHeaderCommandHandler(
            IRepackingHeaderCommandRepository commandRepository,
            IRepackingHeaderQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(
            UpdateRepackingHeaderCommand request,
            CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.RepackingHeader>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            bool isRepacking = request.ItemId == request.OldItemId;
            var actionCode = isRepacking ? "REPACKING_UPDATE" : "YARN_CONVERSION_UPDATE";

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: actionCode,
                actionName: request.Id.ToString(),
                details: $"RepackingHeader with Id {request.Id} updated successfully.",
                module: "RepackingHeader"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = isRepacking
                    ? "Repacking updated successfully."
                    : "Yarn Conversion updated successfully.",
                Data = result
            };
        }
    }
}
