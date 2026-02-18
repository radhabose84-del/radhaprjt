using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Diagnostics;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.Application.ActivityCheckListMaster.Command.UpdateActivityCheckListMaster
{
    public class UpdateActivityCheckListMasterCommand : IRequest<bool>
    {
       public int Id { get; set; }
       public int ActivityID { get; set; }       
       public string? ActivityChecklist { get; set; }
       public int UnitId { get; set; }
       public byte IsActive { get; set; }
        
    }
}