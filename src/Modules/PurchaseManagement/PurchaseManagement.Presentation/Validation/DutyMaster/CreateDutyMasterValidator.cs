using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.DutyMaster.Command.Create;
using PurchaseManagement.Application.Common.Interfaces.IPurchase.DutyMaster;

namespace PurchaseManagement.Application.Purchase.DutyMaster.Validation
{
    public class CreateDutyMasterValidator : AbstractValidator<CreateDutyMasterCommand>
    {
        public CreateDutyMasterValidator(IMiscMasterQueryRepository misc, IDutyMasterQueryRepository read)
        {            
            RuleFor(x => x.Model.TariffNumber).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Model.DutyCategoryId)
                .MustAsync(async (id, ct) => await misc.NotFoundAsync(id))
                .WithMessage("Invalid DutyCategoryId.");

            RuleFor(x => x.Model.BasicCustomsDutyPercentage).InclusiveBetween(0, 99.99m);
            RuleFor(x => x.Model.SocialWelfareSurchargePercentage).InclusiveBetween(0, 99.99m);
            RuleFor(x => x.Model.IGSTPercentage).InclusiveBetween(0, 99.99m);

            RuleFor(x => x.Model.EffectiveFrom).NotEmpty();
          
        }
    }
}
