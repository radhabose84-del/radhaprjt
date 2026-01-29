using AutoMapper;
using Core.Application.Common.HttpResponse;
using Core.Application.Common.Interfaces;
using Core.Application.Common.Interfaces.IModule;
using Core.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core.Application.Modules.Queries.GetModules
{
    public class GetModulesQueryHandler : IRequestHandler<GetModulesQuery, ApiResponseDTO<List<ModuleDto>>>
    {
        private readonly IModuleQueryRepository _moduleRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator; 
        private readonly ILogger<GetModulesQueryHandler> _logger;

    public GetModulesQueryHandler(IModuleQueryRepository moduleRepository, IMapper mapper, IMediator mediator,ILogger<GetModulesQueryHandler> logger)
    {
        _moduleRepository = moduleRepository;
        _mapper = mapper;
        _mediator = mediator;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    }

    public async Task<ApiResponseDTO<List<ModuleDto>>> Handle(GetModulesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching all modules from the repository.");
        // Fetch all modules from the repository
            var (modules, totalCount) = await _moduleRepository.GetAllModulesAsync(request.PageNumber, request.PageSize, request.SearchTerm);
            // if (modules == null || modules.Count == 0)
            // {
            //     _logger.LogWarning("No modules found in the repository.");
            //     return new List<ModuleDto>();
            // }

            var moduleList= _mapper.Map<List<ModuleDto>>(modules);
            _logger.LogInformation("Fetched {ModuleCount} modules from the repository.", modules.Count);

        //Publish Domain Event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "Fetch",
                    actionCode: "GetAllModules",        
                    actionName: "Get Modules",
                    details: $"Fetched details of {modules.Count} modules.",
                    module:"Module"
                );
                await _mediator.Publish(domainEvent, cancellationToken);
                 return new ApiResponseDTO<List<ModuleDto>> 
            { 
                IsSuccess = true, 
                Message = "Success", 
                Data = moduleList ,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
                };

    }
    }
}