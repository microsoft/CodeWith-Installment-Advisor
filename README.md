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
}
```


## Chat API authentication
The Chat API uses Entra ID authentication and supports the OAuth 2.0 client credentials flow. Follow the below steps to set this up in your own environment.

### Create the API app registration
1. Create an app registration in Entra ID that represents the Chat API. Take note of the `Application Id`, the `Application ID URI` and the `Tenant Id`. 
2. Add an app role in this app registration, for example `Chat`.

For running locally, add the following user secrets to the AppHost:
```json
{
  ...
  "chatApiClientId" : "<Application Id>",
  "entraIdTenantId" : "<Tenant Id>",
  "entraIdInstance" : "https://login.microsoftonline.com/"
}
```

### Create a client app registration for local testing
1. Create another app registration that represents a client (e.g. the frontend). Take note of the `Application Id`. 
2. Add the API permission for the app role you created in the API app registration; go to API permissions -> add API permission, search for your API app registration name and add application permission.
3. Create a client secret.
4. Request a token by using the client credentials flow, requesting the following scope: `<Application ID URI of the API app registration>/.default`
