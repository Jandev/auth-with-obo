@description('The location of the resource group')
param location string = 'West Europe'

@description('The name of the BackendService App Service Plan')
param backendServicePlanName string = 'backendServicePlan'

@description('The name of the IntegratingService App Service Plan')
param integratingServicePlanName string = 'integratingServicePlan'

@description('The name of the BackendService App Service')
param backendServiceAppName string = 'backendServiceApp'

@description('The name of the IntegratingService App Service')
param integratingServiceAppName string = 'integratingServiceApp'

resource backendServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: backendServicePlanName
  location: location
  sku: {
    name: 'S1'
    tier: 'Standard'
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
}

resource integratingServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: integratingServicePlanName
  location: location
  sku: {
    name: 'S1'
    tier: 'Standard'
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
}

resource backendServiceApp 'Microsoft.Web/sites@2022-03-01' = {
  name: backendServiceAppName
  location: location
  properties: {
    serverFarmId: backendServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
    }
  }
}

resource integratingServiceApp 'Microsoft.Web/sites@2022-03-01' = {
  name: integratingServiceAppName
  location: location
  properties: {
    serverFarmId: integratingServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
    }
  }
}

output backendServicePlanId string = backendServicePlan.id
output integratingServicePlanId string = integratingServicePlan.id
output backendServiceAppId string = backendServiceApp.id
output integratingServiceAppId string = integratingServiceApp.id
