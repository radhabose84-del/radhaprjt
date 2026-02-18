#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Presentation.Validation.Common;
using UserManagement.Application.FinancialYear.Command.CreateFinancialYear;
using FluentValidation;
using Shared.Validation.Common;



namespace UserManagement.Presentation.Validation.FinancialYear
{
    public class CreateFinancialYearCommandValidator  : AbstractValidator<CreateFinancialYearCommand>
    {
        private readonly List<ValidationRule> _validationRules;
       
    public CreateFinancialYearCommandValidator(MaxLengthProvider maxLengthProvider)
    {
        var startYearMaxLength = maxLengthProvider.GetMaxLength<UserManagement.Domain.Entities.FinancialYear>("StartYear") ?? 50;
        var finYearNameMaxLength = maxLengthProvider.GetMaxLength<UserManagement.Domain.Entities.FinancialYear>("FinYearName") ?? 50;

        _validationRules = ValidationRuleLoader.LoadValidationRules();
        if (_validationRules == null || !_validationRules.Any())
        {
            throw new InvalidOperationException("Validation rules could not be loaded.");
        }

        // Add validation rules dynamically
        AddDynamicRules(startYearMaxLength, finYearNameMaxLength);

        // Add Indian Financial Year validation
        AddFinancialYearDateValidation();
    }

            private void AddDynamicRules(int startYearMaxLength, int finYearNameMaxLength)
            {
                foreach (var rule in _validationRules)
                {
                    switch (rule.Rule)
                    {
                        case "NotEmpty":
                            RuleFor(x => x.StartYear)
                                .NotEmpty()
                                .WithMessage($"{nameof(CreateFinancialYearCommand.StartYear)} {rule.Error}");

                            RuleFor(x => x.FinYearName)
                                .NotEmpty()
                                .WithMessage($"{nameof(CreateFinancialYearCommand.FinYearName)} {rule.Error}");
                            break;

                        case "MaxLength":
                            RuleFor(x => x.StartYear.ToString())
                                .MaximumLength(startYearMaxLength)
                                .WithMessage($"{nameof(CreateFinancialYearCommand.StartYear)} {rule.Error} {startYearMaxLength}");

                            RuleFor(x => x.FinYearName)
                                .MaximumLength(finYearNameMaxLength)
                                .WithMessage($"{nameof(CreateFinancialYearCommand.FinYearName)} {rule.Error} {finYearNameMaxLength}");
                            break;
                    }
                }
            }

            private void AddFinancialYearDateValidation()
            {    
                    RuleFor(x => x.StartYear)
                        .NotEmpty()
                        .WithMessage("Start Year is required.")
                        .Must(startYear => 
                            int.TryParse(startYear, out var year) && 
                            year > 0 && 
                            startYear.Length == 4)  // Ensures 4-digit format
                        .WithMessage("Start Year must be a valid 4-digit year (e.g., 2023).");   

                    RuleFor(x => x.StartDate)
                        .NotEmpty()
                        .WithMessage("Start Date is required.")
                        .Must((command, startDate) => 
                        {
                            if (int.TryParse(command.StartYear, out var startYear))
                            {
                                return startDate == new DateTime(startYear, 4, 1);
                            }
                            return false;
                        })
                        .WithMessage(command => $"Start Date should be April 1st of the Start Year ({command.StartYear}).");

                    RuleFor(x => x.EndDate)
                        .NotEmpty()
                        .WithMessage("End Date is required.")
                        .Must((command, endDate) => 
                        {
                            // Check if StartDate is valid and parse StartYear
                            if (command.StartDate != default && int.TryParse(command.StartYear, out var startYear))
                            {
                                // Calculate the expected EndDate
                                var expectedEndDate = command.StartDate.AddYears(1).AddDays(-1); // One year minus 1 day
                                return endDate == expectedEndDate;
                            }
                            return false;
                        })
                        .WithMessage(command => 
                            $"End Date should be March 31st of the next year after Start Date ({command.StartDate.AddYears(1).AddDays(-1):yyyy-MM-dd}).");     
                
            }

    }
}