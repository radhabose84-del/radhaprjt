using FAM.Application.Common.Mappings;
using FAM.Domain.Entities;

namespace FAM.Application.Manufacture.Queries.GetManufacture
{
    public class ManufactureAutoCompleteDTO : IMapFrom<Manufactures>
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? ManufactureName { get; set; } 
    }
}