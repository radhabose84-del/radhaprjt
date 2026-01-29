using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.MRS.Command.CreateMRS;

namespace MaintenanceManagement.Application.Common.Interfaces.IMRS
{
    public interface IMRSCommandRepository
    {
         Task<int> InsertMRSAsync(HeaderRequest headerRequest);
    }
}