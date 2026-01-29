using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundService.Application.Interfaces.IHangfire
{
    public interface IHangfireQuery
    {
        Task<List<int>> GetHangfireJobByTransactionId(int arg);
    }
}