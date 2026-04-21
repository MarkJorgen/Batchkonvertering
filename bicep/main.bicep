//-----------------------------------------------------------------------------------------------------------------------------------------------
// PARAMETERS
//-----------------------------------------------------------------------------------------------------------------------------------------------
@description('General parameters')
param location string
param resourcePrefix string

// @description('Parameters for Managed Identitiy')
// param managedIdentityName string
// param managedIdentitySuffix string

@description('Location for all resources.')
param environmentName string
param env string

// @description('Parameters for storage account')
// param storageAccountSuffix string
// param storageAccountName string

// @description('Parameters for Azure app configuration store')
// param appConfigName string
// param appConfigSuffix string

@description('Parameters for App service')
param appServicePlanSuffix string
param appServicePlanName string
param webAppSuffix string
param webAppName string 
//param selfServiceAppName string

@description('Parameters for Log analytics and app insights')
param appInsightsName string
param appInsightsSuffix string
param workSpaceName string
param workSpaceNameSuffix string

param sku string


//-----------------------------------------------------------------------------------------------------------------------------------------------
// VARIABLES
//-----------------------------------------------------------------------------------------------------------------------------------------------
@description('Tags for all resources')
var tagValues = {
  CreatedBy: 'timengo'
  Environment: environmentName
}

//-----------------------------------------------------------------------------------------------------------------------------------------------
// RESOURCES
//-----------------------------------------------------------------------------------------------------------------------------------------------
// Deploy log analytcis workspace and insights component
module appi 'modules/application.insights.bicep' = {
  name: 'batch-infrastructure-logging'
  params: {
    location: location
    tags: tagValues
    resourcePrefix: resourcePrefix
    appInsightsName: appInsightsName
    appInsightsSuffix: appInsightsSuffix
    workSpaceName: workSpaceName
    workSpaceNameSuffix: workSpaceNameSuffix
    environmentName: env
  }
}

// Deploy Azure app plan and app service
module app 'modules/app.service.bicep' = {
  name: 'batch-infrastructure-appservice'
  params: {
    location: location
    tags: tagValues
    resourcePrefix: resourcePrefix
    appServicePlanSuffix: appServicePlanSuffix
    appServicePlanName: appServicePlanName
    webAppSuffix: webAppSuffix
    webAppName: webAppName
    appInsightsName: appi.outputs.applicationInsightsName
    sku: sku
    environmentName: env
    //principalId: uai.outputs.uaiId
    //principalName: uai.outputs.uaiName
  }
}

//-----------------------------------------------------------------------------------------------------------------------------------------------
// OUTPUT
//-----------------------------------------------------------------------------------------------------------------------------------------------
