@description('The name of the API Management service')
param apimServiceName string

@description('The Azure region for resources')
param location string

@description('The URL for the Installment API specification')
param installmentApiSpecUrl string

@description('The API audience for the Entra ID application')
param apiAudience string

resource apimService 'Microsoft.ApiManagement/service@2024-06-01-preview' existing = {
  name: apimServiceName
}

resource cryptoScriptIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' existing = {
  name: '${apimServiceName}-crypto-script-identity'
}

resource importInstallmentApi 'Microsoft.Resources/deploymentScripts@2020-10-01' = {
  name: 'importInstallmentApi'
  dependsOn: [
    cryptoScriptIdentity
  ]
  location: location
  kind: 'AzureCLI'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${cryptoScriptIdentity.id}': {}
    }
  }
  properties: {
    azCliVersion: '2.65.0'
    timeout: 'PT30M'
    retentionInterval: 'P1D'
    environmentVariables: [
      {
        name: 'APIM_NAME'
        value: apimServiceName
      }
      {
        name: 'RESOURCEGROUP_NAME'
        value: resourceGroup().name
      }
      {
        name: 'IMPORT_URL'
        value: installmentApiSpecUrl
      }
    ]
    scriptContent: '''
      az apim api import --resource-group $RESOURCEGROUP_NAME --service-name $APIM_NAME --api-id installmentadvisorapi --path installmentadvisorapi --display-name 'InstallmentAdvisor.DataApi' --specification-format OpenApi --specification-url $IMPORT_URL
    '''
  }
}

resource EntraIDTenantIdNamedValue 'Microsoft.ApiManagement/service/namedValues@2024-06-01-preview' = {
  parent: apimService
  name: 'ApiAudience'
  properties: {
    displayName: 'ApiAudience'
    value: apiAudience
    secret: false
  }
}


resource installmentApiMcp 'Microsoft.ApiManagement/service/apis@2024-06-01-preview' = {
  parent: apimService
  name: 'installmentadvisorapi-mcp'
  dependsOn: [
    importInstallmentApi
  ]
  properties: {
    displayName: 'InstallmentAdvisor.DataApi | v1 MCP'
    apiRevision: '1'
    subscriptionRequired: false
    path: 'mcp'
    protocols: [
      'https'
    ]
    authenticationSettings: {
      oAuth2AuthenticationSettings: []
      openidAuthenticationSettings: []
    }
    subscriptionKeyParameterNames: {
      header: 'Ocp-Apim-Subscription-Key'
      query: 'subscription-key'
    }
    type: 'mcp'
    isCurrent: true
  }
}

resource mcpApiPolicy 'Microsoft.ApiManagement/service/apis/policies@2023-05-01-preview' = {
  parent: installmentApiMcp
  name: 'policy'
  properties: {
    format: 'rawxml'
    value: loadTextContent('installment-api-jwt.policy.xml')
  }
}
