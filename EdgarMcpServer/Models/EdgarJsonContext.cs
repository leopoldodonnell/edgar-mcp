using System.Text.Json;
using System.Text.Json.Serialization;

namespace EdgarMcpServer.Models
{
    /// <summary>
    /// JSON serializer context for the Edgar MCP Server
    /// This enables AOT compilation and trimming-safe serialization
    /// </summary>
    [JsonSerializable(typeof(McpRequest))]
    [JsonSerializable(typeof(McpResponse))]
    [JsonSerializable(typeof(McpError))]
    [JsonSerializable(typeof(ToolsListResponse))]
    [JsonSerializable(typeof(ToolDefinition))]
    [JsonSerializable(typeof(ToolParameters))]
    [JsonSerializable(typeof(ToolProperty))]
    [JsonSerializable(typeof(McpResource))]
    [JsonSerializable(typeof(McpResourcesResponse))]
    [JsonSerializable(typeof(McpInitializeParams))]
    [JsonSerializable(typeof(McpClientCapabilities))]
    [JsonSerializable(typeof(McpRootsCapability))]
    [JsonSerializable(typeof(McpClientInfo))]
    [JsonSerializable(typeof(McpInitializeResult))]
    [JsonSerializable(typeof(McpServerCapabilities))]
    [JsonSerializable(typeof(McpPromptsCapability))]
    [JsonSerializable(typeof(McpResourcesCapability))]
    [JsonSerializable(typeof(McpToolsCapability))]
    [JsonSerializable(typeof(McpServerInfo))]
    [JsonSerializable(typeof(McpInvokeParams))]
    [JsonSerializable(typeof(CompanyInfo))]
    [JsonSerializable(typeof(FilingInfo))]
    [JsonSerializable(typeof(FilingSearchResult))]
    [JsonSerializable(typeof(FinancialStatement))]
    [JsonSerializable(typeof(List<CompanyInfo>))]
    [JsonSerializable(typeof(List<string>))]
    [JsonSerializable(typeof(Dictionary<string, JsonElement>))]
    [JsonSerializable(typeof(Dictionary<string, object>))]
    public partial class EdgarJsonContext : JsonSerializerContext
    {
    }
}
