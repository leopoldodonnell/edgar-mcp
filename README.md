# SEC EDGAR MCP Server

This is a .NET-based MCP (Model Context Protocol) server that enables AI assistants to access the SEC EDGAR database through its API. The server provides a standardized interface for AI assistants to retrieve company information, SEC filings, and financial statement data.

**Note:** This server uses stdio (standard input/output) for communication, making it easier to integrate with AI assistants that spawn child processes.

## Features

- Search for companies by ticker symbol or name
- Get detailed company information by CIK number
- Retrieve recent SEC filings for a company
- Access financial statement data from company reports
- Generate financial analysis and comparisons

## Prerequisites

- .NET 8.0 SDK or later
- An internet connection to access the SEC EDGAR API

## Configuration

The application uses `appsettings.json` for configuration. The most important setting is the `UserAgent` for SEC EDGAR API requests, which should include your contact information as per SEC guidelines:

```json
{
  "EdgarApi": {
    "UserAgent": "EdgarMcpServer/1.0.0 (Your Name; your-email@example.com)"
  }
}
```

## Building and Running

### Build the Project

```bash
dotnet build -c Release
```

### Run the Server

```bash
dotnet run --project EdgarMcpServer/EdgarMcpServer.csproj
```

The server will read MCP commands from stdin and write responses to stdout. Error messages are written to stderr.

## MCP Protocol Communication

The server implements the Model Context Protocol using stdio communication. Send JSON requests to stdin and receive JSON responses from stdout.

### List Available Resources

Send the following JSON to stdin:

```json
{
  "method": "list_resources"
}
```

Returns a list of available functions that can be invoked.

### Invoke a Function

Send the following JSON to stdin:

```json
{
  "method": "invoke",
  "params": {
    "name": "function-name",
    "parameters": {
      "param1": "value1",
      "param2": "value2"
    }
  }
}
```

## Available Functions

### search-company

Search for a company by ticker symbol or name.

Parameters:
- `query`: The search term (ticker symbol or company name)

### get-company-info

Get detailed information about a company by its CIK number.

Parameters:
- `cik`: The Central Index Key (CIK) number of the company

### get-company-filings

Get recent SEC filings for a company by CIK number.

Parameters:
- `cik`: The Central Index Key (CIK) number of the company
- `form` (optional): Filter by form type (e.g., "10-K", "10-Q")
- `limit` (optional): Maximum number of filings to return (default: 10)

### get-financial-statement

Get financial statement data for a company.

Parameters:
- `cik`: The Central Index Key (CIK) number of the company
- `concept`: The financial concept/metric to retrieve (e.g., "Revenue", "NetIncome")
- `fiscalPeriod`: The fiscal period (e.g., "Q1", "FY")
- `fiscalYear`: The fiscal year (e.g., 2023)

## Example Prompts for Company Research

Here are some example prompts you can use with AI assistants that have access to the Edgar MCP server:

### Basic Company Information

```
What is Apple's CIK number?
Get me basic information about Microsoft Corporation.
Find the ticker symbol for Alphabet Inc.
```

### Financial Statement Analysis

```
What was Apple's net income for fiscal year 2023?
Compare the revenue growth of Microsoft and Apple over the last 3 years.
Calculate the profit margin for Tesla in their most recent 10-K filing.
What is Amazon's debt-to-equity ratio based on their latest financial statements?
```

### SEC Filings Research

```
Get the most recent 10-K filing for Apple Inc.
Find all 8-K filings for Tesla from the past year.
Summarize the risk factors mentioned in Microsoft's latest annual report.
What acquisitions did Meta Platforms report in their recent SEC filings?
```

### Financial Metrics and Ratios

```
Create a spreadsheet with key financial metrics for Apple Inc.
Calculate the return on assets (ROA) for Microsoft based on their latest 10-K.
Generate a consolidated statement of operations for Amazon for the past 3 years.
What is the current ratio for Google based on their latest quarterly report?
```

### Industry Comparisons

```
Compare the profit margins of Apple, Microsoft, and Google.
Which tech company has the highest revenue growth over the past 2 years?
Create a spreadsheet comparing the R&D expenses of major pharmaceutical companies.
How does Tesla's debt-to-equity ratio compare to other automotive manufacturers?
```

## Integrating with AI Assistants

To integrate this MCP server with an AI assistant, add the server to your MCP configuration file. The configuration varies slightly depending on which AI assistant you're using.

### Generic MCP Configuration

```json
{
  "servers": [
    {
      "name": "edgar",
      "command": "/path/to/EdgarMcpServer"
    }
  ]
}
```

### Windsurf (Cascade) Configuration

For Windsurf's Cascade AI assistant, you can configure the Edgar MCP server in your `.cascade/config.json` file:

```json
{
  "mcpServers": {
    "edgar": {
      "command": "/path/to/EdgarMcpServer/bin/Release/net8.0/EdgarMcpServer",
      "env": {
        "EDGAR_API_USER_AGENT": "EdgarMcpServer/1.0.0 (AI Assistant; you@example.com)"
      }
    }
  }
}
```

### Claude Configuration

For Claude AI assistant, you can configure the Edgar MCP server in your Claude settings:

```json
{
  "mcpServers": {
    "edgar": {
      "command": "/path/to/EdgarMcpServer/bin/Release/net8.0/EdgarMcpServer",
      "env": {
        "EDGAR_API_USER_AGENT": "EdgarMcpServer/1.0.0 (AI Assistant; you@example.com)"
      }
    }
  }
}
```

The AI assistant will spawn the server as a child process and communicate with it via stdio.

## Important Notes

- The SEC EDGAR API has rate limits and usage guidelines. Please review the [SEC's API documentation](https://www.sec.gov/edgar/sec-api-documentation) for details.
- This server does not implement authentication. If deploying to production, consider adding appropriate security measures.
- Financial data should be verified with official sources for critical decision-making.

## License

MIT
