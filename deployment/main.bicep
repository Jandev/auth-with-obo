@description('The location of the resource group')
param location string = 'North Europe'

@description('The name of the BackendService App Service Plan')
param backendServicePlanName string = 'backendServicePlan'

@description('The name of the IntegratingService App Service Plan')
param integratingServicePlanName string = 'integratingServicePlan'

@description('The name of the BackendService App Service')
param backendServiceAppName string = 'backendServiceApp'

@description('The name of the IntegratingService App Service')
param integratingServiceAppName string = 'integratingServiceApp'

@description('The name of the Log Analytics Workspace')
param logAnalyticsWorkspaceName string = 'logAnalyticsWorkspace'

@description('The name of the Application Insights resource')
param appInsightsName string = 'appInsights'

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

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2021-06-01' = {
  name: logAnalyticsWorkspaceName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
  }
}

resource backendServiceIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-07-31-preview' = {
  name: '${backendServiceAppName}-identity'
  location: location
}

resource integratingServiceIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-07-31-preview' = {
  name: '${integratingServiceAppName}-identity'
  location: location
}

resource backendServiceApp 'Microsoft.Web/sites@2022-03-01' = {
  name: backendServiceAppName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${backendServiceIdentity.id}': {}
    }
  }
  properties: {
    serverFarmId: backendServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      appSettings: [
        {
          name: 'AzureAd__Instance'
          value: 'https://login.microsoftonline.com/'
        }
        {
          name: 'AzureAd__Domain'
          value: 'jan-v.nl'
        }
        {
          name: 'AzureAd__TenantId'
          value: '4b1fa0f3-862b-4951-a3a8-df1c72935c79'
        }
        {
          name: 'AzureAd__ClientId'
          value: '8ebbea06-f01e-4f94-8254-32da2e94c240'
        }
        {
          name: 'AzureAd__CallbackPath'
          value: '/signin-oidc'
        }
        {
          name: 'AzureAd__Scopes'
          value: 'BackendDefault'
        }
        {
          name: 'Logging__LogLevel__Default'
          value: 'Information'
        }
        {
          name: 'Logging__LogLevel__Microsoft.AspNetCore'
          value: 'Warning'
        }
        {
          name: 'AllowedHosts'
          value: '*'
        }
        {
          name: 'MicrosoftGraph__BaseUrl'
          value: 'https://graph.microsoft.com/v1.0'
        }
        {
          name: 'MicrosoftGraph__Scopes'
          value: 'user.read'
        }
        {
          name: 'BackendService__BaseUrl'
          value: 'https://backendserviceapp.azurewebsites.net'
        }
        {
          name: 'BackendService__ApplicationIdUri'
          value: 'api://8ebbea06-f01e-4f94-8254-32da2e94c240'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsights.properties.InstrumentationKey
        }
        {
          name: 'ASPNETCORE_HTTPS_PORT'
          value: '443'
        }
      ]
    }
  }
}

resource integratingServiceApp 'Microsoft.Web/sites@2022-03-01' = {
  name: integratingServiceAppName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${integratingServiceIdentity.id}': {}
    }
  }
  properties: {
    serverFarmId: integratingServicePlan.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      appSettings: [
        {
          name: 'AzureAd__Instance'
          value: 'https://login.microsoftonline.com/'
        }
        {
          name: 'AzureAd__Domain'
          value: 'jan-v.nl'
        }
        {
          name: 'AzureAd__TenantId'
          value: '4b1fa0f3-862b-4951-a3a8-df1c72935c79'
        }
        {
          name: 'AzureAd__ClientId'
          value: 'afad1932-49c8-4e3c-a3a1-cf9543c84d9e'
        }
        {
          name: 'AzureAd__CallbackPath'
          value: '/signin-oidc'
        }
        {
          name: 'AzureAd__Scopes'
          value: 'CustomDefault'
        }
        {
          name: 'AzureAd__ClientSecret'
          value: 'Client secret from app-registration. Check user secrets/azure portal.'
        }
        {
          name: 'AzureAd__ClientCertificates'
          value: '[]'
        }
        {
          name: 'Logging__LogLevel__Default'
          value: 'Information'
        }
        {
          name: 'Logging__LogLevel__Microsoft.AspNetCore'
          value: 'Warning'
        }
        {
          name: 'AllowedHosts'
          value: '*'
        }
        {
          name: 'MicrosoftGraph__BaseUrl'
          value: 'https://graph.microsoft.com/v1.0'
        }
        {
          name: 'MicrosoftGraph__Scopes'
          value: 'user.read'
        }
        {
          name: 'BackendService__BaseUrl'
          value: 'https://backendserviceapp.azurewebsites.net'
        }
        {
          name: 'BackendService__ApplicationIdUri'
          value: 'api://8ebbea06-f01e-4f94-8254-32da2e94c240'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appInsights.properties.InstrumentationKey
        }
        {
          name: 'ASPNETCORE_HTTPS_PORT'
          value: '443'
        }
      ]
    }
  }
}

output backendServicePlanId string = backendServicePlan.id
output integratingServicePlanId string = integratingServicePlan.id
output backendServiceAppId string = backendServiceApp.id
output integratingServiceAppId string = integratingServiceApp.id
output logAnalyticsWorkspaceId string = logAnalyticsWorkspace.id
output appInsightsId string = appInsights.id
output backendServiceIdentityId string = backendServiceIdentity.id
output integratingServiceIdentityId string = integratingServiceIdentity.id
