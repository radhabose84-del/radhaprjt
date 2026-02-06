using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace SalesManagement.API.Validation.Common
{
    public class ValidationService
    {

        public void AddValidationServices(IServiceCollection services)
        {

            services.AddScoped<MaxLengthProvider>();
           
        }  
    }
}