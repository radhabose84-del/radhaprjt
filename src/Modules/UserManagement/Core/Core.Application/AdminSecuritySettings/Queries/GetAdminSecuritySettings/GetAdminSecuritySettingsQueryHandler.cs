using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Core.Application.AdminSecuritySettings.Queries.GetAdminSecuritySettings;
using Core.Application.Common.Interfaces.IAdminSecuritySettings;
using Core.Application.Common;
using Core.Domain.Events;
using Microsoft.Extensions.Logging;
using Core.Application.Common.HttpResponse;

namespace Core.Application.AdminSecuritySettings.Queries.GetAdminSecuritySettings
{
    public class GetAdminSecuritySettingsQueryHandler :IRequestHandler<GetAdminSecuritySettingsQuery,ApiResponseDTO<List<GetAdminSecuritySettingsDto>>>
    {
           private readonly IAdminSecuritySettingsQueryRepository   _adminSecuritySettingsQueryRepository;
        private readonly IMapper _mapper; 
           private readonly IMediator _mediator; 

        private readonly ILogger<GetAdminSecuritySettingsQueryHandler> _logger;


         public GetAdminSecuritySettingsQueryHandler(IAdminSecuritySettingsQueryRepository adminSecuritySettingsQueryRepository,IMapper mapper, IMediator mediator,ILogger<GetAdminSecuritySettingsQueryHandler> logger)
        {
            _mapper =mapper;
            _adminSecuritySettingsQueryRepository = adminSecuritySettingsQueryRepository;         
             _mediator = mediator;  

             _logger = logger;


        }
        public async Task<ApiResponseDTO<List<GetAdminSecuritySettingsDto>>> Handle(GetAdminSecuritySettingsQuery request, CancellationToken cancellationToken)
            {
                _logger.LogInformation("Fetching Admin Security Settings Request started: {request}", request);

                // Fetch paginated admin security settings
                var (adminSecuritySettings, totalCount) = await _adminSecuritySettingsQueryRepository
                    .GetAllAdminSecuritySettingsAsync(request.PageNumber, request.PageSize, request.SearchTerm);

                if (adminSecuritySettings == null || !adminSecuritySettings.Any())
                {
                    _logger.LogWarning("No Admin Security Settings records found in the database. Total count: {Count}", adminSecuritySettings?.Count ?? 0);

                    return new ApiResponseDTO<List<GetAdminSecuritySettingsDto>>
                    {
                        IsSuccess = false,
                        Message = "No Record Found"
                    };
                }

                _logger.LogInformation("Admin Security Settings fetched successfully. Mapping to DTO.");

                // Map the result to DTO
                var adminSecuritySettingsList = _mapper.Map<List<GetAdminSecuritySettingsDto>>(adminSecuritySettings);

                // Publish domain event
                var domainEvent = new AuditLogsDomainEvent(
                    actionDetail: "GetAll",
                    actionCode: "",
                    actionName: "",
                    details: "Admin Security Settings details were fetched.",
                    module: "AdminSecuritySettings"
                );

                await _mediator.Publish(domainEvent, cancellationToken);

                _logger.LogInformation("Admin Security Settings {Count} listed successfully.", adminSecuritySettingsList.Count);

                return new ApiResponseDTO<List<GetAdminSecuritySettingsDto>>
                {
                    IsSuccess = true,
                    Message = "Success",
                    Data = adminSecuritySettingsList,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
            }

     

        



    }
}