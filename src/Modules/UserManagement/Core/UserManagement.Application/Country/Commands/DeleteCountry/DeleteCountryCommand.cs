using UserManagement.Application.Country.Queries.GetCountries;
using MediatR;

namespace UserManagement.Application.Country.Commands.DeleteCountry
{
    public class DeleteCountryCommand :  IRequest<CountryDto>
       {
                public int Id { get; set; }                
       }
    
}