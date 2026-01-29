using Core.Application.City.Queries.GetCities;
using Core.Application.Common.HttpResponse;
using MediatR;

namespace Core.Application.City.Commands.DeleteCity
{
       public class DeleteCityCommand :  IRequest<bool>
       {
              public int Id { get; set; }                
       }
    
}