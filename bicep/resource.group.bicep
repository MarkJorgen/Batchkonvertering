targetScope='subscription'
//-----------------------------------------------------------------------------------------------------------------------------------------------
// PARAMETERS
//-----------------------------------------------------------------------------------------------------------------------------------------------
// @description('Name for resource group')
// param resourceGroupName string 

@description('Location for all resources.')
param location string

@description('Location for all resources.')
param environmentName string

@description('Tags for resource')
param createdBy string = 'timengo'


//-----------------------------------------------------------------------------------------------------------------------------------------------
// VARIABLES
//-----------------------------------------------------------------------------------------------------------------------------------------------
var resourceGroupName = 'gif-${environmentName}-dev-app-crmapi'
//-----------------------------------------------------------------------------------------------------------------------------------------------
// RESOURCES
//-----------------------------------------------------------------------------------------------------------------------------------------------
resource newRG 'Microsoft.Resources/resourceGroups@2022-09-01' = {
  name: resourceGroupName
  location:location
  tags: {
    'Created By': createdBy
    'Environment': environmentName
  }
}
