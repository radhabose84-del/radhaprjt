using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IFinancialYearMaster;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.FinancialYearMaster.Commands.UpdateFinancialYearMaster
{
    public class UpdateFinancialYearMasterCommandHandler : IRequestHandler<UpdateFinancialYearMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IFinancialYearMasterCommandRepository _commandRepository;
        private readonly IFinancialYearMasterQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateFinancialYearMasterCommandHandler(
            IFinancialYearMasterCommandRepository commandRepository,
            IFinancialYearMasterQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateFinancialYearMasterCommand request, CancellationToken cancellationToken)
        {
            if (request.IsActive == 0)
            {
                var isLinked = await _queryRepository.IsFinancialYearLinkedAsync(request.Id);
                if (isLinked)
                    throw new ExceptionRules(
                        "This Financial Year is linked with other records. You cannot inactivate this record.");
            }

            var entity = _mapper.Map<Domain.Entities.FinancialYearMaster>(request);

            var updatedId = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "FINANCIAL_YEAR_MASTER_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Financial Year with Id {request.Id} updated successfully.",
                module: "FinancialYearMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Financial Year updated successfully.",
                Data = updatedId
            };
        }
    }
}
