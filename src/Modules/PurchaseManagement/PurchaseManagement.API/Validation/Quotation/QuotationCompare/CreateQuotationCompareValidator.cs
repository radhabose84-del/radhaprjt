using System;
using System.Collections.Generic;
using System.Linq;
using Contracts.Interfaces.External.IWorkflow;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IQuotationCompare;
using PurchaseManagement.Application.Quotation.QuotationCompare.Commands.CreateQuoteComparsion;
using PurchaseManagement.Domain.Common;
using FluentValidation;
using PurchaseManagement.API.Validation.Common;
using Shared.Validation.Common;

namespace PurchaseManagement.API.Validation.Quotation.QuotationCompare
{
    public class CreateQuotationCompareValidator : AbstractValidator<CreateQuoteComparsionCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IWorkflowGrpcClient _workflowGrpcClient;
        private readonly IQuotationCompareCommandRepository _iquotationCompareCommandRepository;
        private readonly IIPAddressService _ipAddressService;

        public CreateQuotationCompareValidator(
            IQuotationCompareCommandRepository iquotationCompareCommandRepository,
            MaxLengthProvider maxLengthProvider, IWorkflowGrpcClient workflowGrpcClient, IIPAddressService iPAddressService)
        {
            _iquotationCompareCommandRepository = iquotationCompareCommandRepository;
            _workflowGrpcClient = workflowGrpcClient;
            _ipAddressService = iPAddressService;

            _validationRules = ValidationRuleLoader.LoadValidationRules();

            if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }
            var UnitId = _ipAddressService.GetUnitId();
            RuleFor(x => new { UnitId })
                            .MustAsync(async (indent, cancellation) =>
                          await _workflowGrpcClient.IsApproveWorkflowConfigure(MiscEnumEntity.QuotationComparison, indent.UnitId, 0))
                            .WithMessage("Approval Workflow is not configured.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {

                    case "NotEmpty":
                        RuleFor(x => x.CreateQuoteComparsion.RfqId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateQuoteComparsionCommand.CreateQuoteComparsion.RfqId)} {rule.Error}");

                        RuleFor(x => x.CreateQuoteComparsion.RfqCode)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateQuoteComparsionCommand.CreateQuoteComparsion.RfqCode)} {rule.Error}");

                        // Validate each detail item
                        RuleForEach(x => x.CreateQuoteComparsion.Details).ChildRules(details =>
                        {
                            details.RuleFor(d => d.QuotationHeaderId)
                                .NotEmpty()
                                .WithMessage($"{nameof(CreateQuoteComparsionDto.CreateQuoteComparsionDetailDto.QuotationHeaderId)} {rule.Error}");

                            details.RuleFor(d => d.QuotationDetailId)
                                .NotEmpty()
                                .WithMessage($"{nameof(CreateQuoteComparsionDto.CreateQuoteComparsionDetailDto.QuotationDetailId)} {rule.Error}");

                            details.RuleFor(d => d.Net)
                                .NotEmpty()
                                .WithMessage($"{nameof(CreateQuoteComparsionDto.CreateQuoteComparsionDetailDto.Net)} {rule.Error}");

                            details.RuleFor(d => d.LandedUnit)
                                .NotEmpty()
                                .WithMessage($"{nameof(CreateQuoteComparsionDto.CreateQuoteComparsionDetailDto.LandedUnit)} {rule.Error}");

                            details.RuleFor(d => d.Total)
                                .NotEmpty()
                                .WithMessage($"{nameof(CreateQuoteComparsionDto.CreateQuoteComparsionDetailDto.Total)} {rule.Error}");
                        });
                        break;

                    case "MinLength":
                        RuleFor(x => x.CreateQuoteComparsion.RfqId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(CreateQuoteComparsionCommand.CreateQuoteComparsion.RfqId)} {rule.Error} {0}");

                        RuleForEach(x => x.CreateQuoteComparsion.Details).ChildRules(details =>
                        {
                            details.RuleFor(d => d.QuotationHeaderId)
                                .GreaterThanOrEqualTo(1)
                                .WithMessage($"{nameof(CreateQuoteComparsionDto.CreateQuoteComparsionDetailDto.QuotationHeaderId)} {rule.Error} {0}");

                             details.RuleFor(d => d.QuotationDetailId)
                                .GreaterThanOrEqualTo(1)
                                .WithMessage($"{nameof(CreateQuoteComparsionDto.CreateQuoteComparsionDetailDto.QuotationDetailId)} {rule.Error} {0}");
                        });
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.CreateQuoteComparsion)
                            .MustAsync(async (quotation, cancellation) =>
                                !await _iquotationCompareCommandRepository.ExistsAsync(
                                    quotation.RfqId,
                                    quotation.RfqCode ?? string.Empty))
                            .WithMessage("Quotation Comparison already exists for the same RFQ Id and RFQ Code.");
                        break;

                    default:
                        break;

                }
            }
        }
    }
}
