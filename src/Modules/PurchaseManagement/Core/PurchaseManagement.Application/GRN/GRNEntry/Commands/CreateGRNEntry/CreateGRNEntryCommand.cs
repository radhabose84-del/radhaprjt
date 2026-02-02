using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.GRN.GRNEntry.Commands.CreateGRNEntry;
using MediatR;

namespace PurchaseManagement.Application.GRN.GRNEntry.Commands
{
    public class CreateGRNEntryCommand : IRequest<int>
    {
        public CreateGRNEntryDto GrnEntryCreate { get; set; } = null!;
    }
}