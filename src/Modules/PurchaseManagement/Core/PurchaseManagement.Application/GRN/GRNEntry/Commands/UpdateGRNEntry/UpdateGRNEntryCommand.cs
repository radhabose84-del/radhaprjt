using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace PurchaseManagement.Application.GRN.GRNEntry.Commands.UpdateGRNEntry
{
    public class UpdateGRNEntryCommand : IRequest<bool>
    {
        public UpdateGRNEntryDto GrnEntryUpdate { get; set; } = null!;
    }
}