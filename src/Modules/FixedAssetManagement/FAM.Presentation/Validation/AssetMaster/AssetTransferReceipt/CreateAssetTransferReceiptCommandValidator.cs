using FAM.Application.AssetMaster.AssetTransferReceipt.Command.CreateAssetTransferReceipt;
using FluentValidation;

namespace FAM.Presentation.Validation.AssetMaster.AssetTransferReceipt
{
    public class CreateAssetTransferReceiptCommandValidator  : AbstractValidator<CreateAssetTransferReceiptCommand>
    {
        public CreateAssetTransferReceiptCommandValidator()
        {
            RuleFor(x => x.AssetTransferReceiptHdrDto.DocDate)
                .NotEmpty().WithMessage("Document Date is required.")
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Document Date cannot be in the future.");

            // RuleFor(x => x.AssetTransferReceiptHdrDto.TransferType)
            //      .NotEmpty().WithMessage("Transfer Type is required.");

            // RuleFor(x => x.AssetTransferReceiptHdrDto.FromUnitId)
            //     .GreaterThan(0).WithMessage("From Unit ID must be greater than 0.");

            // RuleFor(x => x.AssetTransferReceiptHdrDto.ToUnitId)
            //     .GreaterThan(0).WithMessage("To Unit ID must be greater than 0.");

            // RuleFor(x => x.AssetTransferReceiptHdrDto.FromDepartmentId)
            //     .GreaterThan(0).WithMessage("From Department ID must be greater than 0.");

            // RuleFor(x => x.AssetTransferReceiptHdrDto.ToDepartmentId)
            //     .GreaterThan(0).WithMessage("To Department ID must be greater than 0.");

            // RuleFor(x => x.AssetTransferReceiptHdrDto.FromCustodianId)
            //     .GreaterThan(0).WithMessage("From Custodian ID must be greater than 0.");

            // RuleFor(x => x.AssetTransferReceiptHdrDto.ToCustodianId)
            //     .GreaterThan(0).WithMessage("To Custodian ID must be greater than 0.");

            // RuleFor(x => x.AssetTransferReceiptHdrDto.FromCustodianName)
            //      .NotEmpty().WithMessage("FromCustodianName Type is required.");

            // RuleFor(x => x.AssetTransferReceiptHdrDto.ToCustodianName)
            //      .NotEmpty().WithMessage("ToCustodianName Type is required.");

            // RuleFor(x => x.AssetTransferReceiptHdrDto.AssetTransferIssueHdr.AssetTransferId)
            //     .GreaterThan(0).WithMessage("AssetTransferId must be greater than 0.");

            RuleForEach(x => x.AssetTransferReceiptHdrDto.AssetTransferReceiptDtl)
                .ChildRules(asset =>
                {
                    asset.RuleFor(a => a.AssetId)
                    .GreaterThan(0)
                    .WithMessage("AssetId must be greater than 0.");
                });

            //   RuleForEach(x => x.AssetTransferReceiptHdrDto.AssetTransferReceiptDtl)
            //     .ChildRules(asset =>
            //     {
            //         asset.RuleFor(a => a.LocationId)
            //         .GreaterThan(0)
            //         .WithMessage("LocationId must be greater than 0.");
            //     });

            //         RuleForEach(x => x.AssetTransferReceiptHdrDto.AssetTransferReceiptDtl)
            //     .ChildRules(asset =>
            //     {
            //         asset.RuleFor(a => a.SubLocationId)
            //         .GreaterThan(0)
            //         .WithMessage("SubLocationId must be greater than 0.");
            //     });




            
            

            

            
            
   
        }
    }
}