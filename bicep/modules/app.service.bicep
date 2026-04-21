targetScope='resourceGroup'
//-----------------------------------------------------------------------------------------------------------------------------------------------
// PARAMETERS
//-----------------------------------------------------------------------------------------------------------------------------------------------
param resourcePrefix string
param appServicePlanSuffix string
param appServicePlanName string
param webAppSuffix string
param webAppName string 
param location string
param tags object
param appInsightsName string 
param environmentName string = 'dev'

// @description('Principal id which needs to be assigned to app service')
// param principalId string 

// @description('Principal name which needs to be assigned to app service')
// param principalName string 

@description('The language stack of the app.')
@allowed([
  '.net'
  'php'
  'node'
  'html'
])
param language string = '.net'
var configReference = {
  '.net': {
    comments: '.Net app. No additional configuration needed.'
  }
  html: {
    comments: 'HTML app. No additional configuration needed.'
  }
  php: {
    phpVersion: '7.4'
  }
  node: {
    appSettings: [
      {
        name: 'WEBSITE_NODE_DEFAULT_VERSION'
        value: '12.15.0'
      }
    ]
  }
}

@description('Describes plan\'s pricing tier and instance size. Check details at https://azure.microsoft.com/en-us/pricing/details/app-service/')
@allowed([
  'F1'
  'D1'
  'B1'
  'B2'
  'B3'
  'S1'
  'S2'
  'S3'
  'P0V3'
  'P1'
  'P2'
  'P3'
  'P4'
])
param sku string = 'D1'

//-----------------------------------------------------------------------------------------------------------------------------------------------
// VARIABLES
//-----------------------------------------------------------------------------------------------------------------------------------------------
var appName = '${resourcePrefix}-${environmentName}-app-${webAppName}-${webAppSuffix}'
var appPlanName = '${resourcePrefix}-${environmentName}-app-${appServicePlanName}-${appServicePlanSuffix}'

//-----------------------------------------------------------------------------------------------------------------------------------------------
// RESOURCES
//-----------------------------------------------------------------------------------------------------------------------------------------------
resource asp 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: appPlanName
  location: location
  sku: {
    name: sku
  }
  tags: tags
}

resource appInsightsResource 'Microsoft.Insights/components@2020-02-02' existing = {
  name: appInsightsName
}

resource webApp 'Microsoft.Web/sites@2022-03-01' = {
  name: appName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  tags: tags
  properties: {
    siteConfig: union(configReference[language],{
      webSocketsEnabled: true
      minTlsVersion: '1.2'
      scmMinTlsVersion: '1.2'
      ftpsState: 'FtpsOnly'
      appSettings: [
        {
          name: 'APPINSIGHTS_CONNECTIONSTRING'
          value: appInsightsResource.properties.ConnectionString
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsightsResource.properties.InstrumentationKey
        }
      ]
    })
    serverFarmId: asp.id
    httpsOnly: true    
  }
}

//-----------------------------------------------------------------------------------------------------------------------------------------------
// OUTPUT
//-----------------------------------------------------------------------------------------------------------------------------------------------
@description('The resource ID of the Azure web app.')
output webAppid string = webApp.id
