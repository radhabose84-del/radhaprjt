using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using Core.Domain.Enums;
using MediatR;

namespace Core.Application.Currency.Commands.UpdateCurrency
{
    public class UpdateCurrencyCommand  : IRequest<int>
    {
        public int Id { get; set; }
       // public string? Code { get; set; }
        public string? Name { get; set; }
        public byte IsActive { get; set; }
       
    }
}