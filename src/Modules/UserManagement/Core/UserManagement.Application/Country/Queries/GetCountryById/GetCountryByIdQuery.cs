using UserManagement.Application.Common;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.Country.Queries.GetCountries;
using MediatR;

namespace UserManagement.Application.Country.Queries.GetCountryById
{
    public class GetCountryByIdQuery : IRequest<CountryDto>
    {
        public int Id { get; set; }
    }
}