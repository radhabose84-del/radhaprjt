using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;

namespace BSOFT.Bootstrapper.Configurations
{
    public static class SwaggerSetup
    {
        public static void AddSwaggerDocumentation(
            this IServiceCollection services,
            IEnumerable<SwaggerModuleInfo>? moduleInfos = null)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                // バ. Fix schema ID conflicts for duplicate type names
                options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));

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

                var docs = (moduleInfos ?? new[] { SwaggerModuleInfo.Default }).ToArray();

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
                    if (docs.Length == 1 && string.IsNullOrWhiteSpace(docs[0].NamespaceFilter))
                        return true;

                    var doc = docs.FirstOrDefault(d => d.DocumentName == docName);
                    if (doc == null)
                        return false;

                    if (string.IsNullOrWhiteSpace(doc.NamespaceFilter))
                        return true;

                    var controllerAction = apiDesc.ActionDescriptor as ControllerActionDescriptor;
                    var controllerNamespace = controllerAction?.ControllerTypeInfo?.Namespace ?? string.Empty;
                    return controllerNamespace.Contains(doc.NamespaceFilter, StringComparison.OrdinalIgnoreCase);
                });
            });
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
