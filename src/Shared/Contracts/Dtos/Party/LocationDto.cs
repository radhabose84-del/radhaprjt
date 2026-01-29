using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Dtos.Party
{
    public class LocationDto
    {
        public int CityId { get; set; }
        public int StateId { get; set; }
        public int CountryId { get; set; }
    }
}