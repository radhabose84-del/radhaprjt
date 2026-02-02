using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.Common.Interfaces.IPaymentTermMaster;
using PurchaseManagement.Application.PaymentTermMaster.Command.DeletePaymentTermMaster;
using FluentValidation;

namespace PurchaseManagement.API.Validation.PaymentTermMaster
{
    public class DeletePaymentTermMasterCommandValidator  : AbstractValidator<DeletePaymentTermMasterCommand>
    {

        private readonly IPaymentTermMasterQueryRepository _paymentTermMasterQueryRepository;

        public DeletePaymentTermMasterCommandValidator(IPaymentTermMasterQueryRepository paymentTermMasterQueryRepository)
        {
            _paymentTermMasterQueryRepository = paymentTermMasterQueryRepository;
            RuleFor(x => x.Id)
                .GreaterThan(0);

            // exists (not soft-deleted)
            RuleFor(x => x.Id)
                .MustAsync(async (id, ct) => await _paymentTermMasterQueryRepository.ExistsByIdAsync(id))
                .WithMessage(id => $"Payment term not found for Id={id}.");

            // business rule: block delete when referenced
            // RuleFor(x => x.Id)
            //     .MustAsync(async (id, ct) => !await _paymentTermMasterQueryRepository.IsUsedAsync(id))
            //     .WithMessage("Cannot delete: payment term is referenced by other transactions.");

        }
    }
}