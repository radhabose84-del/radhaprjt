using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserManagement.Domain.Enums
{
    public class FinancialYearEnum
    {
        
         public enum FinancialYearStatus
        {
            Inactive = 0,
            Active  = 1
        }
        public enum FinancialYearDelete
        {
            NotDeleted = 0,
            Deleted = 1
        }

      
    }
}