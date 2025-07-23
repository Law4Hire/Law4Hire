using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Law4Hire.Scraper
{
    public interface IVisaSyncService
    {
        Task SyncCategoriesAndTypesAsync(CancellationToken token);
    }
}
