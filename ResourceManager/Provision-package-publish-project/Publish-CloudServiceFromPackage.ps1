Param([string]$publishsettings,
      [string]$storageaccount,
      [string]$subscription,
      [string]$cloudServiceName,
      [string]$containerName="mydeployments",
      # [string]$config = ".\ServiceConfiguration.Cloud.cscfg",
      # [string]$package = ".\TweetPublishService.cspkg",
      [string]$config,
      [string]$package,
      [string]$slot="Production"),
      [string]$groupname
      

 
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
 
    Set-AzureSubscription -SubscriptionId $subscription -CurrentStorageAccount $storageaccount
 
    Select-AzureSubscription -SubscriptionId $subscription
}
 
Function Upload-Package($package, $containerName){
    $blob = "$cloudServiceName.package.$(get-date -f yyyy_MM_dd_hh_ss).cspkg"
     
    $containerState = Get-AzureStorageContainer -Name $containerName -ea 0
    if ($containerState -eq $null)
    {
        New-AzureStorageContainer -Name $containerName | out-null
    }
     
    Set-AzureStorageBlobContent -File $package -Container $containerName -Blob $blob -Force| Out-Null
    $blobState = Get-AzureStorageBlob -blob $blob -Container $containerName
 
    $blobState.ICloudBlob.uri.AbsoluteUri
}
 
Function Create-Deployment($package_url, $service, $slot, $config){
    $opstat = New-AzureDeployment -Slot $slot -Package $package_url -Configuration $config -ServiceName $service
}
  
Function Upgrade-Deployment($package_url, $service, $slot, $config){
    $setdeployment = Set-AzureDeployment -Upgrade -Slot $slot -Package $package_url -Configuration $config -ServiceName $service -Force
}
 
Function Check-Deployment($service, $slot){
    $completeDeployment = Get-AzureDeployment -ServiceName $service -Slot $slot
    $completeDeployment.deploymentid
}
$i=1
 
try{
    # Write-Host "Running Azure Imports"
    # Import-Module "C:Program Files (x86)Microsoft SDKsWindows AzurePowerShellAzureAzure.psd1"
 
    Switch-AzureMode AzureServiceManagement
    

    if(!$groupname){
        $groupname = Read-Host "Enter group number or short group name (max 5 characters!)"  
    }
    if($groupname.Length -lt 2){
        $groupname = "GRGroup" + $groupname
    }     

    if(!$cloudServiceName){
        $cloudServiceName = $groupname + "TweetPublisher"        
    }

    $storageaccount = $groupname.ToLower() + "deployments"


    if (!$subscription){
        $subscriptions = Get-AzureSubscription
        foreach($s in $subscriptions){$s | select "[$i]`r", SubscriptionId, SubscriptionName, DefaultAccount | format-table; $i++}
        $c = Read-Host "Select which subscription to use"  
        # $subscription = Read-Host "Subscription (case-sensitive)"
        $subscription = $subscriptions[$c-1].SubscriptionId
    }
    if (!$storageaccount){    $storageaccount = Read-Host "Storage account name"}
    if (!$cloudServiceName){            $service = Read-Host "Cloud service name"}
    if (!$publishsettings){    $publishsettings = Get-File "Azure publish settings (*.publishsettings)|*.publishsettings"}
    if (!$package){            $package = Get-File "Azure package (*.cspkg)|*.cspkg"}
    if (!$config){            $config = Get-File "Azure config file (*.cspkg)|*.cscfg"}
 
    Set-AzureSettings -publishsettings $publishsettings -subscription $subscription -storageaccount $storageaccount
 
    "Upload the deployment package"
    $package_url = Upload-Package -package $package -containerName $containerName
    "Package uploaded to $package_url"
 
    $deployment = Get-AzureDeployment -ServiceName $cloudServiceName -Slot $slot -ErrorAction silentlycontinue 
 
 
    if ($deployment.Name -eq $null) {
        Write-Host "No deployment is detected. Creating a new deployment. "
        Create-Deployment -package_url $package_url -service $cloudServiceName -slot $slot -config $config
        Write-Host "New Deployment created"
 
    } else {
        Write-Host "Deployment exists in $cloudServiceName.  Upgrading deployment."
        Upgrade-Deployment -package_url $package_url -service $cloudServiceName -slot $slot -config $config
        Write-Host "Upgraded Deployment"
    }
 
    $deploymentid = Check-Deployment -service $cloudServiceName -slot $slot
    Write-Host "Deployed to $cloudServiceName with deployment id $deploymentid"
    exit 0
}
catch [System.Exception] {
    Write-Host $_.Exception.ToString()
    exit 1
}