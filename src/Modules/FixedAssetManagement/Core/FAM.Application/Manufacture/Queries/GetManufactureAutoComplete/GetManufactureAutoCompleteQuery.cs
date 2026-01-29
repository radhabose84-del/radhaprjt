using FAM.Application.Common.HttpResponse;
using FAM.Application.Manufacture.Queries.GetManufacture;
using MediatR;

namespace FAM.Application.Manufacture.Queries.GetManufactureAutoComplete
{
    public class GetManufactureAutoCompleteQuery : IRequest<List<ManufactureAutoCompleteDTO>>
    {
         public string? SearchPattern { get; set; }
    }
}