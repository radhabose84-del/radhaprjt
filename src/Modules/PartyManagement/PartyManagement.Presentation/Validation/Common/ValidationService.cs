using PartyManagement.Application.BankAccount.Command.CreateBankAccount;
using PartyManagement.Application.BankAccount.Command.DeleteBankAccount;
using PartyManagement.Application.BankAccount.Command.UpdateBankAccount;
using PartyManagement.Application.BankMaster.Command.Create;
using PartyManagement.Application.BankMaster.Command.Update;
using PartyManagement.Application.MiscMaster.Command.CreateMiscMaster;
using PartyManagement.Application.MiscMaster.Command.DeleteMiscMaster;
using PartyManagement.Application.MiscMaster.Command.UpdateMiscMaster;
using PartyManagement.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using PartyManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using PartyManagement.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using PartyManagement.Application.PartyGroup.Command.CreatePartyGroup;
using PartyManagement.Application.PartyGroup.Command.DeletePartyGroup;
using PartyManagement.Application.PartyGroup.Command.UpdatePartyGroup;
using PartyManagement.Application.PartyMaster.Command.CreatePartyMaster;
using PartyManagement.Application.PartyMaster.Command.DeletePartyMaster;
using FluentValidation;
using PartyManagement.Presentation.Validation.BankAccount;
using PartyManagement.Presentation.Validation.BankMaster;
using PartyManagement.Presentation.Validation.MiscMaster;
using PartyManagement.Presentation.Validation.MiscTypeMaster;
using PartyManagement.Presentation.Validation.PartyGroup;
using PartyManagement.Presentation.Validation.PartyMaster;
using Microsoft.Extensions.DependencyInjection;

namespace PartyManagement.Presentation.Validation.Common
{
    public class ValidationService
    {
        public void AddValidationServices(IServiceCollection services)
        {
            services.AddScoped<MaxLengthProvider>();
            services.AddScoped<IValidator<CreatePartyGroupCommand>, CreatePartyGroupCommandValidator>();
            services.AddScoped<IValidator<DeletePartyGroupCommand>, DeletePartyGroupCommandValidator>();
            services.AddScoped<IValidator<UpdatePartyGroupCommand>, UpdatePartyGroupCommandValidator>();
            services.AddScoped<IValidator<CreateMiscTypeMasterCommand>, CreateMiscTypeMasterCommandValidator>();
            services.AddScoped<IValidator<DeleteMiscTypeMasterCommand>, DeleteMiscTypeMasterCommandValidator>();
            services.AddScoped<IValidator<UpdateMiscTypeMasterCommand>, UpdateMiscTypeMasterCommandValidator>();
            services.AddScoped<IValidator<CreateMiscMasterCommand>, CreateMiscMasterCommandValidator>();
            services.AddScoped<IValidator<DeleteMiscMasterCommand>, DeleteMiscMasterCommandValidator>();
            services.AddScoped<IValidator<UpdateMiscMasterCommand>, UpdateMiscMasterCommandValidator>();
            services.AddScoped<IValidator<CreatePartyMasterCommand>, CreatePartyMasterCommandValidator>();
            services.AddScoped<IValidator<DeletePartyMasterCommand>, DeletePartyMasterCommandValidator>();


            services.AddScoped<IValidator<CreateBankMasterCommand>, CreateBankMasterValidator>();
            services.AddScoped<IValidator<UpdateBankMasterCommand>, UpdateBankMasterValidator>();            

            services.AddScoped<IValidator<CreateBankAccountCommand>, CreateBankAccountCommandValidator>();
            services.AddScoped<IValidator<DeleteBankAccountCommand>, DeleteBankAccountCommandValidator>();
            services.AddScoped<IValidator<UpdateBankAccountCommand>, UpdateBankAccountCommandValidator>();

        }
    }
}