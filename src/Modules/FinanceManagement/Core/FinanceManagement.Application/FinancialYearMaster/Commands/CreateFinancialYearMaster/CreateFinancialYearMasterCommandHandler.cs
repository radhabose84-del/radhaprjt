using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IFinancialYearMaster;
using FinanceManagement.Domain.Events;
using MediatR;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Application.FinancialYearMaster.Commands.CreateFinancialYearMaster
{
    public class CreateFinancialYearMasterCommandHandler : IRequestHandler<CreateFinancialYearMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IFinancialYearMasterCommandRepository _commandRepository;
        private readonly IFinancialYearMasterQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateFinancialYearMasterCommandHandler(
            IFinancialYearMasterCommandRepository commandRepository,
            IFinancialYearMasterQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateFinancialYearMasterCommand request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            // Resolve the 'OPEN' StatusId for FYS (year) and FPS (period) from MiscMaster
            var fysOpenId = await _queryRepository.GetMiscMasterIdByCodeAsync("FYS", "OPEN");
            var fpsOpenId = await _queryRepository.GetMiscMasterIdByCodeAsync("FPS", "OPEN");

            if (fysOpenId <= 0 || fpsOpenId <= 0)
                throw new ExceptionRules("Status master rows (FYS / FPS) are not seeded in MiscMaster. Seed them before creating a Financial Year.");

            // Map header
            var entity = _mapper.Map<Domain.Entities.FinancialYearMaster>(request);
            entity.CompanyId = companyId;
            entity.StatusId = fysOpenId;

            // Generate 13 periods (12 monthly + Period 13 adjustment with same span as Period 12)
            var periods = GeneratePeriods(request, companyId, fpsOpenId);

            var newId = await _commandRepository.CreateAsync(entity, periods, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "FINANCIAL_YEAR_MASTER_CREATE",
                actionName: request.FinancialYearCode ?? string.Empty,
                details: $"Financial Year '{request.FinancialYearCode}' created with Id {newId} for Company {companyId}; 13 periods generated.",
                module: "FinancialYearMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Financial Year created successfully.",
                Data = newId
            };
        }

        private static List<Domain.Entities.FinancialPeriodMaster> GeneratePeriods(
            CreateFinancialYearMasterCommand request, int companyId, int fpsOpenId)
        {
            var periods = new List<Domain.Entities.FinancialPeriodMaster>();

            // 12 monthly periods
            for (byte p = 1; p <= 12; p++)
            {
                var pStart = request.StartDate.AddMonths(p - 1);
                var pEnd = pStart.AddMonths(1).AddDays(-1);

                periods.Add(new Domain.Entities.FinancialPeriodMaster
                {
                    CompanyId = companyId,
                    PeriodNumber = p,
                    PeriodName = pStart.ToString("MMM-yyyy"),
                    StartDate = pStart,
                    EndDate = pEnd,
                    StatusId = fpsOpenId,
                    IsAdjustmentPeriod = false,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });
            }

            // Period 13 — same span as Period 12 (Indian accounting convention for year-end adjustments)
            var p12 = periods[^1];
            periods.Add(new Domain.Entities.FinancialPeriodMaster
            {
                CompanyId = companyId,
                PeriodNumber = 13,
                PeriodName = $"Adj-{request.FinancialYearCode}",
                StartDate = p12.StartDate,
                EndDate = p12.EndDate,
                StatusId = fpsOpenId,
                IsAdjustmentPeriod = true,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });

            return periods;
        }
    }
}
