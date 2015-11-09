Write-Host "Use Publish-CloudService2.ps1 instead"
    exit 0


Param([string]$publishsettings,
      [string]$subscription,
      [string]$containerName="mydeployments",
      [string]$config = ".\ServiceConfiguration.Cloud.cscfg",
      [string]$package = ".\TweetPublishService.cspkg",
      [string]$slot="Production",
      
      [string]$resourceGroupName,
      [string]$tweetPublishServiceName,
      [string]$tweetHandlerServiceName,
      [string]$storageaccount,
      [string]$searchName,
      [string]$groupname
      
      )
 Switch-AzureMode AzureServiceManagement
 
Function Get-File($filter){
    [System.Reflection.Assembly]::LoadWithPartialName("System.windows.forms") | Out-Null
    $fd = New-Object system.windows.forms.openfiledialog
    $fd.MultiSelect = $false
    $fd.Filter = $filter
    [void]$fd.showdialog()
    return $fd.FileName
}
 
Function Set-AzureSettings($publishsettings, $subscription, $storageaccount){
    Import-AzurePublishSettingsFile $publishsettings
 
    Set-AzureSubscription -SubscriptionId $subscription -CurrentStorageAccountName $deploymentstorag
 
    Select-AzureSubscription -SubscriptionId $subscription
}

 
Function Upload-Package($package, $containerName){
    $blob = "$tweetPublishServiceName.package.$(get-date -f yyyy_MM_dd_hh_ss).cspkg"
     
    $containerState = Get-AzureStorageContainer -Name $containerName -ea 0
    if ($containerState -eq $null)
    {
        New-AzureStorageContainer -Name $containerName | out-null
    }
     
    Set-AzureStorageBlobContent -File $package -Container $containerName -Blob $blob -Force| Out-Null
    $blobState = Get-AzureStorageBlob -blob $blob -Container $containerName
 
    $blobState.ICloudBlob.uri.AbsoluteUri
}
 
Function Create-Deployment($package_url, $tweetPublishServiceName, $slot, $config){
    $opstat = New-AzureDeployment -Slot $slot -Package $package_url -Configuration $config -ServiceName $tweetPublishServiceName
}
  
Function Upgrade-Deployment($package_url, $tweetPublishServiceName, $slot, $config){
    $setdeployment = Set-AzureDeployment -Upgrade -Slot $slot -Package $package_url -Configuration $config -ServiceName $tweetPublishServiceName -Force
}
 
Function Check-Deployment($tweetPublishServiceName, $slot){
    $completeDeployment = Get-AzureDeployment -ServiceName $tweetPublishServiceName -Slot $slot
    $completeDeployment.deploymentid
}
 
try{
    Write-Host "Running Azure Imports"
    #Import-Module "C:Program Files (x86)Microsoft SDKsWindows AzurePowerShellAzureAzure.psd1"
 
    Write-Host "Gathering information"
    $groupname = Read-Host "Enter group number or short group name (max 5 characters!)"  
    if($groupname.Length -lt 2){
        $groupname = "GRGroup" + $groupname
    }     
    $resourceGroupName = $groupname + "Resources"
    $tweetPublishServiceName = $groupname + "TweetPublisher"
    $tweetHandlerServiceName = $groupname + "TweetHandler"
    $searchName = $groupname.ToLower() + "search"
    $storageaccount = $groupname.ToLower() + "storage"
        
    if (!$resourceGroupName){   
        $resourceGroupName = Read-Host "Resource group name, i.e. GRGroupX"
        $storageaccount = $groupname.ToLower() + "storage"
    }
    
    
    if (!$tweetPublishServiceName){$tweetPublishServiceName = Read-Host "Tweet publish service name, i.e. tweetPublisherGroupX. (Must be unique)"}
    if (!$tweetHandlerServiceName){$tweetHandlerServiceName = Read-Host "Tweet handler name, i.e. tweetEventHandlerGroupX. (Must be unique)"}
    if (!$storageaccount){$storageaccount = Read-Host "Tweet storage account name, i.e. tweetstoragegroupx. (Must be unique, and only contain lower case letters)"}
    if (!$searchName){$searchName = Read-Host "Search name,  i.e. groupxsearch. (Must be unique, only contain lower case letters and not be longer than 15 characters)"}
    if (!$subscription){$subscription = Read-Host "SubscriptionId (Get using Get-AzureSubscription)"}
    if (!$publishsettings){$publishsettings = Get-File "Azure publish settings (*.publishsettings)|*.publishsettings"}
    if (!$package){$package = Get-File "Azure package (*.cspkg)|*.cspkg"}
    if (!$config){$config = Get-File "Azure config file (*.cspkg)|*.cscfg"}

 
    Write-Host "Importing publish profile and setting subscription"
    Set-AzureSettings -publishsettings $publishsettings -subscription $subscription -storageaccount $storageaccount
 
    "Upload the deployment package"
    $package_url = Upload-Package -package $package -containerName $containerName
    "Package uploaded to $package_url"
 
    $deployment = Get-AzureDeployment -ServiceName $tweetPublishServiceName -Slot $slot -ErrorAction silentlycontinue 
 
 
    if ($deployment.Name -eq $null) {
        Write-Host "No deployment is detected. Creating a new deployment. "
        Create-Deployment -package_url $package_url -service $tweetPublishServiceName -slot $slot -config $config
        Write-Host "New Deployment created"
 
    } else {
        Write-Host "Deployment exists in $tweetPublishServiceName.  Upgrading deployment."
        Upgrade-Deployment -package_url $package_url -service $tweetPublishServiceName -slot $slot -config $config
        Write-Host "Upgraded Deployment"
    }
 
    $deploymentid = Check-Deployment -service $tweetPublishServiceName -slot $slot
    Write-Host "Deployed to $tweetPublishServiceName with deployment id $deploymentid"
    exit 0
}
catch [System.Exception] {
    Write-Host $_.Exception.ToString()
    exit 1
}