using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundService.Application.Interfaces
{
    public interface IHangfireDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}