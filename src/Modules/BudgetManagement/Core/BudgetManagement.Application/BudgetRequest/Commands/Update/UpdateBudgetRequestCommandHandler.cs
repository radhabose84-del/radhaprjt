using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using BudgetManagement.Application.BudgetRequest.Commands.Update;
using BudgetManagement.Application.Common.Interfaces;
using BudgetManagement.Application.Common.Interfaces.IBudgetRequest;
using BudgetManagement.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

public class UpdateBudgetRequestCommandHandler
    : IRequestHandler<UpdateBudgetRequestCommand>
{
    private readonly IBudgetRequestCommandRepository _budgetRepo;
    private readonly IBudgetRequestQueryRepository _budgetQueryRepo;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly ILogger<UpdateBudgetRequestCommandHandler> _logger;
    private readonly IIPAddressService _ip;
    private readonly IUnitLookup _unitLookup;
    private readonly ICompanyLookup _companyLookup;
    private readonly IFinancialYearLookup _financialYearLookup;

    public UpdateBudgetRequestCommandHandler(
        IBudgetRequestCommandRepository budgetRepo,
        IMapper mapper,
        IMediator mediator,
        IBudgetRequestQueryRepository budgetQueryRepo,
        ILogger<UpdateBudgetRequestCommandHandler> logger,
        IIPAddressService ip,
        IUnitLookup unitLookup,
        ICompanyLookup companyLookup,
        IFinancialYearLookup financialYearLookup)
    {
        _budgetRepo = budgetRepo;
        _mapper = mapper;
        _mediator = mediator;
        _budgetQueryRepo = budgetQueryRepo;
        _logger = logger;
        _ip = ip;
        _unitLookup = unitLookup;
        _companyLookup = companyLookup;
        _financialYearLookup = financialYearLookup;
    }

    public async Task Handle(UpdateBudgetRequestCommand request, CancellationToken cancellationToken)
    {
        var entity = await _budgetRepo.GetByIdAsync(request.Id, cancellationToken);
        if (entity == null) return;

        var originalAmount = entity.RequestAmount;
        var originalRevision = entity.RevisionNumber ?? 0;
        request.FinancialYearId = entity.FinancialYearId;

        _mapper.Map(request, entity);

        if (request.EditFlag == 1)
        {
            entity.RevisionNumber = originalRevision + 1;
            entity.RequestAmount = originalAmount + request.RequestAmount;
        }
        else
        {
            if (!entity.RevisionNumber.HasValue)
                entity.RevisionNumber = originalRevision;
        }

        // If no RequestMonthId, set FY period dates using FinancialYearId
        if (entity.RequestMonthId == null || entity.RequestMonthId == 0)
        {
            if (entity.FinancialYearId <= 0)
                throw new ApplicationException("FinancialYearId is missing in update.");

            var fy = await _financialYearLookup.GetByIdAsync(entity.FinancialYearId, cancellationToken);

            if (fy == null)
                throw new ApplicationException($"Financial year not found for Id={entity.FinancialYearId}");

            if (entity.RequestMonthId == null || entity.RequestMonthId == 0)
            {
                entity.FromDate = DateOnly.FromDateTime(fy.StartDate);
                entity.ToDate = DateOnly.FromDateTime(fy.EndDate);
            }
        }

        await _budgetRepo.UpdateAsync(entity, cancellationToken);

        var domainEvent = new AuditLogsDomainEvent(
            actionDetail: "Update",
            actionCode: entity.RequestCode ?? entity.Id.ToString(),
            actionName: "Budget Request",
            details: $"Budget Request '{entity.RequestCode}' was updated.",
            module: "BudgetRequest"
        );

        await _mediator.Publish(domainEvent, cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.ImagePath))
            await TryMoveImageAsync(entity.Id, request.ImagePath!, entity.RequestCode, cancellationToken);
    }

    private async Task TryMoveImageAsync(int requestId, string tempFileName, string baseCode, CancellationToken ct)
    {
        try
        {
            var companyId = _ip.GetCompanyId();
            var unitId = _ip.GetUnitId();

            var units = await _unitLookup.GetAllUnitAsync();
            var companies = await _companyLookup.GetAllCompanyAsync();

            var unitLookupDict = units.ToDictionary(u => u.UnitId, u => u.UnitName);
            var companyLookupDict = companies.ToDictionary(c => c.CompanyId, c => c.CompanyName);

            companyLookupDict.TryGetValue(companyId, out var companyName);
            unitLookupDict.TryGetValue(unitId, out var unitName);

            companyName ??= string.Empty;
            unitName ??= string.Empty;

            var baseDirectory = await _budgetQueryRepo.GetBaseDirectoryAsync(ct);
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDirectory, companyName, unitName);

            var tempFullPath = Path.Combine(uploadPath, tempFileName);
            EnsureDirectoryExists(Path.GetDirectoryName(tempFullPath)!);

            if (File.Exists(tempFullPath))
            {
                var newFile = $"{baseCode}{Path.GetExtension(tempFullPath)}";
                var newPath = Path.Combine(Path.GetDirectoryName(tempFullPath)!, newFile);

                File.Move(tempFullPath, newPath, overwrite: true);
                await _budgetRepo.UpdateImageAsync(requestId, newFile, ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Image move/rename failed for RequestId={requestId}", requestId);
        }
    }

        private static void EnsureDirectoryExists(string path)
        {
            if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
}
