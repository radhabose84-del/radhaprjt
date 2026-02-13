using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Purchase.DutyMaster.Command.Update;

namespace PurchaseManagement.Application.Purchase.DutyMaster.Validation
{
    public class UpdateDutyMasterValidator : AbstractValidator<UpdateDutyMasterCommand>
    {
        public UpdateDutyMasterValidator(IMiscMasterQueryRepository misc)
        {
            RuleFor(x => x.Model).NotNull();
            RuleFor(x => x.Model.Id).GreaterThan(0).WithMessage("Id must be provided.");            
            RuleFor(x => x.Model.TariffNumber).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Model.DutyCategoryId)
                .MustAsync(async (id, ct) => await misc.NotFoundAsync(id))
                .WithMessage("Invalid DutyCategoryId.");
            RuleFor(x => x.Model.EffectiveFrom).NotEmpty();
        }
    }
}
