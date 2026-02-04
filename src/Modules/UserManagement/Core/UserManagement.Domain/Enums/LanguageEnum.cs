using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserManagement.Domain.Enums
{
    public class LanguageEnum
    {
        public enum LanguageStatus
        {
            Inactive = 0,
            Active  = 1
        }
        public enum LanguageDelete
        {
            NotDeleted = 0,
            Deleted = 1
        }
    }
}