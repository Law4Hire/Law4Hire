using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Law4Hire.Core.Entities
{
    public class VisaCategory
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string? VisaTypesJson { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
