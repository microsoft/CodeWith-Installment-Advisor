# Installment Advisor

CodeWith Sample Repo 0 Installment Advisor

## Infrastructure

The infrastructure utilizes the following components:

* Azure AI Services
* Azure API Management: Disclosing the backend APIs as MCP server to the agents
* Azure Container App: hosting the backend APIs

Deployment:

```powershell
$resourceGroup = New-AzResourceGroup -Name 'your-name' -Location 'your-location'
New-AzResourceGroupDeployment -Name 'deployment' -ResourceGroup $resourceGroup.ResourceGroupName -TemplateFile infra/main.bicep -TemplateParameters -TemplateParameterObject @{
    publisherEmail = 'admin@example.com'
    publisherName = 'Admin User'
    cosmosDataPrincipalId = 'Principal ID of your user, so access cosmosDB'
}
```
