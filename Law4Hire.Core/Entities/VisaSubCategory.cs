using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Law4Hire.Core.Entities
{
    public class VisaSubCategory
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = default!;
        public string Status { get; set; } = "Validated";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public VisaCategory? Category { get; set; }
    }

}
