using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace PurchaseManagement.Application.GRN.GateEntry.Commands.CreateGateEntry
{
    public class CreateGateEntryCommand : IRequest<int>
    {
        public CreateGateEntryDto GateEntryDetails { get; set; } = null!;
    }
}