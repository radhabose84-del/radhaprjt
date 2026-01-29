using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Common;

namespace Core.Domain.Entities
{
    public class PasswordComplexityRule :BaseEntity
    {

       public int Id { get; set; }
       
        public string? PwdComplexityRule  { get; set; }

       
    }
}