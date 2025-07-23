using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Law4Hire.Scraper
{
    public interface IBruceOpenAIAgent
    {
        Task<List<string>> GetSubCategoriesAsync(string category);
        Task<bool> ValidateSubCategoryAsync(string category, string subCategory);
        Task<List<string>> GetVisaTypesAsync(string category, List<string> subCategories);
    }
}
