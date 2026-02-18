using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Dtos.Users
{
    public class MenuDto
    {
        public int Id { get; set; }
        public string MenuName { get; set; } = default!;
    }
}