using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using UserManagement.Domain.Enums;
using MediatR;

namespace UserManagement.Application.FinancialYear.Command.UpdateFinancialYear
{
    public class UpdateFinancialYearCommand : IRequest<int>
    {

         public int Id { get; set; }
        public string? StartYear { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; } 
        public string? FinYearName { get; set; } 
        
        public byte IsActive { get; set; }



       
       
        
    }
}