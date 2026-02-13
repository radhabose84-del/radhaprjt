using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.RescheduleBulkImport;
using FluentValidation;
using MaintenanceManagement.Presentation.Validation.Common;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using Shared.Validation.Common;

namespace MaintenanceManagement.Presentation.Validation.PreventiveSchedulers
{
    public class BulkImportPreventiveSchedulerCommandValidator : AbstractValidator<RescheduleBulkImportCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IPreventiveSchedulerQuery _preventiveSchedulerQuery;
        private string? _validationErrorMessage;
        public BulkImportPreventiveSchedulerCommandValidator(IPreventiveSchedulerQuery preventiveSchedulerQuery)
        {
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            _preventiveSchedulerQuery = preventiveSchedulerQuery;
            if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {

                    case "NotFound":
                        RuleFor(x => x.File)
                               .NotNull()
                               .WithMessage("Excel file is required.")
                                .MustAsync(async (file, cancellation) =>
                                {
                                    var result = await ValidateExcelSheets(file, cancellation);
                                    _validationErrorMessage = result.ErrorMessage;
                                    return result.IsValid;
                                }).WithMessage(x => _validationErrorMessage ?? "Validation failed.");
                        break;
                    default:
                        break;
                }
            }
        }
         

           private async Task<(bool IsValid, string? ErrorMessage)> ValidateExcelSheets(IFormFile file, CancellationToken cancellationToken)
           {
               try
               {
                   using var stream = new MemoryStream();
                   await file.CopyToAsync(stream, cancellationToken);
                   stream.Position = 0;
        
                   ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                   using var package = new ExcelPackage(stream);
        
                   var headerSheet = package.Workbook.Worksheets[0]; // Sheet[1]
                   var detailSheet = package.Workbook.Worksheets[1]; // Sheet[2]
                   var activitySheet = package.Workbook.Worksheets[2]; // Sheet[3]
                   
                   var errorMessages = new List<string>();
        
                   // ✅ Validate MachineGroup in Sheet[1]
                int headerRows = headerSheet.Dimension.Rows;
                   var schedulerNames = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);
                for (int row = 2; row <= headerRows; row++)
                {
                    var machineGroupName = headerSheet.Cells[row, 2].Text.Trim(); // Column 2
                    var schedulerName = headerSheet.Cells[row, 1].Text.Trim();
                    var machineGroup = await _preventiveSchedulerQuery.GetMachineGroupIdByName(machineGroupName);
                    if (machineGroup == null)
                    {
                       errorMessages.Add($"Sheet: {headerSheet.Name}, Row: {row}, Error: Machine Group '{machineGroupName}' not found.");
                    }
                     if (!string.IsNullOrWhiteSpace(schedulerName))
                     {
                         if (!schedulerNames.ContainsKey(schedulerName))
                             schedulerNames[schedulerName] = new List<int>();
                         schedulerNames[schedulerName].Add(row);
                     }
                }
                 var duplicateNames = schedulerNames.Where(x => x.Value.Count > 1).ToList();
            foreach (var dup in duplicateNames)
            {
               errorMessages.Add($"Sheet: {headerSheet.Name}, Rows: {string.Join(", ", dup.Value)}, Duplicate Scheduler Name: '{dup.Key}'");
            }
        
                   // ✅ Validate MachineCode in Sheet[2]
                int detailRows = detailSheet.Dimension.Rows;
                   for (int row = 2; row <= detailRows; row++)
                   {
                       var machineCode = detailSheet.Cells[row, 2].Text.Trim(); // Column 2
                       var machine = await _preventiveSchedulerQuery.GetMachineIdByCode(machineCode);
                       if (machine == null)
                       {
                           errorMessages.Add($"Sheet: {detailSheet.Name}, Row: {row}, Error: Machine Code '{machineCode}' not found.");
                           
                       }
                   }
        
                   // ✅ Validate Activity in Sheet[3]
                   int activityRows = activitySheet.Dimension.Rows;
                   for (int row = 2; row <= activityRows; row++)
                   {
                       var activityName = activitySheet.Cells[row, 2].Text.Trim(); // Column 1
                       var activity = await _preventiveSchedulerQuery.GetActivityIdByName(activityName);
                       if (activity == null)
                       {
                          errorMessages.Add($"Sheet: {activitySheet.Name}, Row: {row}, Error: Activity '{activity}' not found.");
                       }
                   }
        
                   return errorMessages.Count > 0
                ? (false, string.Join(" | ", errorMessages))
                : (true, null);
               }
               catch (Exception ex)
               {
                   return (false, $"Excel processing error: {ex.Message}");
               }
           }
    }
}