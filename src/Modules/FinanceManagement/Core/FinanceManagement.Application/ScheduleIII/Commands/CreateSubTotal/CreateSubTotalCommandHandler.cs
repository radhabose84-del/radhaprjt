using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.CreateSubTotal
{
    public class CreateSubTotalCommandHandler : IRequestHandler<CreateSubTotalCommand, ApiResponseDTO<int>>
    {
        private readonly IScheduleIIICommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IIPAddressService _ipAddressService;

        public CreateSubTotalCommandHandler(
            IScheduleIIICommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper,
            IIPAddressService ipAddressService)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
            _ipAddressService = ipAddressService;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateSubTotalCommand request, CancellationToken cancellationToken)
        {
            var subTotal = _mapper.Map<ScheduleIIISubTotal>(request);

            // Structure identity (Company + Division) comes from the token.
            subTotal.CompanyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");
            subTotal.DivisionId = _ipAddressService.GetDivisionId()
                ?? throw new ExceptionRules("No active division in session.");

            var formulas = request.Formulas.Select(f => new ScheduleIIISubTotalFormula
            {
                OperandTypeId = f.OperandTypeId,
                OperandRefId = f.OperandRefId,
                OperatorId = f.OperatorId,
                DisplayOrder = f.DisplayOrder
            }).ToList();

            var newId = await _commandRepository.CreateSubTotalAsync(subTotal, formulas);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "S3_SUBTOTAL_CREATE",
                actionName: request.SubTotalTypeId.ToString(),
                details: $"Schedule III sub-total (type {request.SubTotalTypeId}) created successfully with Id {newId}.",
                module: "ScheduleIIISubTotal"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Sub-total created successfully.",
                Data = newId
            };
        }
    }
}
