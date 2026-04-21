//-----------------------------------------------------------------------------------------------------------------------------------------------
// PARAMETERS
//-----------------------------------------------------------------------------------------------------------------------------------------------
param location string
param tags object
param resourcePrefix string
param managedIdentityName string
param managedIdentitySuffix string

//-----------------------------------------------------------------------------------------------------------------------------------------------
// VARIABLES
//-----------------------------------------------------------------------------------------------------------------------------------------------
@description('Specifies the name of the managed identity.')
var msiName = '${resourcePrefix}-${managedIdentityName}-${managedIdentitySuffix}'

//-----------------------------------------------------------------------------------------------------------------------------------------------
// RESOURCES
//-----------------------------------------------------------------------------------------------------------------------------------------------
// Create user assigned managed identity
resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' = {
  name: msiName
  location: location
  tags: tags
}

//-----------------------------------------------------------------------------------------------------------------------------------------------
// OUTPUT
//-----------------------------------------------------------------------------------------------------------------------------------------------
@description('The resource ID of the user-assigned managed identity.')
output uaiId string = managedIdentity.properties.principalId

@description('The resource name of the user-assigned managed identity.')
output uaiName string = '${msiName}'
