using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace InventoryManagement.Application.MRS.Command.UpdateMrsEntry
{
    public class UpdateMrsEntryCommand  : IRequest<bool>
    {
        public UpdateMrsEntryDto updateMrsEntry { get; set; } = null!;
    }
}