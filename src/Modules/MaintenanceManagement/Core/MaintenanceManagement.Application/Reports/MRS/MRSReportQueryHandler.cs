#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IReports;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.Reports.MRS
{
    public class MRSReportQueryHandler : IRequestHandler<MRSReportQuery, ApiResponseDTO<List<MRSReportDto>>>
    {
        private readonly IReportRepository _repository;
        private readonly IMapper _mapper;
        // private readonly IUnitService _unitService;
        private readonly IMediator _mediator;

        public MRSReportQueryHandler(IReportRepository repository, IMapper mapper, IMediator mediator)
        {
            _repository = repository;
            _mapper = mapper;
            // _unitService = unitService;
            _mediator = mediator;
        }


        public async Task<ApiResponseDTO<List<MRSReportDto>>> Handle(MRSReportQuery request, CancellationToken cancellationToken)
        {
            var fromDate = request.FromDate ?? throw new ArgumentNullException(nameof(request.FromDate));
            var toDate = request.ToDate ?? throw new ArgumentNullException(nameof(request.ToDate));

            // Fetch MRS report data from repository
            var mrsReports = await _repository.GetMRSReports(fromDate, toDate, request.OldUnitCode);

            // Map to DTOs
            var mrsReportDtos = _mapper.Map<List<MRSReportDto>>(mrsReports);

        
            // Log audit
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetMRSReport",
                actionCode: "Get",
                actionName: mrsReports.Count.ToString(),
                details: "MRS report list fetched.",
                module: "MRS"
            );

            await _mediator.Publish(domainEvent, cancellationToken);

            // Return API response
            return new ApiResponseDTO<List<MRSReportDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = mrsReportDtos,
                TotalCount = mrsReportDtos.Count
            };
        }
    }
}