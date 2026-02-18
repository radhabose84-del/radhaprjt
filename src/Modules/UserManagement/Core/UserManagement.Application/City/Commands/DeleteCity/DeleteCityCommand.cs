using UserManagement.Application.City.Queries.GetCities;
using Contracts.Common;
using MediatR;

namespace UserManagement.Application.City.Commands.DeleteCity
{
       public class DeleteCityCommand :  IRequest<bool>
       {
              public int Id { get; set; }                
       }
    
}