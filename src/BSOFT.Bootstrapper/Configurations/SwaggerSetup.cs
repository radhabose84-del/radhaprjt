using Microsoft.OpenApi.Models;

namespace BSOFT.Bootstrapper.Configurations
{
    public static class SwaggerSetup
    {
        public static void AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                // ✅ ADD THIS: Fix schema ID conflicts for duplicate type names
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
            });
        }
    }
}

// using Microsoft.OpenApi.Models;
// namespace BSOFT.Bootstrapper.Configurations
// {
//     public static class SwaggerSetup
//     {
//         public static void AddSwaggerDocumentation(this IServiceCollection services)
//         {
//             services.AddEndpointsApiExplorer();
//             services.AddSwaggerGen(options =>
//             {
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
//             });
//         }
//     }
// }
