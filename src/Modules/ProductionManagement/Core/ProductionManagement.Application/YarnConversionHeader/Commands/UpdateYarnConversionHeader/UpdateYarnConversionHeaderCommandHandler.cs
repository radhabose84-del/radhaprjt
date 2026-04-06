using AutoMapper;
using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IYarnConversionHeader;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.YarnConversionHeader.Commands.UpdateYarnConversionHeader
{
    public class UpdateYarnConversionHeaderCommandHandler : IRequestHandler<UpdateYarnConversionHeaderCommand, ApiResponseDTO<int>>
    {
        private readonly IYarnConversionHeaderCommandRepository _commandRepository;
        private readonly IYarnConversionHeaderQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateYarnConversionHeaderCommandHandler(
            IYarnConversionHeaderCommandRepository commandRepository,
            IYarnConversionHeaderQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateYarnConversionHeaderCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.YarnConversionHeader>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "YARN_CONVERSION_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Yarn Conversion with Id {request.Id} updated successfully.",
                module: "YarnConversionHeader"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Yarn Conversion updated successfully.",
                Data = result
            };
        }
    }
}
