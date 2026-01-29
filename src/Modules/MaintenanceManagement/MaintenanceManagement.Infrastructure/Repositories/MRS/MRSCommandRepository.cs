using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.Interfaces.IMRS;
using MaintenanceManagement.Application.MRS.Command.CreateMRS;
using Dapper;
using MaintenanceManagement.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Sockets;
using System.Globalization;


namespace MaintenanceManagement.Infrastructure.Repositories.MRS
{
    public class MRSCommandRepository : IMRSCommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IDbConnection _dbConnection;

        public MRSCommandRepository(ApplicationDbContext applicationDbContext,IDbConnection dbConnection)
        {
            _applicationDbContext = applicationDbContext;
            _dbConnection = dbConnection;
        }
        public async Task<int> InsertMRSAsync(HeaderRequest headerRequest)
        {
            if (headerRequest == null)
                throw new ArgumentNullException(nameof(headerRequest));
            var newIRNO = await GetNewIRNOAsync(headerRequest.Divcode, headerRequest.IrDate);
            var requestTime = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
            string hostName = Dns.GetHostName();
            string? localIP = Dns.GetHostEntry(hostName)
                     .AddressList
                     .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                     ?.ToString();
              
            
              decimal totalQty = 0;
              if (headerRequest.Details == null)
                {
                    totalQty = 0; // or throw/log as appropriate
                }
                else
                {
                    totalQty = headerRequest.Details.Sum(d => d.QtyReqd ?? 0);
                }
                var totalValue = Math.Round(headerRequest.Details.Sum(d => (d.QtyReqd ?? 0) * d.Rate), 2);
                string formattedValue = totalValue.ToString("0.00");
                var parameters = new DynamicParameters();
                parameters.Add("@DIVCODE", headerRequest.Divcode);
                parameters.Add("@IRNO", newIRNO);
                parameters.Add("@IRDATE", headerRequest.IrDate);
                parameters.Add("@DEPCODE", headerRequest.Depcode);
                parameters.Add("@SUBDEPCODE", headerRequest.SubDepcode);
                parameters.Add("@REFNO", string.IsNullOrWhiteSpace(headerRequest.Refno) ? 0 : headerRequest.Refno);
                parameters.Add("@MAINTENANCETYPE", headerRequest.MaintenanceType);
                parameters.Add("@REMARKS", headerRequest.Remarks);
                parameters.Add("@REQDATE", headerRequest.IrDate);
                parameters.Add("@REQTIME", requestTime);
                parameters.Add("@TOTQTY", totalQty);
                parameters.Add("@TOTVALUE", formattedValue);
                parameters.Add("@CREATEDBY", "IT01"); // Static or from session/user
                parameters.Add("@CHOST_NAME", hostName); // Get from system if needed

                await _dbConnection.ExecuteAsync(
                    "InsertMRSData",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

        // Insert Details (loop over)
        int srno = 1;
        foreach (var detail in headerRequest.Details)
        {
            var detailParams = new DynamicParameters();
            detailParams.Add("@DIVCODE", headerRequest.Divcode);
            detailParams.Add("@IRNO", newIRNO);
            detailParams.Add("@IRDATE", headerRequest.IrDate);
            detailParams.Add("@IRSNO", srno++);
            detailParams.Add("@ITEMCODE", detail.ItemCode);
            detailParams.Add("@MACNO", string.IsNullOrWhiteSpace(detail.Macno) ? 0 : detail.Macno);
            detailParams.Add("@QTYREQD", detail.QtyReqd);
            detailParams.Add("@REQDATE", headerRequest.IrDate);
            detailParams.Add("@CATCODE", detail.CatCode);
            detailParams.Add("@CCCODE", detail.CcCode);
            detailParams.Add("@CURSTK", detail.CurrStk);
            detailParams.Add("@CREATEDBY", "IT01");
            detailParams.Add("@LOCALIPADD", localIP);
            detailParams.Add("@CHOST_NAME", hostName);
            detailParams.Add("@RATE", detail.Rate);
            detailParams.Add("@VALUE",detail.QtyReqd * detail.Rate);


            await _dbConnection.ExecuteAsync(
                "InsertMRSDetail",
                detailParams,
                commandType: CommandType.StoredProcedure
            );
        }

            return newIRNO;
        }
      

        public async Task<int> GetNewIRNOAsync(string divcode, DateTime irDate)
        {
            divcode ??= string.Empty;

            var parameters = new DynamicParameters();
            parameters.Add("@DIVCODE", divcode);
            parameters.Add("@IRDATE", irDate);
            parameters.Add("@NEW_IRNO", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await _dbConnection.ExecuteAsync(
                "GenerateNextIRNO", // Stored Procedure Name
                parameters,
                commandType: CommandType.StoredProcedure
            );

            var newIrno = parameters.Get<int?>("@NEW_IRNO") ?? 0;

            return newIrno;
        }

         

        
       

         

         
    }
}