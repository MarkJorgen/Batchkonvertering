//-----------------------------------------------------------------------------------------------------------------------------------------------
// PARAMETERS
//-----------------------------------------------------------------------------------------------------------------------------------------------
param resourcePrefix string 
param workSpaceName string 
param workSpaceNameSuffix string 
param appInsightsName string 
param appInsightsSuffix string 
param location string
param tags object
param environmentName string = 'dev'

@description('Specifies the network access type for Log Analytics ingestion.')
@allowed([
  'Enabled'
  'Disabled'
])
param publicNetworkAccessForIngestion string = 'Enabled'

@description('Specifies the network access type for accessing Log Analytics query.')
@allowed([
  'Enabled'
  'Disabled'
])
param publicNetworkAccessForQuery string = 'Enabled'

@description('Specifies which permisision option to use.')
@allowed([
  true
  false
])
param enableLogAccessUsingOnlyResourcePermissions bool = true
//-----------------------------------------------------------------------------------------------------------------------------------------------
// VARIABLES
//-----------------------------------------------------------------------------------------------------------------------------------------------
@description('Application Insights resource name')
var applicationInsightsName = '${resourcePrefix}-${environmentName}-app-${appInsightsName}-${appInsightsSuffix}'

@description('Log Analytics resource name')
var logAnalyticsWorkspaceName = '${resourcePrefix}-${environmentName}-app-${workSpaceName}-${workSpaceNameSuffix}'

//-----------------------------------------------------------------------------------------------------------------------------------------------
// RESOURCES
//-----------------------------------------------------------------------------------------------------------------------------------------------
// Create workspace
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2021-06-01' = {
  name: logAnalyticsWorkspaceName
  location: location
  tags: tags
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    features: {
      enableLogAccessUsingOnlyResourcePermissions: enableLogAccessUsingOnlyResourcePermissions
    }
    retentionInDays: 30
    publicNetworkAccessForIngestion: publicNetworkAccessForIngestion
    publicNetworkAccessForQuery: publicNetworkAccessForQuery
  }
}

// Create insights component
resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: applicationInsightsName
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
    Flow_Type: 'Bluefield'
  }
}

//-----------------------------------------------------------------------------------------------------------------------------------------------
// OUTPUT
//-----------------------------------------------------------------------------------------------------------------------------------------------
@description('The resource ID of the log analytics workspace.')
output logAnalyticsWorkspacesId string = logAnalyticsWorkspace.id

@description('The resource ID of the Application Insights component.')
output applicationInsightsId string = applicationInsights.id

@description('The resource Name of the Application Insights component.')
output applicationInsightsName string = applicationInsights.name
