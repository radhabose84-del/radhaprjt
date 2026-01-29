using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces.IReports;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.Reports.GeneratorConsumption
{
    public class GeneratorConsumptionReportQueryHandler : IRequestHandler<GeneratorConsumptionReportQuery, ApiResponseDTO<List<GeneratorReportDto>>>
    {
        private readonly IReportRepository _repository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GeneratorConsumptionReportQueryHandler(IReportRepository repository, IMapper mapper, IMediator mediator)
        {
            _repository = repository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<GeneratorReportDto>>> Handle(GeneratorConsumptionReportQuery request, CancellationToken cancellationToken)
        {
            var fromDate = request.FromDate ?? throw new ArgumentNullException(nameof(request.FromDate));
            var toDate = request.ToDate ?? throw new ArgumentNullException(nameof(request.ToDate));

            // Fetch AssetTransfer report data from repository
            var powerconsumptionReports = await _repository.GetGeneratorReports(fromDate, toDate);

            // Map to DTOs
            var powerconsumptionReportDtos = _mapper.Map<List<GeneratorReportDto>>(powerconsumptionReports);
        

            // Log audit
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GeneratorConsumptionReportQuery",
                actionCode: "Get",
                actionName: powerconsumptionReportDtos.Count.ToString(),
                details: "GeneratorConsumption report list fetched.",
                module: "GeneratorConsumption"
            );

            await _mediator.Publish(domainEvent, cancellationToken);

            // Return API response
            return new ApiResponseDTO<List<GeneratorReportDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = powerconsumptionReportDtos ?? new List<GeneratorReportDto>(),
                TotalCount = powerconsumptionReportDtos != null ? powerconsumptionReportDtos.Count : 0
            };
        }
    }
}