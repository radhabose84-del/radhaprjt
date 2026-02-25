#nullable disable
using PurchaseManagement.Application.Common.Interfaces.IPaymentTermMaster;
using PurchaseManagement.Application.PaymentTermMaster.Command.CreatePaymentTermMaster;
using FluentValidation;

namespace PurchaseManagement.Presentation.Validation.PaymentTermMaster
{
    public class CreatePaymentTermMasterCommandValidator : AbstractValidator<CreatePaymentTermMasterCommand>
    {    
          
          private readonly IPaymentTermMasterQueryRepository _paymentTermMasterQueryRepository;


        public CreatePaymentTermMasterCommandValidator(IPaymentTermMasterQueryRepository paymentTermMasterQueryRepository)
        {

            _paymentTermMasterQueryRepository = paymentTermMasterQueryRepository;
            var repo = _paymentTermMasterQueryRepository;
            
             RuleFor(x => x.Code)
            .NotEmpty().MaximumLength(30)
            .MustAsync(async (m, code, ct) =>
            {
                var normalized = code?.Trim();
                if (string.IsNullOrWhiteSpace(normalized)) return false;
                return !await repo.ExistsByCodeAsync(normalized);
            })
            .WithMessage(x => $"Payment term code '{x.Code?.Trim()}' already exists.");

            RuleFor(x => x.Description).NotEmpty().MaximumLength(200);
            RuleFor(x => x.BaselineTypeId).GreaterThan(0);
            RuleFor(x => x.CreditDays).GreaterThanOrEqualTo(0);

            // Advance allowed with or without installments
            RuleFor(x => x.AdvancePercent)
                .InclusiveBetween(0, 100)
                .When(x => x.AdvancePercent.HasValue);

            // Discount rules
            RuleFor(x => x.DiscountPercent)
                .InclusiveBetween(0, 100)
                .When(x => x.DiscountPercent.HasValue);

            RuleFor(x => x.DiscountDays)
                .GreaterThan(0)
                .When(x => x.DiscountPercent.HasValue && x.DiscountPercent.Value > 0);

            RuleFor(x => x.GraceDays)
                .GreaterThanOrEqualTo(0)
                .When(x => x.GraceDays.HasValue);

            // Installments present → structure + totals = 100 - Advance
            When(x => x.Installments != null && x.Installments.Count > 0, () =>
            {
                RuleForEach(x => x.Installments).ChildRules(c =>
                {
                    c.RuleFor(i => i.SeqNo).GreaterThanOrEqualTo(1);
                    c.RuleFor(i => i.Percent).InclusiveBetween(0, 100);
                    c.RuleFor(i => i.DueDays).GreaterThanOrEqualTo(0);
                });

                RuleFor(x => x.Installments)
                    .Must(list => list.Select(i => i.SeqNo).Distinct().Count() == list.Count)
                    .WithMessage("Installment SeqNo must be unique.");

                RuleFor(x => x.Installments)
                    .Must((x, list) =>
                        Math.Abs(list.Sum(i => i.Percent) - (100m - x.AdvancePercent.GetValueOrDefault())) <= 0.01m)
                    .WithMessage(x => $"Installments must total {100m - x.AdvancePercent.GetValueOrDefault():0.##}%."); 

               
            });

       
        }
    }
}