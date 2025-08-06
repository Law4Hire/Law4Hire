using System.Text.Json;
using Law4Hire.Core.Entities;
using Law4Hire.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Law4Hire.Infrastructure.Data.SeedData;

public class DataSeeder
{
    private readonly Law4HireDbContext _context;
    private readonly ILogger<DataSeeder> _logger;
    private Dictionary<string, string[]>? _categoryMappings;

    public DataSeeder(Law4HireDbContext context, ILogger<DataSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAllDataAsync()
    {
        await SeedCountriesAsync();
        await SeedUSStatesAsync();
        await SeedVisaTypesAsync();
        await SeedCategoryClassMappingsAsync();
        // Skip category visa types seeding for now due to schema issues
        // await SeedCategoryVisaTypesAsync();
    }

    public async Task SeedVisaTypesAsync()
    {
        try
        {
            _logger.LogInformation("Starting visa types data seeding...");

            // Check if visa types already exist
            if (await _context.BaseVisaTypes.AnyAsync())
            {
                _logger.LogInformation("Visa types already exist, skipping seeding.");
                return;
            }

            // Read the updated.json file
            var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "updated.json");
            if (!File.Exists(jsonPath))
            {
                _logger.LogWarning("updated.json file not found at: {Path}", jsonPath);
                return;
            }

            var jsonContent = await File.ReadAllTextAsync(jsonPath);
            var visaData = JsonSerializer.Deserialize<VisaTypeData[]>(jsonContent);

            if (visaData == null)
            {
                _logger.LogWarning("Invalid visa types data format in updated.json");
                return;
            }

            var visaTypes = new List<BaseVisaType>();
            foreach (var visa in visaData)
            {
                var visaType = new BaseVisaType
                {
                    Id = Guid.NewGuid(),
                    Code = visa.code,
                    Name = visa.name,
                    Description = visa.description,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Question1 = visa.question1,
                    Question2 = visa.question2,
                    Question3 = visa.question3,
                    ConfidenceScore = 1.0m
                };
                
                visaTypes.Add(visaType);
            }

            await _context.BaseVisaTypes.AddRangeAsync(visaTypes);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully seeded {Count} visa types", visaTypes.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding visa types data");
            throw;
        }
    }

    public async Task SeedCountriesAsync()
    {
        try
        {
            _logger.LogInformation("Starting countries data seeding...");

            // Check if countries already exist
            if (await _context.Countries.AnyAsync())
            {
                _logger.LogInformation("Countries already exist, skipping seeding.");
                return;
            }

            var countries = GetUNRecognizedCountries();
            await _context.Countries.AddRangeAsync(countries);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully seeded {Count} countries", countries.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding countries data");
            throw;
        }
    }

    public async Task SeedUSStatesAsync()
    {
        try
        {
            _logger.LogInformation("Starting US states data seeding...");

            // Check if states already exist
            if (await _context.USStates.AnyAsync())
            {
                _logger.LogInformation("US states already exist, skipping seeding.");
                return;
            }

            var states = GetUSStatesAndTerritories();
            await _context.USStates.AddRangeAsync(states);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully seeded {Count} US states and territories", states.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding US states data");
            throw;
        }
    }

    private List<Country> GetUNRecognizedCountries()
    {
        var countries = new List<Country>();
        var countryData = new (string Name, string Code3, string Code2)[]
        {
            ("Afghanistan", "AFG", "AF"),
            ("Albania", "ALB", "AL"),
            ("Algeria", "DZA", "DZ"),
            ("Andorra", "AND", "AD"),
            ("Angola", "AGO", "AO"),
            ("Antigua and Barbuda", "ATG", "AG"),
            ("Argentina", "ARG", "AR"),
            ("Armenia", "ARM", "AM"),
            ("Australia", "AUS", "AU"),
            ("Austria", "AUT", "AT"),
            ("Azerbaijan", "AZE", "AZ"),
            ("Bahamas", "BHS", "BS"),
            ("Bahrain", "BHR", "BH"),
            ("Bangladesh", "BGD", "BD"),
            ("Barbados", "BRB", "BB"),
            ("Belarus", "BLR", "BY"),
            ("Belgium", "BEL", "BE"),
            ("Belize", "BLZ", "BZ"),
            ("Benin", "BEN", "BJ"),
            ("Bhutan", "BTN", "BT"),
            ("Bolivia", "BOL", "BO"),
            ("Bosnia and Herzegovina", "BIH", "BA"),
            ("Botswana", "BWA", "BW"),
            ("Brazil", "BRA", "BR"),
            ("Brunei", "BRN", "BN"),
            ("Bulgaria", "BGR", "BG"),
            ("Burkina Faso", "BFA", "BF"),
            ("Burundi", "BDI", "BI"),
            ("Cabo Verde", "CPV", "CV"),
            ("Cambodia", "KHM", "KH"),
            ("Cameroon", "CMR", "CM"),
            ("Canada", "CAN", "CA"),
            ("Central African Republic", "CAF", "CF"),
            ("Chad", "TCD", "TD"),
            ("Chile", "CHL", "CL"),
            ("China", "CHN", "CN"),
            ("Colombia", "COL", "CO"),
            ("Comoros", "COM", "KM"),
            ("Congo", "COG", "CG"),
            ("Costa Rica", "CRI", "CR"),
            ("Croatia", "HRV", "HR"),
            ("Cuba", "CUB", "CU"),
            ("Cyprus", "CYP", "CY"),
            ("Czech Republic", "CZE", "CZ"),
            ("Democratic Republic of the Congo", "COD", "CD"),
            ("Denmark", "DNK", "DK"),
            ("Djibouti", "DJI", "DJ"),
            ("Dominica", "DMA", "DM"),
            ("Dominican Republic", "DOM", "DO"),
            ("Ecuador", "ECU", "EC"),
            ("Egypt", "EGY", "EG"),
            ("El Salvador", "SLV", "SV"),
            ("Equatorial Guinea", "GNQ", "GQ"),
            ("Eritrea", "ERI", "ER"),
            ("Estonia", "EST", "EE"),
            ("Eswatini", "SWZ", "SZ"),
            ("Ethiopia", "ETH", "ET"),
            ("Fiji", "FJI", "FJ"),
            ("Finland", "FIN", "FI"),
            ("France", "FRA", "FR"),
            ("Gabon", "GAB", "GA"),
            ("Gambia", "GMB", "GM"),
            ("Georgia", "GEO", "GE"),
            ("Germany", "DEU", "DE"),
            ("Ghana", "GHA", "GH"),
            ("Greece", "GRC", "GR"),
            ("Grenada", "GRD", "GD"),
            ("Guatemala", "GTM", "GT"),
            ("Guinea", "GIN", "GN"),
            ("Guinea-Bissau", "GNB", "GW"),
            ("Guyana", "GUY", "GY"),
            ("Haiti", "HTI", "HT"),
            ("Honduras", "HND", "HN"),
            ("Hungary", "HUN", "HU"),
            ("Iceland", "ISL", "IS"),
            ("India", "IND", "IN"),
            ("Indonesia", "IDN", "ID"),
            ("Iran", "IRN", "IR"),
            ("Iraq", "IRQ", "IQ"),
            ("Ireland", "IRL", "IE"),
            ("Israel", "ISR", "IL"),
            ("Italy", "ITA", "IT"),
            ("Jamaica", "JAM", "JM"),
            ("Japan", "JPN", "JP"),
            ("Jordan", "JOR", "JO"),
            ("Kazakhstan", "KAZ", "KZ"),
            ("Kenya", "KEN", "KE"),
            ("Kiribati", "KIR", "KI"),
            ("Kuwait", "KWT", "KW"),
            ("Kyrgyzstan", "KGZ", "KG"),
            ("Laos", "LAO", "LA"),
            ("Latvia", "LVA", "LV"),
            ("Lebanon", "LBN", "LB"),
            ("Lesotho", "LSO", "LS"),
            ("Liberia", "LBR", "LR"),
            ("Libya", "LBY", "LY"),
            ("Liechtenstein", "LIE", "LI"),
            ("Lithuania", "LTU", "LT"),
            ("Luxembourg", "LUX", "LU"),
            ("Madagascar", "MDG", "MG"),
            ("Malawi", "MWI", "MW"),
            ("Malaysia", "MYS", "MY"),
            ("Maldives", "MDV", "MV"),
            ("Mali", "MLI", "ML"),
            ("Malta", "MLT", "MT"),
            ("Marshall Islands", "MHL", "MH"),
            ("Mauritania", "MRT", "MR"),
            ("Mauritius", "MUS", "MU"),
            ("Mexico", "MEX", "MX"),
            ("Micronesia", "FSM", "FM"),
            ("Moldova", "MDA", "MD"),
            ("Monaco", "MCO", "MC"),
            ("Mongolia", "MNG", "MN"),
            ("Montenegro", "MNE", "ME"),
            ("Morocco", "MAR", "MA"),
            ("Mozambique", "MOZ", "MZ"),
            ("Myanmar", "MMR", "MM"),
            ("Namibia", "NAM", "NA"),
            ("Nauru", "NRU", "NR"),
            ("Nepal", "NPL", "NP"),
            ("Netherlands", "NLD", "NL"),
            ("New Zealand", "NZL", "NZ"),
            ("Nicaragua", "NIC", "NI"),
            ("Niger", "NER", "NE"),
            ("Nigeria", "NGA", "NG"),
            ("North Korea", "PRK", "KP"),
            ("North Macedonia", "MKD", "MK"),
            ("Norway", "NOR", "NO"),
            ("Oman", "OMN", "OM"),
            ("Pakistan", "PAK", "PK"),
            ("Palau", "PLW", "PW"),
            ("Panama", "PAN", "PA"),
            ("Papua New Guinea", "PNG", "PG"),
            ("Paraguay", "PRY", "PY"),
            ("Peru", "PER", "PE"),
            ("Philippines", "PHL", "PH"),
            ("Poland", "POL", "PL"),
            ("Portugal", "PRT", "PT"),
            ("Qatar", "QAT", "QA"),
            ("Romania", "ROU", "RO"),
            ("Russia", "RUS", "RU"),
            ("Rwanda", "RWA", "RW"),
            ("Saint Kitts and Nevis", "KNA", "KN"),
            ("Saint Lucia", "LCA", "LC"),
            ("Saint Vincent and the Grenadines", "VCT", "VC"),
            ("Samoa", "WSM", "WS"),
            ("San Marino", "SMR", "SM"),
            ("Sao Tome and Principe", "STP", "ST"),
            ("Saudi Arabia", "SAU", "SA"),
            ("Senegal", "SEN", "SN"),
            ("Serbia", "SRB", "RS"),
            ("Seychelles", "SYC", "SC"),
            ("Sierra Leone", "SLE", "SL"),
            ("Singapore", "SGP", "SG"),
            ("Slovakia", "SVK", "SK"),
            ("Slovenia", "SVN", "SI"),
            ("Solomon Islands", "SLB", "SB"),
            ("Somalia", "SOM", "SO"),
            ("South Africa", "ZAF", "ZA"),
            ("South Korea", "KOR", "KR"),
            ("South Sudan", "SSD", "SS"),
            ("Spain", "ESP", "ES"),
            ("Sri Lanka", "LKA", "LK"),
            ("Sudan", "SDN", "SD"),
            ("Suriname", "SUR", "SR"),
            ("Sweden", "SWE", "SE"),
            ("Switzerland", "CHE", "CH"),
            ("Syria", "SYR", "SY"),
            ("Tajikistan", "TJK", "TJ"),
            ("Tanzania", "TZA", "TZ"),
            ("Thailand", "THA", "TH"),
            ("Timor-Leste", "TLS", "TL"),
            ("Togo", "TGO", "TG"),
            ("Tonga", "TON", "TO"),
            ("Trinidad and Tobago", "TTO", "TT"),
            ("Tunisia", "TUN", "TN"),
            ("Turkey", "TUR", "TR"),
            ("Turkmenistan", "TKM", "TM"),
            ("Tuvalu", "TUV", "TV"),
            ("Uganda", "UGA", "UG"),
            ("Ukraine", "UKR", "UA"),
            ("United Arab Emirates", "ARE", "AE"),
            ("United Kingdom", "GBR", "GB"),
            ("United States", "USA", "US"),
            ("Uruguay", "URY", "UY"),
            ("Uzbekistan", "UZB", "UZ"),
            ("Vanuatu", "VUT", "VU"),
            ("Vatican City", "VAT", "VA"),
            ("Venezuela", "VEN", "VE"),
            ("Vietnam", "VNM", "VN"),
            ("Yemen", "YEM", "YE"),
            ("Zambia", "ZMB", "ZM"),
            ("Zimbabwe", "ZWE", "ZW")
        };

        for (int i = 0; i < countryData.Length; i++)
        {
            var (name, code3, code2) = countryData[i];
            countries.Add(new Country
            {
                Id = Guid.NewGuid(),
                Name = name,
                CountryCode = code3,
                CountryCode2 = code2,
                IsUNRecognized = true,
                IsActive = true,
                SortOrder = i + 1,
                CreatedAt = DateTime.UtcNow
            });
        }

        return countries.OrderBy(c => c.Name).ToList();
    }

    private List<USState> GetUSStatesAndTerritories()
    {
        var states = new List<USState>();
        var stateData = new (string Name, string Code, bool IsState)[]
        {
            ("Alabama", "AL", true),
            ("Alaska", "AK", true),
            ("Arizona", "AZ", true),
            ("Arkansas", "AR", true),
            ("California", "CA", true),
            ("Colorado", "CO", true),
            ("Connecticut", "CT", true),
            ("Delaware", "DE", true),
            ("Florida", "FL", true),
            ("Georgia", "GA", true),
            ("Hawaii", "HI", true),
            ("Idaho", "ID", true),
            ("Illinois", "IL", true),
            ("Indiana", "IN", true),
            ("Iowa", "IA", true),
            ("Kansas", "KS", true),
            ("Kentucky", "KY", true),
            ("Louisiana", "LA", true),
            ("Maine", "ME", true),
            ("Maryland", "MD", true),
            ("Massachusetts", "MA", true),
            ("Michigan", "MI", true),
            ("Minnesota", "MN", true),
            ("Mississippi", "MS", true),
            ("Missouri", "MO", true),
            ("Montana", "MT", true),
            ("Nebraska", "NE", true),
            ("Nevada", "NV", true),
            ("New Hampshire", "NH", true),
            ("New Jersey", "NJ", true),
            ("New Mexico", "NM", true),
            ("New York", "NY", true),
            ("North Carolina", "NC", true),
            ("North Dakota", "ND", true),
            ("Ohio", "OH", true),
            ("Oklahoma", "OK", true),
            ("Oregon", "OR", true),
            ("Pennsylvania", "PA", true),
            ("Rhode Island", "RI", true),
            ("South Carolina", "SC", true),
            ("South Dakota", "SD", true),
            ("Tennessee", "TN", true),
            ("Texas", "TX", true),
            ("Utah", "UT", true),
            ("Vermont", "VT", true),
            ("Virginia", "VA", true),
            ("Washington", "WA", true),
            ("West Virginia", "WV", true),
            ("Wisconsin", "WI", true),
            ("Wyoming", "WY", true),
            ("District of Columbia", "DC", false),
            ("American Samoa", "AS", false),
            ("Guam", "GU", false),
            ("Northern Mariana Islands", "MP", false),
            ("Puerto Rico", "PR", false),
            ("U.S. Virgin Islands", "VI", false)
        };

        for (int i = 0; i < stateData.Length; i++)
        {
            var (name, code, isState) = stateData[i];
            states.Add(new USState
            {
                Id = Guid.NewGuid(),
                Name = name,
                StateCode = code,
                IsState = isState,
                IsActive = true,
                SortOrder = i + 1,
                CreatedAt = DateTime.UtcNow
            });
        }

        return states.OrderBy(s => s.Name).ToList();
    }

    public async Task SeedCategoryClassMappingsAsync()
    {
        try
        {
            _logger.LogInformation("Starting category class mappings data seeding...");

            // Read the CategoryClass.json file
            var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "CategoryClass.json");
            if (!File.Exists(jsonPath))
            {
                _logger.LogWarning("CategoryClass.json file not found at: {Path}", jsonPath);
                return;
            }

            var jsonContent = await File.ReadAllTextAsync(jsonPath);
            var categoryMappings = JsonSerializer.Deserialize<Dictionary<string, string[]>>(jsonContent);

            if (categoryMappings == null)
            {
                _logger.LogWarning("Invalid category class data format in CategoryClass.json");
                return;
            }

            _logger.LogInformation("Successfully loaded {Count} category mappings from CategoryClass.json", categoryMappings.Count);

            // Store the mappings in memory for use by the category visa type seeding
            _categoryMappings = categoryMappings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding category class mappings data");
            throw;
        }
    }

    public async Task SeedCategoryVisaTypesAsync()
    {
        try
        {
            _logger.LogInformation("Starting category visa types data seeding...");

            // Check if category visa types already exist
            if (await _context.CategoryVisaTypes.AnyAsync())
            {
                _logger.LogInformation("Category visa types already exist, skipping seeding.");
                return;
            }

            // Get all visa categories and visa types
            var categories = await _context.VisaCategories.ToListAsync();
            var visaTypes = await _context.BaseVisaTypes.ToListAsync();

            if (!categories.Any() || !visaTypes.Any())
            {
                _logger.LogWarning("Cannot seed category visa types - missing categories or visa types");
                return;
            }

            var categoryVisaTypes = new List<CategoryVisaType>();

            // Map visa types to categories based on visa name patterns
            foreach (var visaType in visaTypes)
            {
                var categoryMappings = GetCategoryMappingsForVisa(visaType.Code);
                
                foreach (var categoryName in categoryMappings)
                {
                    var category = categories.FirstOrDefault(c => c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
                    if (category != null)
                    {
                        categoryVisaTypes.Add(new CategoryVisaType
                        {
                            Id = Guid.NewGuid(),
                            CategoryId = category.Id,
                            VisaTypeId = visaType.Id,
                            CreatedAt = DateTime.UtcNow,
                            IsActive = true
                        });
                    }
                }
            }

            await _context.CategoryVisaTypes.AddRangeAsync(categoryVisaTypes);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully seeded {Count} category visa type mappings", categoryVisaTypes.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding category visa types data");
            throw;
        }
    }

    private List<string> GetCategoryMappingsForVisa(string visaCode)
    {
        var mappings = new List<string>();

        if (_categoryMappings == null)
        {
            // Fallback to default mapping if CategoryClass.json wasn't loaded
            mappings.Add("Visit");
            return mappings;
        }

        // Extract the prefix from the visa code (e.g., "H-1B" -> "H", "EB-1A" -> "EB")
        var prefix = ExtractVisaPrefix(visaCode);
        
        if (_categoryMappings.TryGetValue(prefix, out var categories))
        {
            mappings.AddRange(categories);
        }
        else
        {
            // Check for exact match
            if (_categoryMappings.TryGetValue(visaCode, out var exactCategories))
            {
                mappings.AddRange(exactCategories);
            }
            else
            {
                // Default fallback
                mappings.Add("Other");
            }
        }

        return mappings;
    }

    private string ExtractVisaPrefix(string visaCode)
    {
        if (string.IsNullOrEmpty(visaCode))
            return "Other";

        // Handle special cases
        if (visaCode.StartsWith("IR-") || visaCode.StartsWith("CR-"))
            return visaCode.Substring(0, 2);
        if (visaCode.StartsWith("EB-"))
            return "EB";
        if (visaCode.StartsWith("NATO-"))
            return "NATO";

        // Extract first letter(s) before dash or number
        var match = System.Text.RegularExpressions.Regex.Match(visaCode, @"^([A-Z]+)");
        return match.Success ? match.Groups[1].Value : "Other";
    }
}

// Helper classes for JSON deserialization
public class VisaTypesData
{
    public VisaTypeData[] Visas { get; set; } = Array.Empty<VisaTypeData>();
}

public class VisaTypeData
{
    public string code { get; set; } = string.Empty;
    public string name { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public string? question1 { get; set; }
    public string? question2 { get; set; }
    public string? question3 { get; set; }
}