using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Enums;
using UserManagement.Application.FinancialYear.Queries.GetFinancialYear;
using System.Text.Json.Serialization;
using static UserManagement.Domain.Enums.FinancialYearEnum;

namespace UserManagement.Application.FinancialYear.Command.CreateFinancialYear
{
    public class CreateFinancialYearCommand : IRequest<FinancialYearDto>
    {

        public int Id { get; set; }
        public string? StartYear { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; } 
        public string? FinYearName { get; set; }
    }
}