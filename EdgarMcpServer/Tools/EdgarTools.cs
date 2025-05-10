using System.ComponentModel;
using System.Text.Json;
using EdgarMcpServer.Models;
using EdgarMcpServer.Services;
using ModelContextProtocol.Server;

namespace EdgarMcpServer.Tools
{
    /// <summary>
    /// MCP Tools for interacting with the SEC EDGAR API
    /// </summary>
    [McpServerToolType]
    public class EdgarTools
    {
        private readonly EdgarService _edgarService;

        public EdgarTools(EdgarService edgarService)
        {
            _edgarService = edgarService;
        }

        /// <summary>
        /// Search for companies by ticker or name
        /// </summary>
        /// <param name="query">Search query (ticker symbol or company name)</param>
        /// <returns>A list of matching companies</returns>
        [McpServerTool]
        [Description("Search for companies by ticker or name")]
        public async Task<List<CompanyInfo>> SearchCompany(string query)
        {
            return await _edgarService.SearchCompanyAsync(query) ?? new List<CompanyInfo>();
        }

        /// <summary>
        /// Get company details by CIK
        /// </summary>
        /// <param name="cik">Company CIK number</param>
        /// <returns>Company information</returns>
        [McpServerTool]
        [Description("Get company details by CIK")]
        public async Task<CompanyInfo?> GetCompanyInfo(string cik)
        {
            return await _edgarService.GetCompanyInfoAsync(cik);
        }

        /// <summary>
        /// Get SEC filings for a company
        /// </summary>
        /// <param name="cik">Company CIK number</param>
        /// <param name="form">Form type (e.g., 10-K, 10-Q, 8-K)</param>
        /// <param name="limit">Maximum number of filings to return</param>
        /// <returns>A list of filings</returns>
        [McpServerTool]
        [Description("Get SEC filings for a company")]
        public async Task<FilingSearchResult?> GetCompanyFilings(string cik, string? form = null, int? limit = null)
        {
            return await _edgarService.GetCompanyFilingsAsync(cik, form, limit ?? 10);
        }

        /// <summary>
        /// Get financial statement data
        /// </summary>
        /// <param name="cik">Company CIK number</param>
        /// <param name="concept">Financial concept (e.g., Assets, Liabilities, NetIncome)</param>
        /// <param name="year">Fiscal year</param>
        /// <param name="period">Fiscal period (Q1, Q2, Q3, FY)</param>
        /// <returns>Financial statement data</returns>
        [McpServerTool]
        [Description("Get financial statement data")]
        public async Task<FinancialStatement?> GetFinancialStatement(string cik, string concept, string period, int year)
        {
            try
            {
                // Create a cancellation token that will cancel after 15 seconds
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                
                // Create a task that will complete when the service call completes or is canceled
                var task = _edgarService.GetFinancialStatementAsync(cik, concept, period, year);
                
                // Wait for the task to complete or for the timeout to occur
                var completedTask = await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(15), cts.Token));
                
                if (completedTask == task)
                {
                    // The service call completed before the timeout
                    return await task; // Await here to propagate any exceptions
                }
                else
                {
                    // The timeout occurred before the service call completed
                    // Return a fallback response
                    return new FinancialStatement
                    {
                        Cik = cik,
                        CompanyName = "Timeout Occurred",
                        FiscalYear = year,
                        FiscalPeriod = period,
                        Form = "10-K",
                        FilingDate = DateTime.Now.ToString("yyyy-MM-dd"),
                        Data = new Dictionary<string, object>
                        {
                            { "error", "Request timed out after 15 seconds" },
                            { "conceptFound", false }
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                // Return a fallback response in case of any exception
                return new FinancialStatement
                {
                    Cik = cik,
                    CompanyName = "Error Occurred",
                    FiscalYear = year,
                    FiscalPeriod = period,
                    Form = "10-K",
                    FilingDate = DateTime.Now.ToString("yyyy-MM-dd"),
                    Data = new Dictionary<string, object>
                    {
                        { "error", $"An error occurred: {ex.Message}" },
                        { "conceptFound", false }
                    }
                };
            }
        }
    }
}
