using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.Common.Interfaces.IServiceMaster;
using PurchaseManagement.Application.ServiceMaster.Commands.CreateService;
using FluentValidation;
using PurchaseManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace PurchaseManagement.Presentation.Validation.ServiceMaster
{
    public class CreateServiceMasterCommandValidator : AbstractValidator<CreateServiceCommand>
    {
        private readonly List<ValidationRule> _validationRules;

        private readonly IServiceQueryRepository _serviceQueryRepository;

       // private readonly ISacQueryRepository sacs;          // if you have these repos
       //  private readonly IUomQueryRepository uoms;

        public CreateServiceMasterCommandValidator(IServiceQueryRepository serviceQueryRepository, MaxLengthProvider maxLengthProvider)
        {
            _serviceQueryRepository = serviceQueryRepository;

            // Field-level
            RuleFor(x => x.ServiceDescription)
                .NotEmpty().WithMessage("Service Description is required.")
                .MaximumLength(100)
                .Matches(@"^[A-Za-z0-9\s\-/()]+$")
                .WithMessage("Only letters, numbers, space, -, /, ( ) are allowed.");

            RuleFor(x => x.SacId)
                .GreaterThan(0).WithMessage("SAC is required.");

            RuleFor(x => x.UomId)
                .GreaterThan(0).WithMessage("UOM is required.");

            RuleFor(x => x.ServiceCategoryId)
                .Must(v => !v.HasValue || v.Value > 0)
                .WithMessage("Please select a Service Category.");  

            // Cross-field uniqueness: (SacId, UomId, ServiceDescription) among non-deleted rows
            RuleFor(x => x).CustomAsync(async (cmd, ctx, ct) =>
            {
                var exists = await _serviceQueryRepository.ExistsSimilarAsync(cmd.SacId, cmd.UomId, cmd.ServiceDescription.Trim(), null, ct);
                if (exists)
                    ctx.AddFailure("A similar service already exists (same SAC, UOM & Description).");
            });
        }
        

        

        
    }
}