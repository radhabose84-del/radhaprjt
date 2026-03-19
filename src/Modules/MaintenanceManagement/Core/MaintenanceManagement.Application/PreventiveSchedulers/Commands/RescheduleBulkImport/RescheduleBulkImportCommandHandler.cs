using System.Globalization;
using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IBackgroundService;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Commands.RescheduleBulkImport
{
    public class RescheduleBulkImportCommandHandler : IRequestHandler<RescheduleBulkImportCommand, ApiResponseDTO<string>>
    {
        private readonly IPreventiveSchedulerCommand _preventiveSchedulerCommand;
        private readonly IBackgroundServiceClient _backgroundServiceClient;
        private readonly IPreventiveSchedulerQuery _preventiveSchedulerQuery;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RescheduleBulkImportCommandHandler(IPreventiveSchedulerCommand preventiveSchedulerCommand,
            IBackgroundServiceClient backgroundServiceClient, IPreventiveSchedulerQuery preventiveSchedulerQuery,
            IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _preventiveSchedulerCommand = preventiveSchedulerCommand;
            _backgroundServiceClient = backgroundServiceClient;
            _preventiveSchedulerQuery = preventiveSchedulerQuery;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ApiResponseDTO<string>> Handle(RescheduleBulkImportCommand request, CancellationToken cancellationToken)
        {
            using var stream = new MemoryStream();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            await request.File.CopyToAsync(stream, cancellationToken);
            stream.Position = 0;

            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];
            var detailSheet = package.Workbook.Worksheets[1];
            var activitySheet = package.Workbook.Worksheets[2];

            int rowCount = worksheet.Dimension.Rows;
            int detailRowCount = detailSheet.Dimension.Rows;
            int activityRowCount = activitySheet.Dimension.Rows;

            List<PreventiveSchedulerHeaderBulkImportDto> headerDtos = new();

            for (int row = 2; row <= rowCount; row++)
            {
                string schedulerName = worksheet.Cells[row, 1].Text.Trim();
                var MachineGroup = worksheet.Cells[row, 2].Text.Trim();

                var machinegroupName = await _preventiveSchedulerQuery.GetMachineGroupIdByName(MachineGroup);
                var headerDto = new PreventiveSchedulerHeaderBulkImportDto
                {
                    PreventiveSchedulerName = schedulerName,
                    MachineGroupId = machinegroupName.Id,
                    DepartmentId = int.Parse(worksheet.Cells[row, 3].Text.Trim()),
                    MaintenanceCategoryId = int.Parse(worksheet.Cells[row, 4].Text.Trim()),
                    ScheduleId = int.Parse(worksheet.Cells[row, 5].Text.Trim()),
                    FrequencyTypeId = int.Parse(worksheet.Cells[row, 6].Text.Trim()),
                    FrequencyInterval = int.Parse(worksheet.Cells[row, 7].Text.Trim()),
                    FrequencyUnitId = int.Parse(worksheet.Cells[row, 8].Text.Trim()),
                    EffectiveDate = DateOnly.ParseExact(worksheet.Cells[row, 9].Text.Trim(), "dd-MM-yyyy", CultureInfo.InvariantCulture),
                    GraceDays = int.Parse(worksheet.Cells[row, 10].Text.Trim()),
                    ReminderWorkOrderDays = int.Parse(worksheet.Cells[row, 11].Text.Trim()),
                    ReminderMaterialReqDays = int.Parse(worksheet.Cells[row, 12].Text.Trim()),
                    IsDownTimeRequired = byte.Parse(worksheet.Cells[row, 13].Text.Trim()),
                    DownTimeEstimateHrs = int.Parse(worksheet.Cells[row, 14].Text.Trim()),
                    CompanyId = int.Parse(worksheet.Cells[row, 15].Text.Trim()),
                    UnitId = int.Parse(worksheet.Cells[row, 16].Text.Trim()),
                    PreventDetails = new List<PrevetiveSchedulerDetailBulkImportDto>(),
                    PreventActivities = new List<PreventiveSchedulerBulkImprotActivityDto>()
                };

                for (int dRow = 2; dRow <= detailRowCount; dRow++)
                {
                    string detailSchedulerName = detailSheet.Cells[dRow, 1].Text.Trim();
                    if (schedulerName.Equals(detailSchedulerName, StringComparison.OrdinalIgnoreCase))
                    {
                        string machinecode = detailSheet.Cells[dRow, 2].Text.Trim();
                        var machineinfo = await _preventiveSchedulerQuery.GetMachineIdByCode(machinecode);

                        var detailDto = new PrevetiveSchedulerDetailBulkImportDto
                        {
                            MachineId = machineinfo.Id,
                            WorkOrderCreationStartDate = DateOnly.ParseExact(detailSheet.Cells[dRow, 3].Text.Trim(), "dd-MM-yyyy", CultureInfo.InvariantCulture),
                            ActualWorkOrderDate = string.IsNullOrWhiteSpace(detailSheet.Cells[dRow, 4].Text)
                                ? null
                                : DateOnly.ParseExact(detailSheet.Cells[dRow, 4].Text.Trim(), "dd-MM-yyyy", CultureInfo.InvariantCulture),
                            MaterialReqStartDays = DateOnly.ParseExact(detailSheet.Cells[dRow, 5].Text.Trim(), "dd-MM-yyyy", CultureInfo.InvariantCulture),
                            LastMaintenanceActivityDate = string.IsNullOrWhiteSpace(detailSheet.Cells[dRow, 6].Text)
                                ? null
                                : DateOnly.ParseExact(detailSheet.Cells[dRow, 6].Text.Trim(), "dd-MM-yyyy", CultureInfo.InvariantCulture)
                        };

                        headerDto.PreventDetails.Add(detailDto);
                    }
                }

                for (int ARow = 2; ARow <= activityRowCount; ARow++)
                {
                    string detailSchedulerName = activitySheet.Cells[ARow, 1].Text.Trim();
                    if (schedulerName.Equals(detailSchedulerName, StringComparison.OrdinalIgnoreCase))
                    {
                        string activity = activitySheet.Cells[ARow, 2].Text.Trim();
                        var Activityinfo = await _preventiveSchedulerQuery.GetActivityIdByName(activity);

                        var activityDtoDto = new PreventiveSchedulerBulkImprotActivityDto
                        {
                            ActivityId = Activityinfo.Id
                        };

                        headerDto.PreventActivities.Add(activityDtoDto);
                    }
                }

                headerDtos.Add(headerDto);
            }

            var preventiveScheduler = _mapper.Map<List<PreventiveSchedulerHeader>>(headerDtos);
            var response = await _preventiveSchedulerCommand.BulkImportPreventiveHeaderAsync(preventiveScheduler);

            foreach (var detail in response)
            {
                var getMachineWiseDetail = await _preventiveSchedulerQuery.GetPreventiveSchedulerDetail(detail.Id);

                int jobDelayMin = 0;
                foreach (var machinedetail in getMachineWiseDetail)
                {
                    var startDateTime = machinedetail.WorkOrderCreationStartDate.ToDateTime(TimeOnly.MinValue);
                    var delay = startDateTime - DateTime.Now;
                    string newJobId;
                    var delayInMinutes = (int)delay.TotalMinutes;
                    var token = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"].ToString();
                    if (delay.TotalSeconds > 0)
                    {
                        newJobId = await _backgroundServiceClient.ScheduleWorkOrder(machinedetail.Id, delayInMinutes, token);
                    }
                    else
                    {
                        jobDelayMin += 2;
                        newJobId = await _backgroundServiceClient.ScheduleWorkOrder(machinedetail.Id, jobDelayMin, token);
                    }
                    machinedetail.HangfireJobId = newJobId;
                    await _preventiveSchedulerCommand.UpdateDetailAsync(machinedetail.Id, newJobId);
                }
            }

            return new ApiResponseDTO<string>
            {
                IsSuccess = true,
                Message = "Update successful."
            };
        }
    }
}
