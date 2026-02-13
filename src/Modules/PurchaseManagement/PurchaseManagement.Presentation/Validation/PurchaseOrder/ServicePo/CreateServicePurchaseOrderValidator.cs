// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Contracts.Interfaces.External.IWorkflow;
// using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO;
// using PurchaseManagement.Application.PurchaseOrder.Dtos.ServicePO;
// using FluentValidation;
// using PurchaseManagement.Presentation.Validation.Common;
// using Shared.Validation.Common;

// namespace PurchaseManagement.Presentation.Validation.PurchaseOrder.ServicePo
// {
//     public class CreateServicePurchaseOrderValidator : AbstractValidator<CreateServicePurchaseOrderDto>
//     {

//         private readonly IWorkflowGrpcClient _workflowGrpcClient;
//         private readonly IServicePurchaseOrderCommandRepository _servicePurchaseOrderCommandRepository;         
//         private readonly List<ValidationRule> _validationRules;
       
         
//         public CreateServicePurchaseOrderValidator(IWorkflowGrpcClient workflowGrpcClient , List<ValidationRule> validationRules , IServicePurchaseOrderCommandRepository servicePurchaseOrderCommandRepository)
//         {
//             _workflowGrpcClient = workflowGrpcClient;
//             _validationRules = validationRules;
//             _servicePurchaseOrderCommandRepository = servicePurchaseOrderCommandRepository;

//             if (_validationRules == null || !_validationRules.Any())
//             {
//                 throw new InvalidOperationException("Validation rules could not be loaded.");
//             }
//              foreach (var rule in _validationRules)
//             {
//                 switch (rule.Rule)
//                 {

//                    case "Workflow":
//                                     RuleFor(x => x.UnitId)
//                                 .MustAsync(async (unitId, cancellation) =>
//                                     await _workflowGrpcClient.IsApproveWorkflowConfigure(
//                                         PurchaseManagement.Domain.Common.MiscEnumEntity.ServicePO, // entity type
//                                         unitId,
//                                         0))                      // DepartmentId not required, pass null
//                                 .WithMessage(rule.Error);
//                             break;
//                 }
//             }

//             RuleFor(x => x.UnitId).GreaterThan(0);
//             RuleFor(x => x.PODate).NotEmpty();
//             RuleFor(x => x.VendorId).GreaterThan(0);
//             RuleForEach(x => x.ServicePos).SetValidator(new ServiceHeaderValidator());
//         }

//         private class ServiceHeaderValidator : AbstractValidator<PurchaseOrderServiceHeaderDto>
//         {
//             public ServiceHeaderValidator()
//             {
//                 RuleFor(x => x.ServiceCategoryId).GreaterThan(0);
//                 RuleForEach(x => x.Lines).SetValidator(new ServiceLineValidator());
//             }
//         }

//         private class ServiceLineValidator : AbstractValidator<PurchaseOrderServiceLineDto>
//         {
//             public ServiceLineValidator()
//             {
//                 RuleFor(x => x.PlannedQuantity).GreaterThan(0);
//                 RuleFor(x => x.PlannedRate).GreaterThan(0);
//             }
//         }
//     }
// }