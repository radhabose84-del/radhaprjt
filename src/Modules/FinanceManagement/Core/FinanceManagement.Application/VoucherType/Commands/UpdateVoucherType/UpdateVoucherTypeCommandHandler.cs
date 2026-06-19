using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IVoucherTypeMaster;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.VoucherType.Commands.UpdateVoucherType
{
    public class UpdateVoucherTypeCommandHandler : IRequestHandler<UpdateVoucherTypeCommand, ApiResponseDTO<int>>
    {
        private readonly IVoucherTypeMasterCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateVoucherTypeCommandHandler(
            IVoucherTypeMasterCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateVoucherTypeCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.VoucherTypeMaster>(request);

            var updatedId = await _commandRepository.UpdateAsync(entity, request.AllowedAccountTypeIds);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "VOUCHER_TYPE_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Voucher Type with Id {request.Id} updated successfully.",
                module: "VoucherType"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Voucher Type updated successfully.",
                Data = updatedId
            };
        }
    }
}
