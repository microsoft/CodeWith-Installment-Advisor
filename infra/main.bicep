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

param apiAudience string

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
