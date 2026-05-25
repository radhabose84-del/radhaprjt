using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;

namespace BSOFT.Api.Configurations
{
    public static class SwaggerSetup
    {
        // ✅ Default module docs
        private static readonly SwaggerModuleInfo[] swaggerModuleDocs =
        {
            new SwaggerModuleInfo("UserManagement", "User Management API", "v1", "UserManagement.Presentation.Controllers"),
            new SwaggerModuleInfo("FixedAssetManagement", "Fixed Asset Management API", "v1", "FAM.Presentation.Controllers"),
            new SwaggerModuleInfo("MaintenanceManagement", "Maintenance Management API", "v1", "MaintenanceManagement.Presentation.Controllers"),
            new SwaggerModuleInfo("PurchaseManagement", "Purchase Management API", "v1", "PurchaseManagement.Presentation.Controllers"),
            new SwaggerModuleInfo("InventoryManagement", "Inventory Management API", "v1", "InventoryManagement.Presentation.Controllers"),
            new SwaggerModuleInfo("PartyManagement", "Party Management API", "v1", "PartyManagement.Presentation.Controllers"),
			new SwaggerModuleInfo("BudgetManagement", "Budget Management API", "v1", "BudgetManagement.Presentation.Controllers"),
            new SwaggerModuleInfo("WarehouseManagement", "Warehouse Management API", "v1", "WarehouseManagement.Presentation.Controllers"),
            new SwaggerModuleInfo("SalesManagement", "Sales Management API", "v1", "SalesManagement.Presentation.Controllers"),
            new SwaggerModuleInfo("ProjectManagement", "Project Management API", "v1", "ProjectManagement.Presentation.Controllers"),
            new SwaggerModuleInfo("BackgroundService", "Background Service API", "v1", "BackgroundService.Presentation.Controllers"),
            new SwaggerModuleInfo("GateEntryManagement", "Gate Entry Management API", "v1", "GateEntryManagement.Presentation.Controllers"),
            new SwaggerModuleInfo("FinanceManagement", "Finance Management API", "v1", "FinanceManagement.Presentation.Controllers"),
            new SwaggerModuleInfo("ProductionManagement", "Production Management API", "v1", "ProductionManagement.Presentation.Controllers"),
            new SwaggerModuleInfo("LogisticsManagement", "Logistics Management API", "v1", "LogisticsManagement.Presentation.Controllers"),
            new SwaggerModuleInfo("QCManagement", "QC Management API", "v1", "QCManagement.Presentation.Controllers")
        };

        // ✅ Expose for Program.cs SwaggerUI dropdown
        public static IReadOnlyCollection<SwaggerModuleInfo> DefaultModuleDocs => swaggerModuleDocs;

        public static IServiceCollection AddSwaggerDocumentation(
            this IServiceCollection services,
            IEnumerable<SwaggerModuleInfo>? moduleInfos = null)
        {
            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(options =>
            {
                // Map DateOnly/TimeOnly so Swagger generates "string" + "date"/"time" format
                // instead of a complex object with year/month/day properties
                options.MapType<DateOnly>(() => new OpenApiSchema
                {
                    Type = "string",
                    Format = "date"
                });
                options.MapType<TimeOnly>(() => new OpenApiSchema
                {
                    Type = "string",
                    Format = "time"
                });

                // Fix schema ID conflicts for duplicate type names
                options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));

                // JWT Bearer auth
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' followed by your token."
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                var docs = (moduleInfos ?? swaggerModuleDocs).ToArray();

                foreach (var doc in docs)
                {
                    options.SwaggerDoc(doc.DocumentName, new OpenApiInfo
                    {
                        Title = doc.Title,
                        Version = doc.Version
                    });
                }

                options.DocInclusionPredicate((docName, apiDesc) =>
                {
                    // Single-doc mode without namespace filter => include everything
                    if (docs.Length == 1 && string.IsNullOrWhiteSpace(docs[0].NamespaceFilter))
                        return true;

                    var doc = docs.FirstOrDefault(d => d.DocumentName == docName);
                    if (doc == null)
                        return false;

                    // If doc has no filter => include everything in that doc
                    if (string.IsNullOrWhiteSpace(doc.NamespaceFilter))
                        return true;

                    var controllerAction = apiDesc.ActionDescriptor as ControllerActionDescriptor;
                    var controllerNamespace = controllerAction?.ControllerTypeInfo?.Namespace ?? string.Empty;

                    // Include endpoints whose controller namespace matches the module filter
                    return controllerNamespace.Contains(doc.NamespaceFilter, StringComparison.OrdinalIgnoreCase);
                });
            });

            return services;
        }
    }

    public sealed class SwaggerModuleInfo
    {
        public static SwaggerModuleInfo Default => new("v1", "BSOFT API", "v1", null);

        public string DocumentName { get; }
        public string Title { get; }
        public string Version { get; }
        public string? NamespaceFilter { get; }

        public SwaggerModuleInfo(string documentName, string title, string version, string? namespaceFilter)
        {
            DocumentName = documentName;
            Title = title;
            Version = version;
            NamespaceFilter = namespaceFilter;
        }
    }
}


// using System;
// using System.Collections.Generic;
// using System.Linq;
// using Microsoft.AspNetCore.Mvc.Controllers;
// using Microsoft.OpenApi.Models;

// namespace BSOFT.Api.Configurations
// {
//     public static class SwaggerSetup
//     {
//         public static void AddSwaggerDocumentation(
//             this IServiceCollection services,
//             IEnumerable<SwaggerModuleInfo>? moduleInfos = null)
//         {
//             services.AddEndpointsApiExplorer();
//             services.AddSwaggerGen(options =>
//             {
//                 // バ. Fix schema ID conflicts for duplicate type names
//                 options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));

//                 options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//                 {
//                     Name = "Authorization",
//                     Type = SecuritySchemeType.Http,
//                     Scheme = "Bearer",
//                     BearerFormat = "JWT",
//                     In = ParameterLocation.Header,
//                     Description = "Enter 'Bearer' followed by your token."
//                 });

//                 options.AddSecurityRequirement(new OpenApiSecurityRequirement
//                 {
//                     {
//                         new OpenApiSecurityScheme
//                         {
//                             Reference = new OpenApiReference
//                             {
//                                 Type = ReferenceType.SecurityScheme,
//                                 Id = "Bearer"
//                             }
//                         },
//                         Array.Empty<string>()
//                     }
//                 });

//                 var docs = (moduleInfos ?? new[] { SwaggerModuleInfo.Default }).ToArray();

//                 foreach (var doc in docs)
//                 {
//                     options.SwaggerDoc(doc.DocumentName, new OpenApiInfo
//                     {
//                         Title = doc.Title,
//                         Version = doc.Version
//                     });
//                 }

//                 options.DocInclusionPredicate((docName, apiDesc) =>
//                 {
//                     if (docs.Length == 1 && string.IsNullOrWhiteSpace(docs[0].NamespaceFilter))
//                         return true;

//                     var doc = docs.FirstOrDefault(d => d.DocumentName == docName);
//                     if (doc == null)
//                         return false;

//                     if (string.IsNullOrWhiteSpace(doc.NamespaceFilter))
//                         return true;

//                     var controllerAction = apiDesc.ActionDescriptor as ControllerActionDescriptor;
//                     var controllerNamespace = controllerAction?.ControllerTypeInfo?.Namespace ?? string.Empty;
//                     return controllerNamespace.Contains(doc.NamespaceFilter, StringComparison.OrdinalIgnoreCase);
//                 });
//             });
//         }
//     }

//     public sealed class SwaggerModuleInfo
//     {
//         public static SwaggerModuleInfo Default => new("v1", "BSOFT API", "v1", null);

//         public string DocumentName { get; }
//         public string Title { get; }
//         public string Version { get; }
//         public string? NamespaceFilter { get; }

//         public SwaggerModuleInfo(string documentName, string title, string version, string? namespaceFilter)
//         {
//             DocumentName = documentName;
//             Title = title;
//             Version = version;
//             NamespaceFilter = namespaceFilter;
//         }
//     }
// }

