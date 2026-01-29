using Core.Application.Common;
using Core.Application.Common.HttpResponse;
using Core.Application.Country.Queries.GetCountries;
using MediatR;

namespace Core.Application.Country.Queries.GetCountryById
{
    public class GetCountryByIdQuery : IRequest<CountryDto>
    {
        public int Id { get; set; }
    }
}