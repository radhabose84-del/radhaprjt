using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace UserManagement.Application.Currency.Commands.DeleteCurrency
{
    public class DeleteCurrencyCommand : IRequest<int>
    {
         public int Id { get; set; }
    }
}