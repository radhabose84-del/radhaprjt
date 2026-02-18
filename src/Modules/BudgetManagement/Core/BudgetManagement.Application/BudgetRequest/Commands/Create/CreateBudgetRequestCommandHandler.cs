#nullable disable
using System.Text.Json;
using AutoMapper;
using Contracts.Events.Workflow;
using Contracts.Interfaces.Lookups.Users;
using BudgetManagement.Application.Common.Interfaces;
using BudgetManagement.Application.Common.Interfaces.IBudgetRequest;
using BudgetManagement.Application.Common.Interfaces.IMiscMaster;
using BudgetManagement.Domain.Common;
using BudgetManagement.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BudgetManagement.Application.BudgetRequest.Commands.Create;

public class CreateBudgetRequestCommandHandler
    : IRequestHandler<CreateBudgetRequestCommand, int>
{
    private readonly IBudgetRequestCommandRepository _budgetRepo;
    private readonly IBudgetRequestQueryRepository _budgetQueryRepo;
    private readonly IMiscMasterQueryRepository _miscRepo;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly ILogger<CreateBudgetRequestCommandHandler> _logger;
    private readonly IUnitLookup _unitLookup;
    private readonly ICompanyLookup _companyLookup;
    private readonly IIPAddressService _ip;
    private readonly IEventPublisher _eventPublisher;
    private readonly IFinancialYearLookup _financialYearLookup;

    public CreateBudgetRequestCommandHandler(
        IBudgetRequestCommandRepository budgetRepo,
        IMiscMasterQueryRepository miscRepo,
        IMapper mapper,
        IMediator mediator,
        IBudgetRequestQueryRepository budgetQueryRepo,
        ILogger<CreateBudgetRequestCommandHandler> logger,
        IUnitLookup unitLookup,
        ICompanyLookup companyLookup,
        IIPAddressService ip,
        IEventPublisher eventPublisher,
        IFinancialYearLookup financialYearLookup)
    {
        _budgetRepo = budgetRepo;
        _miscRepo = miscRepo;
        _mapper = mapper;
        _mediator = mediator;
        _budgetQueryRepo = budgetQueryRepo;
        _logger = logger;
        _unitLookup = unitLookup;
        _companyLookup = companyLookup;
        _ip = ip;
        _eventPublisher = eventPublisher;
        _financialYearLookup = financialYearLookup;
    }

    public async Task<int> Handle(CreateBudgetRequestCommand request, CancellationToken cancellationToken)
    {
        // map
        var entity = _mapper.Map<BudgetManagement.Domain.Entities.BudgetRequest>(request);
        var requestDate = request.FromDate ?? DateOnly.FromDateTime(DateTime.Today);

        if (entity.FinancialYearId <= 0)
        {
            var financialYears = await _financialYearLookup.GetAllFinancialYearAsync();

            // convert DateTime → DateOnly for comparison
            var fyMatch = financialYears
                .FirstOrDefault(fy =>
                {
                    var start = DateOnly.FromDateTime(fy.StartDate.Date);
                    var end = DateOnly.FromDateTime(fy.EndDate.Date);
                    return start <= requestDate && requestDate <= end;
                });

            if (fyMatch != null)
            {
                entity.FinancialYearId = fyMatch.FinancialYearId;
                if (entity.RequestMonthId == null || entity.RequestMonthId == 0)
                {
                    entity.FromDate = DateOnly.FromDateTime(fyMatch.StartDate);
                    entity.ToDate = DateOnly.FromDateTime(fyMatch.EndDate);
                }
            }
            else
            {
                var msg = $"Financial year does not exist for date {requestDate:yyyy-MM-dd}.";
                _logger.LogWarning(msg);

                throw new ApplicationException(msg);
            }
        }

        entity.RequestCode = await _budgetRepo.GenerateCodeAsync(
            unitId: request.UnitId,            
            requestDate: requestDate,
            ct: cancellationToken);

        // save
        var created = await _budgetRepo.AddAsync(entity, cancellationToken);
        var entity1 = await _budgetRepo.GetByIdBudgetRequestWorkFlowAsync(created.Id);
        var reverseMap = _mapper.Map<CreateBudgetRequestReverseDto>(entity1);
        string serializedPayload = JsonSerializer.Serialize(reverseMap);

        // ---- Audit domain event ----
        var domainEvent = new AuditLogsDomainEvent(
            actionDetail: "Create",
            actionCode: created.RequestCode ?? created.Id.ToString(),
            actionName: "Budget Request",
            details: $"Budget Request '{created.RequestCode}' was created.",
            module: "BudgetRequest"
        );

        await _mediator.Publish(domainEvent, cancellationToken);        
       
        
        if (created.Id > 0)
            {
                 if (!string.IsNullOrWhiteSpace(request.ImagePath))
                    await TryMoveImageAsync(created.Id, request.ImagePath!, created.RequestCode, cancellationToken);

                var correlationId = Guid.NewGuid();
                var @event = new TransactionCreatedEvent
                {
                    CorrelationId = correlationId,
                    ModuleTypeName = MiscEnumEntity.BudgetRequest,
                    ModuleTransactionId = created.Id,
                    Payload = serializedPayload
                };

                await _eventPublisher.SaveEventAsync(@event);
                await _eventPublisher.PublishPendingEventsAsync();
            }
        return created.Id;
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
