using UserManagement.Application.City.Queries.GetCities;
using UserManagement.Application.Common.HttpResponse;
using MediatR;

namespace UserManagement.Application.City.Commands.DeleteCity
{
       public class DeleteCityCommand :  IRequest<bool>
       {
              public int Id { get; set; }                
       }
    
}