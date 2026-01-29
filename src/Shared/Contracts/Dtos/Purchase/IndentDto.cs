using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Dtos.Purchase
{
    public class IndentDto
    {
        public int Id { get; set; }
        public string IndentNumber { get; set; }
        public string IndentDate { get; set; }
        public int IndentTypeId { get; set; }
        public int UnitId { get; set; }
        public int DepartmentId { get; set; }
        public string Purpose { get; set; }
        public ICollection<IndentDetailDto> IndentDetails { get; set; }
    }
}