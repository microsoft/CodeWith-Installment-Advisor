@description('Name of the API Management service')
param apimServiceName string = 'apim-${uniqueString(resourceGroup().id)}'

@description('SKU name for the API Management service')
param apimSkuName string = 'Basic'

@description('Capacity of the API Management service SKU')
param apimSkuCapacity int = 1

@description('Email address of the API Management publisher')
param publisherEmail string

@description('Name of the API Management publisher')
param publisherName string

@description('URL for the Installment API specification')
param installmentApiSpecUrl string

@description('Audience for the API Management Entra ID application')
param apiAudience string

@description('Name of the AI Foundry service')
param aiFoundryName string = 'foundry-${uniqueString(resourceGroup().id)}'

@description('SKU name for the AI Foundry service')
param aiFoundrySkuName string = 'S0'

@description('Name of the Cosmos DB account')
param cosmosDBAccountName string = 'cosmos-${uniqueString(resourceGroup().id)}'

@description('Name of the Cosmos DB database')
param cosmosDBDatabaseName string = 'ChatHistory'

@description('Name of the Cosmos DB container for chat history')
param cosmosDBContainerName string = 'Messages'

@description('Principal ID of the identity that will access Cosmos DB data')
param cosmosDataPrincipalId string = deployer().objectId

var location = resourceGroup().location

var oauth_scopes = 'openid https://graph.microsoft.com/.default'

module apiManagement 'modules/apiManagement.bicep' = {
  name: 'apiManagement'
  params: {
    apimServiceName: apimServiceName
    location: location
    publisherEmail: publisherEmail
    publisherName: publisherName
    skuName: apimSkuName
    skuCapacity: apimSkuCapacity
  }
}

module apiManagementOAuth 'modules/apim-oauth/apiManagementOAuth.bicep' = {
  name: 'apiManagementOAuth'
  params: {
    apimServiceName: apiManagement.outputs.apimServiceName
    location: location
    entraAppUniqueName: 'mcp-oauth-${resourceGroup().name}'
    entraAppDisplayName: 'MCP OAuth App'
    oauthScopes: oauth_scopes
    entraAppUserAssignedIdentityPrincipleId: apiManagement.outputs.managedIdentityPrincipalId
    entraAppUserAssignedIdentityClientId: apiManagement.outputs.managedIdentityClientId
  }
}

module apiManagementInstallmentAdvisor 'modules/installment-api/installment-api.bicep' = {
  name: 'apiManagementInstallmentAdvisor'
  params: {
    apimServiceName: apiManagement.outputs.apimServiceName
    location: location
    installmentApiSpecUrl: installmentApiSpecUrl
    apiAudience: apiAudience
  }
}
module aiFoundry 'modules/aiFoundry.bicep' = {
  name: 'aiFoundry'
  params: {
    aiFoundryName: aiFoundryName
    location: location
    skuName: aiFoundrySkuName
    aiFoundryProjects: [
      {
        name: 'default-project'
        location: location
      }
    ]
    modelDeployments: [
      {
        name: 'gpt-4.1'
        sku: {
          capacity: 150
          name: 'GlobalStandard'
        }
        model: {
          name: 'gpt-4.1'
          format: 'OpenAI'
        }
      }
      {
        name: 'gpt-4o'
        sku: {
          capacity: 150
          name: 'GlobalStandard'
        }
        model: {
          name: 'gpt-4o'
          format: 'OpenAI'
        }
      }
    ]
  }
}

module cosmosDB 'br/public:avm/res/document-db/database-account:0.15.0' = {
  name: 'cosmosDB'
  params: {
    name: cosmosDBAccountName
    location: location
    networkRestrictions: {
      publicNetworkAccess: 'Enabled'
    }
    sqlDatabases: [
      {
        name: cosmosDBDatabaseName
        containers: [
          {
            indexingPolicy: {
              automatic: true
            }
            name: cosmosDBContainerName
            paths: [
              '/userId'
            ]
          }
        ]
      }
    ]
    dataPlaneRoleDefinitions: [
      {
        assignments: [
          {
            principalId: cosmosDataPrincipalId
          }
        ]
        dataActions: [
          'Microsoft.DocumentDB/databaseAccounts/readMetadata'
          'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/*'
          'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers/items/*'
        ]
        roleName: 'cosmos-sql-role'
      }
    ]
  }
}

output cosmosDBAccountEndpoint string = cosmosDB.outputs.endpoint
output aiFoundryProjectEndpoint string = aiFoundry.outputs.projectEndpoint
output aiFoundryOpenAIEndpoint string = aiFoundry.outputs.openAIEndpoint
