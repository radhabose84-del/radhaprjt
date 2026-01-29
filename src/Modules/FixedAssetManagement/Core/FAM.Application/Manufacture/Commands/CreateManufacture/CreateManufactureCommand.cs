using FAM.Application.Common.HttpResponse;
using FAM.Application.Manufacture.Queries.GetManufacture;
using MediatR;

namespace FAM.Application.Manufacture.Commands.CreateManufacture
{
    public class CreateManufactureCommand : IRequest<ManufactureDTO>
    {
        public string? Code { get; set; }        
        public string? ManufactureName { get; set; }                
        public int? ManufactureType { get; set; }
        public int CountryId { get; set; }        
        public int StateId { get; set; }        
        public int CityId { get; set; }        
        public string? AddressLine1 { get; set; }        
        public string? AddressLine2 { get; set; }        
        public string? PinCode { get; set; }        
        public string? PersonName { get; set; }        
        public string? PhoneNumber { get; set; }        
        public string? Email { get; set; }   
    }
}