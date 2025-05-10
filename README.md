# SEC EDGAR MCP Server

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

This is a .NET-based MCP (Model Context Protocol) server that enables AI assistants to access the SEC EDGAR database through its API. The server provides a standardized interface for AI assistants to retrieve company information, SEC filings, and financial statement data.

**Note:** This server uses stdio (standard input/output) for communication, making it easier to integrate with AI assistants that spawn child processes.

## Disclaimer

This software is provided "as is", without warranty of any kind, express or implied, including but not limited to the warranties of merchantability, fitness for a particular purpose and noninfringement. In no event shall the authors or copyright holders be liable for any claim, damages or other liability, whether in an action of contract, tort or otherwise, arising from, out of or in connection with the software or the use or other dealings in the software.

This project is not affiliated with, endorsed by, or sponsored by the U.S. Securities and Exchange Commission (SEC). All data retrieved through this tool is subject to the SEC's terms of service and usage policies.

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

## SEC EDGAR API Information

This project uses the SEC EDGAR API v1.0 (as of May 2025). The SEC EDGAR API provides access to company filings and financial data through several endpoints:

- **Company Facts API**: `https://data.sec.gov/api/xbrl/companyfacts/CIK{cik}.json`
- **Company Concept API**: `https://data.sec.gov/api/xbrl/companyconcept/CIK{cik}/us-gaap/{concept}.json`
- **Submissions API**: `https://data.sec.gov/submissions/CIK{cik}.json`

For detailed information about the SEC EDGAR API, please refer to the official documentation:
- [SEC EDGAR API Documentation](https://www.sec.gov/edgar/sec-api-documentation)
- [SEC Data Delivery APIs](https://www.sec.gov/edgar/sec-api-documentation)

When troubleshooting API issues, check the following:
1. Ensure your User-Agent header is properly configured with your contact information
2. Verify you're not exceeding the SEC's rate limits (10 requests per second)
3. Confirm the CIK number is properly formatted with leading zeros (10 digits total)
4. Check the SEC's [EDGAR System Status](https://www.sec.gov/edgar/system-status) for any outages

## Important Notes

- The SEC EDGAR API has rate limits and usage guidelines. Please review the [SEC's API documentation](https://www.sec.gov/edgar/sec-api-documentation) for details.
- This server does not implement authentication. If deploying to production, consider adding appropriate security measures.
- Financial data should be verified with official sources for critical decision-making.

## Releases

The SEC EDGAR MCP Server is built and released automatically using GitHub Actions. Releases are available for:

- Windows (x64)
- macOS (Apple Silicon/ARM64)

Each release includes self-contained executables that don't require .NET to be installed on the target machine.

### Creating a Release

Releases can be triggered in two ways:

1. **Tag-based release**: Push a tag with the format `v*` (e.g., `v1.0.0`) to the repository
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```

2. **Manual release**: Trigger the "Build and Release" workflow manually from the GitHub Actions tab and specify a version number

The GitHub Actions workflow will build the application for all supported platforms, create archives, and publish them as release assets.

## Contributing

Contributions to improve the SEC EDGAR MCP Server are welcome! Here's how you can contribute:

1. **Fork the repository**: Click the Fork button at the top right of the repository page
2. **Clone your fork**: `git clone https://github.com/YOUR_USERNAME/edgar-mcp.git`
3. **Create a branch**: `git checkout -b feature/your-feature-name`
4. **Make your changes**: Implement your feature or bug fix
5. **Test your changes**: Ensure your changes don't break existing functionality
6. **Commit your changes**: `git commit -m "Add your commit message here"`
7. **Push to your fork**: `git push origin feature/your-feature-name`
8. **Submit a pull request**: Go to the original repository and click "New Pull Request"

Please ensure your code follows the existing style and includes appropriate tests. All pull requests should be made against the `main` branch.

### Pull Request Guidelines

- Provide a clear description of the changes in your PR
- Include any relevant issue numbers in the PR description
- Update documentation as needed
- Add or update tests as appropriate
- Ensure all tests pass before submitting

## License

MIT License

Copyright (c) 2025 Leopold O'Donnell

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
