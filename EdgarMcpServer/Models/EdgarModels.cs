using System.Text.Json;
using System.Text.Json.Serialization;

namespace EdgarMcpServer.Models
{
    // MCP Protocol Models
    public class McpRequest
    {
        [JsonPropertyName("jsonrpc")]
        public string JsonRpc { get; set; } = "2.0";
        
        [JsonPropertyName("id")]
        public int? Id { get; set; }
        
        [JsonPropertyName("method")]
        public string Method { get; set; } = string.Empty;
        
        [JsonPropertyName("params")]
        public JsonElement Params { get; set; }
    }

    public class McpResponse
    {
        [JsonPropertyName("jsonrpc")]
        public string JsonRpc { get; set; } = "2.0";
        
        [JsonPropertyName("id")]
        public int? Id { get; set; }
        
        [JsonPropertyName("result")]
        public object? Result { get; set; }
        
        [JsonPropertyName("error")]
        public McpError? Error { get; set; }
    }
    
    public class McpError
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }
        
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
        
        [JsonPropertyName("data")]
        public object? Data { get; set; }
    }
    
    /// <summary>
    /// Response model for tools/list endpoint
    /// </summary>
    // The tools/list response should be a direct array of tool definitions
    // This is what Windsurf expects - no wrapper object with a 'tools' property
    public class ToolsListResponse : List<ToolDefinition>
    {
        public ToolsListResponse() : base() { }
        
        public ToolsListResponse(IEnumerable<ToolDefinition> tools) : base(tools) { }
    }

    /// <summary>
    /// Definition of a tool for the tools/list endpoint
    /// </summary>
    public class ToolDefinition
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("parameters")]
        public ToolParameters Parameters { get; set; } = new ToolParameters();
    }

    /// <summary>
    /// Parameters for a tool definition
    /// </summary>
    public class ToolParameters
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "object";

        [JsonPropertyName("properties")]
        public Dictionary<string, ToolProperty> Properties { get; set; } = new Dictionary<string, ToolProperty>();

        [JsonPropertyName("required")]
        public List<string> Required { get; set; } = new List<string>();
    }

    /// <summary>
    /// Property definition for a tool parameter
    /// </summary>
    public class ToolProperty
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }

    public class McpResource
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
        
        [JsonPropertyName("uri")]
        public string Uri { get; set; } = string.Empty;
    }

    public class McpResourcesResponse
    {
        [JsonPropertyName("resources")]
        public List<McpResource> Resources { get; set; } = new List<McpResource>();
        
        [JsonPropertyName("nextCursor")]
        public string? NextCursor { get; set; }
    }
    
    // MCP Initialization Models
    public class McpInitializeParams
    {
        [JsonPropertyName("protocolVersion")]
        public string ProtocolVersion { get; set; } = "2024-11-05";
        
        [JsonPropertyName("capabilities")]
        public McpClientCapabilities Capabilities { get; set; } = new McpClientCapabilities();
        
        [JsonPropertyName("clientInfo")]
        public McpClientInfo ClientInfo { get; set; } = new McpClientInfo();
    }
    
    public class McpClientCapabilities
    {
        [JsonPropertyName("roots")]
        public McpRootsCapability? Roots { get; set; }
        
        [JsonPropertyName("sampling")]
        public object? Sampling { get; set; }
        
        [JsonPropertyName("experimental")]
        public object? Experimental { get; set; }
    }
    
    public class McpRootsCapability
    {
        [JsonPropertyName("listChanged")]
        public bool ListChanged { get; set; }
    }
    
    public class McpClientInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;
    }
    
    public class McpInitializeResult
    {
        [JsonPropertyName("protocolVersion")]
        public string ProtocolVersion { get; set; } = "2024-11-05";
        
        [JsonPropertyName("capabilities")]
        public McpServerCapabilities Capabilities { get; set; } = new McpServerCapabilities();
        
        [JsonPropertyName("serverInfo")]
        public McpServerInfo ServerInfo { get; set; } = new McpServerInfo();
        
        [JsonPropertyName("instructions")]
        public string? Instructions { get; set; }
    }
    
    public class McpServerCapabilities
    {
        [JsonPropertyName("logging")]
        public object? Logging { get; set; } = new {};
        
        [JsonPropertyName("prompts")]
        public McpPromptsCapability? Prompts { get; set; }
        
        [JsonPropertyName("resources")]
        public McpResourcesCapability? Resources { get; set; } = new McpResourcesCapability();
        
        [JsonPropertyName("tools")]
        public McpToolsCapability? Tools { get; set; } = new McpToolsCapability { ListChanged = true };
        
        [JsonPropertyName("experimental")]
        public object? Experimental { get; set; }
    }
    
    public class McpPromptsCapability
    {
        [JsonPropertyName("listChanged")]
        public bool ListChanged { get; set; }
    }
    
    public class McpResourcesCapability
    {
        [JsonPropertyName("subscribe")]
        public bool Subscribe { get; set; }
        
        [JsonPropertyName("listChanged")]
        public bool ListChanged { get; set; }
    }
    
    public class McpToolsCapability
    {
        [JsonPropertyName("listChanged")]
        public bool ListChanged { get; set; }
    }
    
    public class McpServerInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;
    }

    public class McpInvokeParams
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("parameters")]
        public JsonElement Parameters { get; set; }
    }

    // SEC EDGAR Models
    public class CompanyInfo
    {
        [JsonPropertyName("cik")]
        public string Cik { get; set; } = string.Empty;
        
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("sic")]
        public string? Sic { get; set; }
        
        [JsonPropertyName("sicDescription")]
        public string? SicDescription { get; set; }
        
        [JsonPropertyName("tickers")]
        public List<string>? Tickers { get; set; }
    }

    public class FilingInfo
    {
        [JsonPropertyName("accessionNumber")]
        public string AccessionNumber { get; set; } = string.Empty;
        
        [JsonPropertyName("filingDate")]
        public string FilingDate { get; set; } = string.Empty;
        
        [JsonPropertyName("reportDate")]
        public string? ReportDate { get; set; }
        
        [JsonPropertyName("form")]
        public string Form { get; set; } = string.Empty;
        
        [JsonPropertyName("primaryDocument")]
        public string PrimaryDocument { get; set; } = string.Empty;
        
        [JsonPropertyName("primaryDocUrl")]
        public string PrimaryDocUrl { get; set; } = string.Empty;
        
        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }

    public class FilingSearchResult
    {
        [JsonPropertyName("cik")]
        public string Cik { get; set; } = string.Empty;
        
        [JsonPropertyName("companyName")]
        public string CompanyName { get; set; } = string.Empty;
        
        [JsonPropertyName("filings")]
        public List<FilingInfo> Filings { get; set; } = new List<FilingInfo>();
    }

    public class FinancialStatement
    {
        [JsonPropertyName("cik")]
        public string Cik { get; set; } = string.Empty;
        
        [JsonPropertyName("companyName")]
        public string CompanyName { get; set; } = string.Empty;
        
        [JsonPropertyName("form")]
        public string Form { get; set; } = string.Empty;
        
        [JsonPropertyName("filingDate")]
        public string FilingDate { get; set; } = string.Empty;
        
        [JsonPropertyName("fiscalYear")]
        public int FiscalYear { get; set; }
        
        [JsonPropertyName("fiscalPeriod")]
        public string FiscalPeriod { get; set; } = string.Empty;
        
        [JsonPropertyName("data")]
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
    }
}
