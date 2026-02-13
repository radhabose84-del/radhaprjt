using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SalesManagement.Presentation.Validation.Common;

namespace SalesManagement.Presentation.Validation.Common
{
    public class ValidationService
    {

        public void AddValidationServices(IServiceCollection services)
        {

            services.AddScoped<MaxLengthProvider>();
           
        }  
    }
}