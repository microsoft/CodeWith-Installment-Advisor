@description('Name of the API Management service')
param apimServiceName string = 'apim-${uniqueString(resourceGroup().id)}'

@description('Email address of the API Management publisher')
param publisherEmail string

@description('Name of the API Management publisher')
param publisherName string

var location = resourceGroup().location

module apiManagement 'modules/apiManagement.bicep' = {
  name: 'apiManagement'
  params: {
    apimServiceName: apimServiceName
    location: location
    publisherEmail: publisherEmail
    publisherName: publisherName
  }
}
