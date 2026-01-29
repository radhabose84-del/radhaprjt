using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.Reports.MRS
{
    public class MRSReportDto
    {           
        public string? IrNo { get; set; }                 
        public DateTime IrDate { get; set; }            
        public string? WoNo { get; set; }                
        public string? Remarks { get; set; }              
        public string? Type { get; set; }    
        public string? DEPNAME { get; set; }       
        public string? SubDept { get; set; }  
        public int IrSno { get; set; }                  
        public string? ItemCode { get; set; }            
        public string? ItemName { get; set; }             
        public string? MachineCode { get; set; }    
        public string? MachineName { get; set; }           
        public decimal Qty { get; set; }                 
        public string? CATDESC { get; set; }  
        public string? Consumption { get; set; }      
        public decimal Rate { get; set; }              
        public decimal Value { get; set; }
    }
}