using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAdvice;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DispatchAdvice.Commands.UpdateDispatchAdvice
{
    public class UpdateDispatchAdviceCommandHandler : IRequestHandler<UpdateDispatchAdviceCommand, ApiResponseDTO<int>>
    {
        private readonly IDispatchAdviceCommandRepository _commandRepository;
        private readonly IDispatchAdviceQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateDispatchAdviceCommandHandler(
            IDispatchAdviceCommandRepository commandRepository,
            IDispatchAdviceQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateDispatchAdviceCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<DispatchAdviceHeader>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "DISPATCHADVICE_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Dispatch Advice with Id {request.Id} updated successfully.",
                module: "DispatchAdvice");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Dispatch Advice updated successfully.",
                Data = result
            };
        }
    }
}
