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

@description('Name of the AI Search service')
param aiSearchName string = 'search-${uniqueString(resourceGroup().id)}'

@description('Principal ID of the identity that will access Cosmos DB data')
param principalId string = deployer().objectId

var location = resourceGroup().location

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
      {
        name: 'text-embedding-ada-002'
        sku: {
          capacity: 100
          name: 'GlobalStandard'
        }
        model: {
          name: 'text-embedding-ada-002'
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
            principalId: principalId
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

module aiSearch 'br/public:avm/res/search/search-service:0.10.0' = {
  name: 'aiSearch'
  params: {
    name: aiSearchName
    sku: 'basic'
    location: resourceGroup().location
    replicaCount: 1
    partitionCount: 1
  }
}

output cosmosDBAccountEndpoint string = cosmosDB.outputs.endpoint
output aiFoundryProjectEndpoint string = aiFoundry.outputs.projectEndpoint
output aiFoundryOpenAIEndpoint string = aiFoundry.outputs.openAIEndpoint
