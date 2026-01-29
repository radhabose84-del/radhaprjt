using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Domain.Common;

namespace Core.Domain.Entities
{
    public class UserGroup : BaseEntity
    {
        public int Id { get; set; }
        public string? GroupCode { get; set; }
        public string? GroupName { get; set; }
        public List<User>? Users { get; set; }
       
    }
}