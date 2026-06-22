using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.Common;
using FinanceManagement.Application.Common.Interfaces.IVoucherTypeMaster;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.VoucherType.Commands.CreateVoucherType
{
    public class CreateVoucherTypeCommandHandler : IRequestHandler<CreateVoucherTypeCommand, ApiResponseDTO<int>>
    {
        private readonly IVoucherTypeMasterCommandRepository _commandRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IFinancialYearLookup _financialYearLookup;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateVoucherTypeCommandHandler(
            IVoucherTypeMasterCommandRepository commandRepository,
            IIPAddressService ipAddressService,
            IFinancialYearLookup financialYearLookup,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _ipAddressService = ipAddressService;
            _financialYearLookup = financialYearLookup;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateVoucherTypeCommand request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var entity = _mapper.Map<Domain.Entities.VoucherTypeMaster>(request);
            entity.CompanyId = companyId;

            // Seed the counter for the CURRENT fiscal year (resolved from the finyear lookup), so the
            // saved series matches what the list shows. Other FYs' counters are created lazily.
            var years = await _financialYearLookup.GetAllFinancialYearAsync();
            var currentFinancialYearId = FinancialYearResolver.ResolveCurrent(years)?.FinancialYearId;

            var newId = await _commandRepository.CreateAsync(entity, request.AllowedAccountTypeIds, currentFinancialYearId);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "VOUCHER_TYPE_CREATE",
                actionName: request.VoucherTypeCode ?? string.Empty,
                details: $"Voucher Type '{request.VoucherTypeCode}' created successfully with Id {newId}.",
                module: "VoucherType"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Voucher Type created successfully.",
                Data = newId
            };
        }
    }
}
