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

var location = resourceGroup().location

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
