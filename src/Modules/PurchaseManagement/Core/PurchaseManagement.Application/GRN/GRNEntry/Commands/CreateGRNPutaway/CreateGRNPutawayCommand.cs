using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace PurchaseManagement.Application.GRN.GRNEntry.Commands.CreateGRNPutaway
{
    public class CreateGRNPutawayCommand : IRequest<int>
    {
          public List<CreateGRNPutawayDto>? PutawayList { get; set; } 
    }
}