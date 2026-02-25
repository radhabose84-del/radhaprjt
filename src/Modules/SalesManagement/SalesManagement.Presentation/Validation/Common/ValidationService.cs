using Microsoft.Extensions.DependencyInjection;

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