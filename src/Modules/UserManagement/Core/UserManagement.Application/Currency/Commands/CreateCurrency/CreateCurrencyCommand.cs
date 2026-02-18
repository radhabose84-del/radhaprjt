using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using UserManagement.Domain.Enums;
using MediatR;

namespace UserManagement.Application.Currency.Commands.CreateCurrency
{
    public class CreateCurrencyCommand :IRequest<int>
    { 
        public string? Code { get; set; }
        public string? Name { get; set; }

    }
}