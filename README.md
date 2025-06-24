# Installment Advisor

CodeWith Sample Repo 0 Installment Advisor

## Infrastructure

The infrastructure utilizes the following components:

* Azure AI Services
* Azure API Management: Disclosing the backend APIs as MCP server to the agents
* Azure Container App: hosting the backend APIs
* Azure Cosmos DB: for logging threads for users

Deployment:

```powershell
$resourceGroup = New-AzResourceGroup -Name 'your-name' -Location 'your-location'
New-AzResourceGroupDeployment -Name 'deployment' -ResourceGroup $resourceGroup.ResourceGroupName -TemplateFile infra/main.bicep -TemplateParameterObject @{
    publisherEmail = 'admin@example.com'
    publisherName = 'Admin User'
    installmentApiSpecUrl = '[url-to-demo-api]/openapi/v1.json'
}
```

or 

```azurecli
az deployment group create --resource-group rg-installment --template-file main.bicep --parameters publisherEmail='admin@example.com' publisherName='Admin User' installmentApiSpecUrl='[url-to-demo-api]/openapi/v1.json'
```

### API Management

The Azure API Management instance is used for disclosing backend APIs as MCP tools to the Chat Backend API, it acts as the MCP Server. Details on how this is done are described in [Expose REST API in API Management as MCP Server](https://learn.microsoft.com/en-us/azure/api-management/export-rest-mcp-server). API Management is also where the Auth flow for MCP is implemented.

Viewing the MCP Server configuration of API Management in the portal requires a feature flag [https://portal.azure.com/?Microsoft_Azure_ApiManagement=mcp](https://portal.azure.com/?Microsoft_Azure_ApiManagement=mcp)

Authentication on the MCP Server is done by implementing additional API Management Policies on top of the MCP Server registration.
