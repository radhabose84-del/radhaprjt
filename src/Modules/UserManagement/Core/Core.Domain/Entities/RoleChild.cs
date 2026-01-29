using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Domain.Entities
{
    public class RoleChild
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public UserRole? Role { get; set; }
        public int MenuId { get; set; }
        public Menu? Menu { get; set; }
    }
}