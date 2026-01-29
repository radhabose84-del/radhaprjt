using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Application.Interfaces.IHangfire;
using Dapper;
using Microsoft.Extensions.DependencyInjection;

namespace BackgroundService.Infrastructure.Repositories.HangFire
{
    public class HangfireQueryRepository : IHangfireQuery
    {
        private readonly IDbConnection _dbConnection;
        public HangfireQueryRepository([FromKeyedServices("Hangfire")] IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }
        public async Task<List<int>> GetHangfireJobByTransactionId(int arg)
        {
            const string sql = @"
            SELECT j.Id
            FROM HangFire.Job j
            LEFT JOIN HangFire.[State] s ON s.JobId = j.Id AND s.Id = j.StateId
            WHERE  TRY_CONVERT(int, JSON_VALUE(j.Arguments, '$[0]')) = @arg
              AND s.Name IN ('Scheduled','Enqueued','Processing');";

            var ids = (await _dbConnection.QueryAsync<int>(sql, new { arg })).ToList();
            // foreach (var id in ids)
            //     BackgroundJob.Delete(id.ToString());

            return ids.ToList();
        }
    }
}