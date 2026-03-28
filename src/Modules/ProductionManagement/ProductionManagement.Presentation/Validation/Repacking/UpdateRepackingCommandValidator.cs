using FluentValidation;
using ProductionManagement.Application.Common.Interfaces.IRepacking;
using ProductionManagement.Application.Repacking.Commands.UpdateRepacking;
using ProductionManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace ProductionManagement.Presentation.Validation.Repacking
{
    public class UpdateRepackingCommandValidator : AbstractValidator<UpdateRepackingCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IRepackingQueryRepository _queryRepo;

        public UpdateRepackingCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IRepackingQueryRepository queryRepo)
        {
            _queryRepo = queryRepo;

            var maxLengthRemarks = maxLengthProvider.GetMaxLength<Domain.Entities.RepackingHeader>("Remarks") ?? 500;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.RepackingDate)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateRepackingCommand.RepackingDate)} {rule.Error}");

                        RuleFor(x => x.OldPackHeaderId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateRepackingCommand.OldPackHeaderId)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.Remarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"{nameof(UpdateRepackingCommand.Remarks)} {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepo.NotFoundAsync(id))
                            .WithMessage($"Repacking {rule.Error}");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.OldPackHeaderId)
                            .MustAsync(async (id, ct) => await _queryRepo.OldPackHeaderExistsAsync(id))
                            .WithMessage($"{nameof(UpdateRepackingCommand.OldPackHeaderId)} {rule.Error}")
                            .When(x => x.OldPackHeaderId > 0);

                        RuleForEach(x => x.RepackingDetails)
                            .ChildRules(detail =>
                            {
                                detail.RuleFor(d => d.OldPackDetailId)
                                    .MustAsync(async (id, ct) => await _queryRepo.PackDetailExistsAsync(id))
                                    .WithMessage($"OldPackDetailId {rule.Error}")
                                    .When(d => d.OldPackDetailId > 0);

                                detail.RuleFor(d => d.LotId)
                                    .MustAsync(async (id, ct) => await _queryRepo.LotExistsAsync(id))
                                    .WithMessage($"LotId {rule.Error}")
                                    .When(d => d.LotId > 0);

                                detail.RuleFor(d => d.PackTypeId)
                                    .MustAsync(async (id, ct) => await _queryRepo.PackTypeExistsAsync(id))
                                    .WithMessage($"PackTypeId {rule.Error}")
                                    .When(d => d.PackTypeId > 0);
                            })
                            .When(x => x.RepackingDetails != null);
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateRepackingCommand.IsActive)} {rule.Error}");
                        break;

                    case "GreaterThan":
                        RuleFor(x => x.TotalBags)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateRepackingCommand.TotalBags)} {rule.Error}");

                        RuleFor(x => x.NetWeight)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateRepackingCommand.NetWeight)} {rule.Error}");

                        RuleForEach(x => x.RepackingDetails)
                            .ChildRules(detail =>
                            {
                                detail.RuleFor(d => d.StartPackNo)
                                    .GreaterThan(0)
                                    .WithMessage($"StartPackNo {rule.Error}");

                                detail.RuleFor(d => d.EndPackNo)
                                    .GreaterThan(0)
                                    .WithMessage($"EndPackNo {rule.Error}")
                                    .GreaterThanOrEqualTo(d => d.StartPackNo)
                                    .WithMessage("EndPackNo must be greater than or equal to StartPackNo.");

                                detail.RuleFor(d => d.NetWeightPerPack)
                                    .GreaterThan(0)
                                    .WithMessage($"NetWeightPerPack {rule.Error}");
                            })
                            .When(x => x.RepackingDetails != null);
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleFor(x => x.LooseConeKgs)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateRepackingCommand.LooseConeKgs)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
