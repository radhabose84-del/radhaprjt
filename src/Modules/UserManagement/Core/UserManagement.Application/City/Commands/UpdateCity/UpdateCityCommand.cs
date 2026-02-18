using UserManagement.Application.City.Queries.GetCities;
using Contracts.Common;
using MediatR;

namespace UserManagement.Application.City.Commands.UpdateCity
{
       public class UpdateCityCommand : IRequest<CityDto>
       {
                public int Id { get; set; }
                public string? CityCode { get; set; }
                public string? CityName { get; set; }                
                public int StateId { get; set; }
                public byte IsActive { get; set; } 
         }
  
}