using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using EdgarMcpServer.Models;
using Microsoft.Extensions.Logging;

namespace EdgarMcpServer.Services
{
    public class EdgarService
    {
        private readonly HttpClient _httpClient;
        private readonly string _userAgent;
        private readonly SemaphoreSlim _requestRateLimiter;
        private readonly ILogger<EdgarService> _logger;

        public EdgarService(HttpClient httpClient, ILogger<EdgarService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            
            // SEC EDGAR requires a user agent with contact information
            // This MUST include your name, organization, and email address per SEC guidelines
            
            // Get user agent from environment variable
            _userAgent = Environment.GetEnvironmentVariable("EDGAR_API_USER_AGENT") ?? string.Empty;
            
            // Fallback to default if not available
            if (string.IsNullOrEmpty(_userAgent))
            {
                _userAgent = "EdgarMcpServer/1.0.0 (AI Assistant; contact-email@example.com)";
            }
            
            _logger.LogInformation($"Using SEC EDGAR API User Agent: {_userAgent}");
            
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
            // SEC API rate limit is 10 requests per second
            // Using a semaphore to ensure we don't exceed this limit
            _requestRateLimiter = new SemaphoreSlim(1, 1);
        }

        /// <summary>
        /// Search for a company by ticker or name
        /// </summary>
        public async Task<List<CompanyInfo>> SearchCompanyAsync(string query)
        {
            try
            {
                // SEC EDGAR API doesn't provide a direct search endpoint
                // We'll use the company tickers endpoint and filter client-side
                // This endpoint is on www.sec.gov, not data.sec.gov
                
                // Apply rate limiting
                await _requestRateLimiter.WaitAsync();
                try
                {
                    var response = await _httpClient.GetAsync("https://www.sec.gov/files/company_tickers.json");
                    response.EnsureSuccessStatusCode();
                    
                    var content = await response.Content.ReadAsStringAsync();
                    var companiesDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);
                    
                    if (companiesDict == null)
                        return new List<CompanyInfo>();
                    
                    var companies = new List<CompanyInfo>();
                    
                    foreach (var entry in companiesDict)
                    {
                        var companyData = entry.Value;
                        var ticker = companyData.GetProperty("ticker").GetString() ?? string.Empty;
                        var name = companyData.GetProperty("title").GetString() ?? string.Empty;
                        var cik = companyData.GetProperty("cik_str").GetInt32().ToString("D10");
                        
                        if (ticker.Contains(query, StringComparison.OrdinalIgnoreCase) || 
                            name.Contains(query, StringComparison.OrdinalIgnoreCase))
                        {
                            companies.Add(new CompanyInfo
                            {
                                Cik = cik,
                                Name = name,
                                Tickers = new List<string> { ticker }
                            });
                        }
                    }
                    
                    return companies;
                }
                finally
                {
                    // Wait 100ms to respect SEC rate limits (10 requests per second)
                    await Task.Delay(100);
                    _requestRateLimiter.Release();
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error searching for company with query: {query}. Error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Get company information by CIK
        /// </summary>
        public async Task<CompanyInfo?> GetCompanyInfoAsync(string cik)
        {
            try
            {
                // Ensure CIK is properly formatted with leading zeros (10 digits)
                cik = cik.PadLeft(10, '0');
                
                // Apply rate limiting
                await _requestRateLimiter.WaitAsync();
                try
                {
                    // Use the submissions endpoint from data.sec.gov
                    var response = await _httpClient.GetAsync($"https://data.sec.gov/submissions/CIK{cik}.json");
                    response.EnsureSuccessStatusCode();
                    
                    var content = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<JsonElement>(content);
                    
                    var companyInfo = new CompanyInfo
                    {
                        Cik = cik,
                        Name = data.GetProperty("name").GetString() ?? string.Empty,
                        Sic = data.TryGetProperty("sic", out var sicElement) ? sicElement.GetString() : null,
                        Tickers = data.TryGetProperty("tickers", out var tickersElement) 
                            ? JsonSerializer.Deserialize<List<string>>(tickersElement.GetRawText()) 
                            : new List<string>()
                    };
                    
                    return companyInfo;
                }
                finally
                {
                    // Wait 100ms to respect SEC rate limits (10 requests per second)
                    await Task.Delay(100);
                    _requestRateLimiter.Release();
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error getting company info for CIK: {cik}. Error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get recent filings for a company by CIK
        /// </summary>
        public async Task<FilingSearchResult?> GetCompanyFilingsAsync(string cik, string? form = null, int limit = 10)
        {
            try
            {
                // Ensure CIK is properly formatted with leading zeros (10 digits)
                cik = cik.PadLeft(10, '0');
                
                // Apply rate limiting
                await _requestRateLimiter.WaitAsync();
                try
                {
                    // Use the submissions endpoint from data.sec.gov
                    var response = await _httpClient.GetAsync($"https://data.sec.gov/submissions/CIK{cik}.json");
                    response.EnsureSuccessStatusCode();
                    
                    var content = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<JsonElement>(content);
                    
                    var companyName = data.GetProperty("name").GetString() ?? string.Empty;
                    var filings = new List<FilingInfo>();
                    
                    // The submissions API provides both recent and historical filings
                    // We'll use the recent filings for this implementation
                    if (data.TryGetProperty("filings", out var filingsElement) && 
                        filingsElement.TryGetProperty("recent", out var recentFilings))
                    {
                        var formArray = recentFilings.GetProperty("form").EnumerateArray().ToList();
                        var filingDateArray = recentFilings.GetProperty("filingDate").EnumerateArray().ToList();
                        var accessionNumberArray = recentFilings.GetProperty("accessionNumber").EnumerateArray().ToList();
                        var primaryDocArray = recentFilings.GetProperty("primaryDocument").EnumerateArray().ToList();
                        
                        // The API also provides reportDate and description fields, which we could use
                        // to enhance our FilingInfo model
                        var reportDateArray = recentFilings.TryGetProperty("reportDate", out var reportDateProp) ?
                            reportDateProp.EnumerateArray().ToList() : null;
                        var descriptionArray = recentFilings.TryGetProperty("description", out var descriptionProp) ?
                            descriptionProp.EnumerateArray().ToList() : null;
                        
                        for (int i = 0; i < formArray.Count && filings.Count < limit; i++)
                        {
                            var currentForm = formArray[i].GetString() ?? string.Empty;
                            
                            // Filter by form if specified
                            if (form != null && !currentForm.Equals(form, StringComparison.OrdinalIgnoreCase))
                                continue;
                            
                            var accessionNumber = accessionNumberArray[i].GetString() ?? string.Empty;
                            var primaryDoc = primaryDocArray[i].GetString() ?? string.Empty;
                            
                            // Create primary document URL
                            var formattedAccessionNumber = accessionNumber.Replace("-", "");
                            var primaryDocUrl = $"https://www.sec.gov/Archives/edgar/data/{cik}/{formattedAccessionNumber}/{primaryDoc}";
                            
                            var filingInfo = new FilingInfo
                            {
                                AccessionNumber = accessionNumber,
                                FilingDate = filingDateArray[i].GetString() ?? string.Empty,
                                Form = currentForm,
                                PrimaryDocument = primaryDoc,
                                PrimaryDocUrl = primaryDocUrl
                            };
                            
                            // Add optional fields if available
                            if (reportDateArray != null && i < reportDateArray.Count)
                            {
                                filingInfo.ReportDate = reportDateArray[i].ValueKind != JsonValueKind.Null ?
                                    reportDateArray[i].GetString() : null;
                            }
                            
                            if (descriptionArray != null && i < descriptionArray.Count)
                            {
                                filingInfo.Description = descriptionArray[i].ValueKind != JsonValueKind.Null ?
                                    descriptionArray[i].GetString() : null;
                            }
                            
                            filings.Add(filingInfo);
                        }
                    }
                    
                    return new FilingSearchResult
                    {
                        Cik = cik,
                        CompanyName = companyName,
                        Filings = filings
                    };
                }
                finally
                {
                    // Wait 100ms to respect SEC rate limits (10 requests per second)
                    await Task.Delay(100);
                    _requestRateLimiter.Release();
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error getting filings for CIK: {cik}. Error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get financial statement data for a company
        /// </summary>
        /// <param name="cik">Company CIK number</param>
        /// <param name="concept">Financial concept (e.g., Assets, Liabilities, NetIncome)</param>
        /// <param name="fiscalPeriod">Fiscal period (Q1, Q2, Q3, FY)</param>
        /// <param name="fiscalYear">Fiscal year</param>
        /// <returns>Financial statement data</returns>
        public async Task<FinancialStatement?> GetFinancialStatementAsync(string cik, string concept, string fiscalPeriod, int fiscalYear)
        {
            _logger.LogDebug($"GetFinancialStatementAsync - CIK: {cik}, Concept: {concept}, Period: {fiscalPeriod}, Year: {fiscalYear}");
            
            // Validate parameters
            if (string.IsNullOrWhiteSpace(cik) || string.IsNullOrWhiteSpace(concept) || string.IsNullOrWhiteSpace(fiscalPeriod))
            {
                _logger.LogWarning("GetFinancialStatementAsync - Invalid parameters");
                return null;
            }
            
            // Normalize period format and ensure CIK is properly formatted
            fiscalPeriod = fiscalPeriod.ToUpper();
            cik = cik.PadLeft(10, '0');
            
            // Create a financial statement object with basic information
            var financialStatement = new FinancialStatement
            {
                Cik = cik,
                CompanyName = "Unknown Company", // Will be updated if we can get company info
                FiscalYear = fiscalYear,
                FiscalPeriod = fiscalPeriod,
                Form = "10-K", // Default form
                FilingDate = DateTime.Now.ToString("yyyy-MM-dd"),
                Data = new Dictionary<string, object>()
            };
            
            try
            {
                // First try to get company info with a short timeout
                try
                {
                    using var companyInfoCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                    var companyInfo = await GetCompanyInfoAsync(cik);
                    if (companyInfo != null)
                    {
                        financialStatement.CompanyName = companyInfo.Name;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"GetFinancialStatementAsync - Error getting company info: {ex.Message}");
                    // Continue with unknown company name
                }
                
                // Now try to get the financial data with a longer timeout
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                
                // For financial data, we'll use the company concept endpoint
                var url = $"https://data.sec.gov/api/xbrl/companyconcept/CIK{cik}/us-gaap/{concept}.json";
                _logger.LogDebug($"GetFinancialStatementAsync - Requesting URL: {url}");
                
                // Create the request
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("User-Agent", _userAgent);
                
                // Use a memory stream to capture partial responses
                using var memoryStream = new MemoryStream();
                
                try
                {
                    // Start the request but don't wait for it to complete
                    using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token);
                    
                    // Check if we got a successful status code
                    if (response.IsSuccessStatusCode)
                    {
                        // Get the response stream
                        using var responseStream = await response.Content.ReadAsStreamAsync(cts.Token);
                        
                        // Create a buffer for reading
                        byte[] buffer = new byte[8192]; // 8KB buffer
                        int bytesRead;
                        int totalBytesRead = 0;
                        
                        // Read the stream in chunks with timeout
                        try
                        {
                            while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length, cts.Token)) > 0)
                            {
                                await memoryStream.WriteAsync(buffer, 0, bytesRead, cts.Token);
                                totalBytesRead += bytesRead;
                                _logger.LogDebug($"GetFinancialStatementAsync - Read {totalBytesRead} bytes so far");
                            }
                            
                            _logger.LogDebug($"GetFinancialStatementAsync - Successfully read all {totalBytesRead} bytes");
                        }
                        catch (OperationCanceledException)
                        {
                            _logger.LogWarning($"GetFinancialStatementAsync - Timeout while reading stream, but we have {memoryStream.Length} bytes");
                            // Continue with partial data
                        }
                        
                        // If we have any data, try to parse it
                        if (memoryStream.Length > 0)
                        {
                            memoryStream.Position = 0; // Reset position to beginning
                            
                            try
                            {
                                // Parse the JSON response (complete or partial)
                                using var jsonDoc = await JsonDocument.ParseAsync(memoryStream, default, cts.Token);
                                var root = jsonDoc.RootElement;
                                
                                // Check if the units property exists
                                if (root.TryGetProperty("units", out var units))
                                {
                                    // Process the units data
                                    bool conceptFound = false;
                                    
                                    foreach (var unitType in units.EnumerateObject())
                                    {
                                        // Add the unit type to our data
                                        financialStatement.Data["unit"] = unitType.Name;
                                        
                                        // Skip if not an array
                                        if (unitType.Value.ValueKind != JsonValueKind.Array)
                                            continue;
                                        
                                        // Look for matching period and year
                                        foreach (var item in unitType.Value.EnumerateArray())
                                        {
                                            // Safely check fiscal year and period with null checks
                                            if (!item.TryGetProperty("fy", out var fy) || 
                                                !item.TryGetProperty("fp", out var fp))
                                                continue;
                                                
                                            // Handle potential null values
                                            int? itemYear = null;
                                            string? itemPeriod = null;
                                            
                                            // Safely get the fiscal year
                                            if (fy.ValueKind == JsonValueKind.Number)
                                                itemYear = fy.GetInt32();
                                                
                                            // Safely get the fiscal period
                                            if (fp.ValueKind == JsonValueKind.String)
                                                itemPeriod = fp.GetString();
                                                
                                            // Skip if not matching our criteria
                                            if (itemYear != fiscalYear || itemPeriod != fiscalPeriod)
                                                continue;
                                            
                                            // Found matching data
                                            if (item.TryGetProperty("val", out var val))
                                            {
                                                // Safely handle different value types
                                                if (val.ValueKind != JsonValueKind.Null)
                                                {
                                                    financialStatement.Data[concept] = val.GetRawText();
                                                    conceptFound = true;
                                                }
                                                else
                                                {
                                                    // Handle null value
                                                    financialStatement.Data[concept] = "null";
                                                    conceptFound = true;
                                                    financialStatement.Data["nullValue"] = true;
                                                }
                                            }
                                            
                                            // Get additional metadata
                                            if (item.TryGetProperty("filed", out var filed) && filed.ValueKind == JsonValueKind.String)
                                            {
                                                string? filedDate = filed.GetString();
                                                if (!string.IsNullOrEmpty(filedDate))
                                                    financialStatement.FilingDate = filedDate;
                                            }
                                                
                                            if (item.TryGetProperty("form", out var form) && form.ValueKind == JsonValueKind.String)
                                            {
                                                string? formType = form.GetString();
                                                if (!string.IsNullOrEmpty(formType))
                                                    financialStatement.Form = formType;
                                            }
                                            
                                            break;
                                        }
                                        
                                        if (conceptFound)
                                            break;
                                    }
                                    
                                    // Add metadata
                                    financialStatement.Data["taxonomy"] = "us-gaap";
                                    financialStatement.Data["conceptFound"] = conceptFound;
                                    
                                    if (!conceptFound)
                                    {
                                        financialStatement.Data["error"] = $"Concept {concept} not found for period {fiscalPeriod} {fiscalYear}";
                                    }
                                }
                                else
                                {
                                    // Units property not found
                                    financialStatement.Data["error"] = "Units property not found in response";
                                    financialStatement.Data["conceptFound"] = false;
                                    financialStatement.Data["partialResponse"] = true;
                                }
                            }
                            catch (JsonException ex)
                            {
                                // JSON parsing error - likely incomplete JSON
                                _logger.LogWarning($"GetFinancialStatementAsync - JSON parsing error with partial data: {ex.Message}");
                                financialStatement.Data["error"] = $"JSON parsing error: {ex.Message}";
                                financialStatement.Data["conceptFound"] = false;
                                financialStatement.Data["partialResponse"] = true;
                                
                                // Return the raw partial data for debugging
                                memoryStream.Position = 0;
                                using var reader = new StreamReader(memoryStream);
                                string partialContent = await reader.ReadToEndAsync();
                                
                                // Only include the first 1000 characters to avoid overwhelming the response
                                if (partialContent.Length > 1000)
                                {
                                    partialContent = partialContent.Substring(0, 1000) + "... (truncated)";
                                }
                                
                                financialStatement.Data["partialData"] = partialContent;
                            }
                        }
                        else
                        {
                            // No data received
                            financialStatement.Data["error"] = "No data received from API";
                            financialStatement.Data["conceptFound"] = false;
                        }
                    }
                    else
                    {
                        // HTTP error
                        financialStatement.Data["error"] = $"HTTP error: {response.StatusCode}";
                        financialStatement.Data["conceptFound"] = false;
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("GetFinancialStatementAsync - Request timed out");
                    financialStatement.Data["error"] = "Request timed out";
                    financialStatement.Data["conceptFound"] = false;
                    financialStatement.Data["timeout"] = true;
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogWarning($"GetFinancialStatementAsync - HTTP request error: {ex.Message}");
                    financialStatement.Data["error"] = $"HTTP request error: {ex.Message}";
                    financialStatement.Data["conceptFound"] = false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetFinancialStatementAsync - Unhandled exception: {ex.Message}");
                financialStatement.Data["error"] = $"Unhandled exception: {ex.Message}";
                financialStatement.Data["conceptFound"] = false;
            }
            
            return financialStatement;
        }

        // No helper methods needed for the simplified implementation
    }
}
