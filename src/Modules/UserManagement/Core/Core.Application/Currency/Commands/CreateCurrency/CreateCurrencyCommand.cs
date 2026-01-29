using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Application.Common.HttpResponse;
using Core.Domain.Enums;
using MediatR;

namespace Core.Application.Currency.Commands.CreateCurrency
{
    public class CreateCurrencyCommand :IRequest<int>
    { 
        public string? Code { get; set; }
        public string? Name { get; set; }

    }
}