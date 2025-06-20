param aiFoundryName string
param location string
param skuName string = 'S0'
param aiFoundryProjects foundryProjectType
param modelDeployments modelDeploymentType

type foundryProjectType = {
  name: string
  location: string
}[]

type modelDeploymentType = {
  name: string
  sku: {
    capacity: int
    name: string
  }
  model: {
      name: string
      format: string
  }
}[]

resource aiFoundry 'Microsoft.CognitiveServices/accounts@2025-04-01-preview' = {
  name: aiFoundryName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  sku: {
    name: skuName
  }
  kind: 'AIServices'
  properties: {
    allowProjectManagement: true 
    customSubDomainName: aiFoundryName
  }
}

@batchSize(1)
resource aiProject 'Microsoft.CognitiveServices/accounts/projects@2025-04-01-preview' = [for project in aiFoundryProjects: {
  name: project.name
  parent: aiFoundry
  location: project.location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {}
}]

@batchSize(1)
resource modelDeployment 'Microsoft.CognitiveServices/accounts/deployments@2025-04-01-preview'=  [for deployment in modelDeployments: {
  parent: aiFoundry
  name: deployment.name
  sku : {
    capacity: deployment.sku.capacity
    name: deployment.sku.name
  }
  properties: {
    model:{
      name: deployment.model.name
      format: deployment.model.format
    }
  }
}]

output projectEndpoint string = 'https://${aiFoundry.name}.services.ai.azure.com/api/projects/'
output openAIEndpoint string = 'https://${aiFoundry.name}.openai.azure.com/'
