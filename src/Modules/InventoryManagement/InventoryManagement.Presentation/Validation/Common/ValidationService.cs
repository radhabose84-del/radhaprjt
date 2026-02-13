using InventoryManagement.Application.Budget.Commands.CreateBudget;
using InventoryManagement.Application.Budget.Commands.UpdateBudget;
using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Application.HSNMaster.Command.CreateHSNMaster;
using InventoryManagement.Application.HSNMaster.Command.DeleteHSNMaster;
using InventoryManagement.Application.HSNMaster.Command.UpdateHSNMaster;
using InventoryManagement.Application.Issue.Command.CreateIssueEntry;
using InventoryManagement.Application.Item.ItemCategory.Commands.CreateItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Commands.DeleteItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Commands.UpdateItemCategory;
using InventoryManagement.Application.Item.ItemDetail.Commands.CreateItem;
using InventoryManagement.Application.Item.ItemDetail.Commands.UpdateItem;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using InventoryManagement.Application.Item.ItemGroup.Commands.CreateItemGroup;
using InventoryManagement.Application.Item.ItemGroup.Commands.DeleteItemGroup;
using InventoryManagement.Application.Item.ItemGroup.Commands.UpdateItemGroup;
using InventoryManagement.Application.Item.PutAway.Commands.CreatePutAwayRule;
using InventoryManagement.Application.Item.Templates.Commands.CreateTemplate;
using InventoryManagement.Application.Item.Templates.Commands.DeleteTemplate;
using InventoryManagement.Application.Item.Templates.Commands.UpdateTemplate;
using InventoryManagement.Application.MiscMaster;
using InventoryManagement.Application.MiscMaster.Command.CreateMiscMaster;
using InventoryManagement.Application.MiscMaster.Command.DeleteMiscMaster;
using InventoryManagement.Application.MiscMaster.Command.UpdateMiscMaster;
using InventoryManagement.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using InventoryManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using InventoryManagement.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using InventoryManagement.Application.MRS.Command.CreateMrsEntry;
using InventoryManagement.Application.MRS.Command.UpdateMrsEntry;
using InventoryManagement.Application.UOM.Command.CreateUOM;
using InventoryManagement.Application.UOM.Command.UpdateUOM;
using InventoryManagement.Application.UOMConversion.Command.CreateUOMConversion;
using InventoryManagement.Application.UOMConversion.Command.UpdateUOMConversion;
using FluentValidation;
using FluentValidation.AspNetCore;
using InventoryManagement.Presentation.Validation.Budget;
using InventoryManagement.Presentation.Validation.HSNMaster;
using InventoryManagement.Presentation.Validation.Issue;
using InventoryManagement.Presentation.Validation.Item.ItemCategory;
using InventoryManagement.Presentation.Validation.Item.ItemDetail;
using InventoryManagement.Presentation.Validation.Item.ItemGroup;
using InventoryManagement.Presentation.Validation.Item.PutAway;
using InventoryManagement.Presentation.Validation.Item.Templates;
using InventoryManagement.Presentation.Validation.MiscMaster;
using InventoryManagement.Presentation.Validation.MiscTypeMaster;
using InventoryManagement.Presentation.Validation.MRS;
using InventoryManagement.Presentation.Validation.UOM;
using InventoryManagement.Presentation.Validation.UOMConversion;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryManagement.Presentation.Validation.Common
{
    public class ValidationService
    {
        public void AddValidationServices(IServiceCollection services)
        {
            services.AddScoped<MaxLengthProvider>();
            services.AddScoped<IMaxLengthProvider, MaxLengthProvider>();            
            services.AddScoped<IValidator<CreateItemCategoryCommand>, CreateItemCategoryCommandValidator>();
            services.AddScoped<IValidator<DeleteItemCategoryCommand>, DeleteItemCategoryCommandValidator>();
            services.AddScoped<IValidator<UpdateItemCategoryCommand>, UpdateItemCategoryCommandValidator>();
            services.AddScoped<IValidator<CreateItemGroupCommand>, CreateItemGroupCommandValidator>();
            services.AddScoped<IValidator<UpdateItemGroupCommand>, UpdateItemGroupCommandValidator>();
            services.AddScoped<IValidator<DeleteItemGroupCommand>, DeleteItemGroupCommandValidator>();

            services.AddScoped<IValidator<CreateMiscTypeMasterCommand>, CreateMiscTypeMasterCommandValidator>();
            services.AddScoped<IValidator<DeleteMiscTypeMasterCommand>, DeleteMiscTypeMasterCommandValidator>();
            services.AddScoped<IValidator<UpdateMiscTypeMasterCommand>, UpdateMiscTypeMasterCommandValidator>();
            services.AddScoped<IValidator<CreateMiscMasterCommand>, CreateMiscMasterCommandValidator>();
            services.AddScoped<IValidator<DeleteMiscMasterCommand>, DeleteMiscMasterCommandValidator>();
            services.AddScoped<IValidator<UpdateMiscMasterCommand>, UpdateMiscMasterCommandValidator>();
            services.AddScoped<IValidator<CreateBudgetCommand>, CreateBudgetCommandValidator>();
            services.AddScoped<IValidator<UpdateBudgetCommand>, UpdateBudgetCommandValidator>();

            services.AddScoped<IValidator<CreateHSNMasterCommand>, CreateHSNMasterCommandValidator>();
            services.AddScoped<IValidator<UpdateHSNMasterCommand>, UpdateHSNMasterCommandValidator>();
            services.AddScoped<IValidator<DeleteHSNMasterCommand>, DeleteHSNMasterCommandValidator>();
            services.AddScoped<IValidator<CreateUOMCommand>, CreateUOMCommandValidator>();
            services.AddScoped<IValidator<UpdateUOMCommand>, UpdateUOMCommandValidator>();
            services.AddScoped<IValidator<CreateUOMConversionCommand>, CreateUOMConversionCommandValidator>();
            services.AddScoped<IValidator<UpdateUOMConversionCommand>, UpdateUOMConversionCommandValidator>();


            services.AddScoped<IValidator<CreateItemCommand>, CreateItemCommandValidator>();
            services.AddScoped<IValidator<UpdateItemCommand>, UpdateItemCommandValidator>();
            services.AddScoped<IValidator<ItemPurchaseDto>, ItemPurchaseDtoValidator>();
            services.AddScoped<IValidator<ItemInventoryDto>, ItemInventoryDtoValidator>();
            services.AddScoped<IValidator<ItemQualityDto>, ItemQualityDtoValidator>();
            services.AddScoped<IValidator<ItemSupplierDto>, ItemSupplierDtoValidator>();
            services.AddScoped<IValidator<ItemManufactureDto>, ItemManufacturingDtoValidator>();
            services.AddScoped<IValidator<ItemUomDto>, ItemUomDtoValidator>();
            //services.AddValidatorsFromAssembly(typeof(CreateItemCommandValidator).Assembly);
            services.AddScoped<IValidator<CreateTemplateCommand>, CreateTemplateCommandValidator>();
            services.AddScoped<IValidator<UpdateTemplateCommand>, UpdateTemplateCommandValidator>();
            services.AddScoped<IValidator<DeleteTemplateCommand>, DeleteTemplateCommandValidator>();
            services.AddScoped<IValidator<CreatePutAwayRuleCommand>, CreatePutAwayRuleCommandValidator>();
            services.AddScoped<IValidator<ItemDto>, CreateItemTemplateCommandValidator>();
            services.AddScoped<IValidator<ItemDto>, CreateItemVariantsCommandValidator>();
            services.AddScoped<IValidator<CreateMrsEntryCommand>, CreateMrsEntryCommandValidator>();
            services.AddScoped<IValidator<UpdateMrsEntryCommand>, UpdateMrsEntryCommandValidator>();
            services.AddScoped<IValidator<CreateIssueEntryCommand>, CreateIssueEntryCommandValidator>();
        }
    }
}
