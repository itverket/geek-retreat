1. Run Add-AzureAccount
2. Select-AzureSubscription  (Get-AzureSubscription to list aviable ones)
3. Run Create-PublishSubscribeTweetResourceGroup.ps1 to provision the required components in a resource group in Azure.
4. Update the connection strings in ServiceConfiguration.Cloud.cscfg
5. Download and save publish settings with Get-AzurePublishSettingsFile
6. Run Publish-CloudService2.ps1 and chose desired .cspkg and .cscfg files to publish it. (Remember to chose the same group name)