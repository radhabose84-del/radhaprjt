using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Application.Common.HttpResponse;
using UserManagement.Application.Currency.Queries.GetCurrency;
using MediatR;

namespace UserManagement.Application.Currency.Queries.GetCurrencyById
{
    public class GetCurrencyByIdQuery :IRequest<CurrencyDto>
    {
        public int CurrencyId { get; set; }
    }
}