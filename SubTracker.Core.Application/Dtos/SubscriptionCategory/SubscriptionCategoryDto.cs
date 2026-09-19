using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubTracker.Core.Application.Dtos.SubscriptionCategory
{
    public class SubscriptionCategoryDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public bool IsActive { get; set; }
    }
}
