using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using MediatR;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Application.FinancialYear.Queries.GetFinancialYear;
using System.Text.Json.Serialization;
using static Core.Domain.Enums.FinancialYearEnum;

namespace Core.Application.FinancialYear.Command.CreateFinancialYear
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