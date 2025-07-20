using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Law4Hire.Core.DTOs
{
    public class WorkflowResult
    {
        public string StepName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? DocumentName { get; set; }
        public string? Link { get; set; }
        public TimeSpan? EstimatedTime { get; set; }
        public decimal? EstimatedPrice { get; set; }
    }

}
