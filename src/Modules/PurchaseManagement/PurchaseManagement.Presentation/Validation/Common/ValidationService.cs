using PurchaseManagement.Application.MiscMaster.Command.CreateMiscMaster;
using PurchaseManagement.Application.MiscMaster.Command.DeleteMiscMaster;
using PurchaseManagement.Application.MiscMaster.Command.UpdateMiscMaster;
using PurchaseManagement.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using PurchaseManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using PurchaseManagement.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using PurchaseManagement.Application.PaymentTermMaster.Command.CreatePaymentTermMaster;
using PurchaseManagement.Application.PaymentTermMaster.Command.DeletePaymentTermMaster;
using PurchaseManagement.Application.PaymentTermMaster.Command.UpdatePaymentTermMaster;
using PurchaseManagement.Application.Quotation.RfqEntry.Commands.Create;
using PurchaseManagement.Application.TnCTemplateMaster.Command.CreateTnCTemplateMasterCommand;
using PurchaseManagement.Application.TnCTemplateMaster.Command.DeleteTnCTemplateMasterCommand;
using PurchaseManagement.Application.TnCTemplateMaster.Command.UpdateTnCTemplateMasterCommand;
using FluentValidation;
using PurchaseManagement.Presentation.Validation.MiscMaster;
using PurchaseManagement.Presentation.Validation.MiscTypeMaster;
using PurchaseManagement.Presentation.Validation.PaymentTermMaster;
using PurchaseManagement.Presentation.Validation.TnCTemplateMaster;
using PurchaseManagement.Presentation.Validation.Quotation.RfqEntry;
using PurchaseManagement.Presentation.Validation.PurchaseIndent;
using PurchaseManagement.Application.PurchaseIndents.Command.CreatePurchaseIndent;
using PurchaseManagement.Application.Quotation.QuotationCompare.Commands.CreateQuoteComparsion;
using PurchaseManagement.Presentation.Validation.Quotation.QuotationCompare;
using PurchaseManagement.Application.PurchaseIndents.Command.UpdatePurchaseIndent;
using PurchaseManagement.Application.PriceMaster.Commands.Create;
using PurchaseManagement.Application.PriceMaster.Commands.Update;
using PurchaseManagement.Application.GRN.GateEntry.Commands.CreateGateEntry;
using PurchaseManagement.Presentation.Validation.GRN.GateEntry;
using PurchaseManagement.Application.GRN.GRNEntry.Commands;
using PurchaseManagement.Presentation.Validation.GRN.GRNEntry;
using PurchaseManagement.Application.GRN.GRNEntry.Commands.UpdateGRNEntry;
using PurchaseManagement.Application.GRN.GRNEntry.Commands.CreateGRNPutaway;
using PurchaseManagement.Application.ServiceMaster.Commands.CreateService;
using PurchaseManagement.Presentation.Validation.ServiceMaster;
using PurchaseManagement.Application.ServiceMaster.Commands.UpdateService;
using PurchaseManagement.Application.ServiceMaster.Commands.DeleteService;
using PurchaseManagement.Application.MRS.Command.CreateMrsEntry;
using PurchaseManagement.Presentation.Validation.MRS;
using PurchaseManagement.Application.MRS.Command.UpdateMrsEntry;
using PurchaseManagement.Application.Issue.Command.CreateIssueEntry;
using PurchaseManagement.Presentation.Validation.Issue;
using PurchaseManagement.Presentation.Validation.PurchaseOrder.ServicePo;
using PurchaseManagement.Presentation.Validation.ExchangeRate;
using PurchaseManagement.Application.ExchangeRate.Commands;
using PurchaseManagement.Application.ExchangeRate.Queries.GetLatestRate;
using PurchaseManagement.Application.Port.Commands;
using PurchaseManagement.Application.PurchaseOrder.ImportPO.Command.Create;
using PurchaseManagement.Application.PurchaseOrder.ImportPO.Validation;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ImportPO;
using PurchaseManagement.Application.PurchaseOrder.ImportPO.Command.ImportPOAmendment;
using PurchaseManagement.Application.PurchaseOrder.Local.Commands.Amend;
using PurchaseManagement.Application.PurchaseOrder.ServicePO.Command.CreateServiceEntrySheet;
using PurchaseManagement.Application.PurchaseOrder.BillEntry.Commands.Create;
using PurchaseManagement.Presentation.Validation.PurchaseOrder.BillEntry;
using PurchaseManagement.Application.PurchaseOrder.BillEntry.Commands.Update;
using PurchaseManagement.Application.PurchaseOrder.BillEntry.Dto;
using PurchaseManagement.Application.PurchaseOrder.BillEntry.Queries.GetAll;
using PurchaseManagement.Application.IssueReturn.Command.CreateIssueReturn;
using PurchaseManagement.Presentation.Validation.IssueReturn;
using PurchaseManagement.Application.ContractPO.Commands.Create;
using PurchaseManagement.Application.ContractPO.Commands.Update;
using PurchaseManagement.Application.ContractPO.Commands.Delete;
using PurchaseManagement.Presentation.Validation.ContractPO;
using Microsoft.Extensions.DependencyInjection;


namespace PurchaseManagement.Presentation.Validation.Common
{
    public class ValidationService
    {
        public void AddValidationServices(IServiceCollection services)
        {
            services.AddScoped<MaxLengthProvider>();

            services.AddScoped<IValidator<CreateMiscTypeMasterCommand>, CreateMiscTypeMasterCommandValidator>();
            services.AddScoped<IValidator<DeleteMiscTypeMasterCommand>, DeleteMiscTypeMasterCommandValidator>();
            services.AddScoped<IValidator<UpdateMiscTypeMasterCommand>, UpdateMiscTypeMasterCommandValidator>();
            services.AddScoped<IValidator<CreateMiscMasterCommand>, CreateMiscMasterCommandValidator>();
            services.AddScoped<IValidator<DeleteMiscMasterCommand>, DeleteMiscMasterCommandValidator>();
            services.AddScoped<IValidator<UpdateMiscMasterCommand>, UpdateMiscMasterCommandValidator>();
            services.AddScoped<IValidator<CreatePaymentTermMasterCommand>, CreatePaymentTermMasterCommandValidator>();
            services.AddScoped<IValidator<UpdatePaymentTermMasterCommand>, UpdatePaymentTermMasterCommandValidator>();
            services.AddScoped<IValidator<DeletePaymentTermMasterCommand>, DeletePaymentTermMasterCommandValidator>();
            services.AddScoped<IValidator<CreateRfqCommand>, CreateRfqValidator>();
            services.AddScoped<IValidator<UpdateRfqCommand>, UpdateRfqValidator>();
            services.AddScoped<IValidator<CreateTnCTemplateMasterCommand>, CreateTnCTemplateMasterValidator>();
            services.AddScoped<IValidator<UpdateTnCTemplateMasterCommand>, UpdateTnCTemplateMasterValidator>();
            services.AddScoped<IValidator<DeleteTnCTemplateMasterCommand>, DeleteTnCTemplateMasterValidator>();
            services.AddScoped<IValidator<CreatePurchaseIndentCommand>, CreatePurchaseIndentCommandValidator>();
            services.AddScoped<IValidator<CreateQuoteComparsionCommand>, CreateQuotationCompareValidator>();
            services.AddScoped<IValidator<UpdatePurchaseIndentCommand>, UpdatePurchaseIndentCommandValidator>();
            services.AddScoped<IValidator<CreatePriceMasterCommand>, CreatePriceMasterCommandValidator>();
            services.AddScoped<IValidator<UpdatePriceMasterCommand>, UpdatePriceMasterCommandValidator>();
            services.AddScoped<IValidator<CreateGateEntryCommand>, CreateGateEntryCommandValidator>();
            services.AddScoped<IValidator<CreateGRNEntryCommand>, CreateGRNEntryCommandValidator>();
            services.AddScoped<IValidator<UpdateGRNEntryCommand>, UpdateGRNEntryCommandValidator>();
            services.AddScoped<IValidator<CreateGRNPutawayCommand>, CreateGRNPutawayCommandValidator>();
            services.AddScoped<IValidator<CreateServiceCommand>, CreateServiceMasterCommandValidator>();
            services.AddScoped<IValidator<UpdateServiceCommand>, UpdateServiceMasterCommandValidator>();
            services.AddScoped<IValidator<DeleteServiceCommand>, SoftDeleteServiceCommandValidator>();
            services.AddScoped<IValidator<CreateMrsEntryCommand>, CreateMrsEntryCommandValidator>();
            services.AddScoped<IValidator<UpdateMrsEntryCommand>, UpdateMrsEntryCommandValidator>();
            services.AddScoped<IValidator<CreateIssueEntryCommand>, CreateIssueEntryCommandValidator>();



            services.AddScoped<IValidator<GetLatestRateQuery>, GetLatestRateQueryValidator>();
            services.AddScoped<IValidator<ExchangeRateCommand>, ExchangeRateCommandValidator>();

            services.AddScoped<IValidator<CreatePortMasterCommand>, CreatePortMasterValidator>();
            services.AddScoped<IValidator<DeletePortMasterCommand>, DeletePortMasterValidator>();
            services.AddScoped<IValidator<UpdatePortMasterCommand>, UpdatePortMasterValidator>();

            services.AddScoped<IValidator<CreateImportPOCommand>, CreateImportPOCommandValidator>();
            services.AddScoped<IValidator<UpdateImportPOCommand>, UpdateImportPOCommandValidator>();
            services.AddScoped<IValidator<ImportPOCreateDto>, ImportPOCreateDtoValidator>();
            services.AddScoped<IValidator<ImportPOHeaderDto>, ImportPOHeaderDtoValidator>();
            services.AddScoped<IValidator<ImportPODetailDto>, ImportPODetailDtoValidator>();
            services.AddScoped<IValidator<ImportPurchasePaymentTermDto>, PurchasePaymentTermDtoValidator>();
            services.AddScoped<IValidator<ImportPOAmendmentCommand>, ImportPoAmendmentCommandValidator>();

            services.AddScoped<IValidator<CreateServiceEntrySheetCommand>, CreateServiceEntrySheetValidator>();

            services.AddScoped<IValidator<CreatePurchaseBillEntryCommand>, CreatePurchaseBillEntryCommandValidator>();
            services.AddScoped<IValidator<UpdatePurchaseBillEntryCommand>, UpdatePurchaseBillEntryCommandValidator>();
            services.AddScoped<IValidator<PurchaseBillEntryDetailDto>, PurchaseBillEntryCommandValidator>();
            services.AddScoped<IValidator<PurchaseBillEntryHeaderDto>, PurchaseBillEntryDtoValidator>();
            services.AddScoped<IValidator<GetAllPurchaseBillEntryQuery>, GetPurchaseBillEntryListQueryValidator>(); 
            services.AddScoped<IValidator<CreateIssueReturnEntryCommand>, CreateIssueReturnEntryCommandValidator>();

            services.AddScoped<IValidator<CreateContractPOCommand>, CreateContractPOValidator>();
            services.AddScoped<IValidator<UpdateContractPOCommand>, UpdateContractPOValidator>();
            services.AddScoped<IValidator<DeleteContractPOCommand>, DeleteContractPOValidator>();

        }
    }
}
