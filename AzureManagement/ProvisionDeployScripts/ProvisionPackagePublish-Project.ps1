Param(
	[string]$resourceGroupName,
	[string]$storageAccountName,
	[string]$geekRetreatRepoPath,
	[string]$groupname,
	[string]$tweetPublisherCsproj,
	[string]$tweetHandlerCsproj,
	[string]$tweetHandlerOut,
	[string]$tweetPublisherOut,
	[string]$searchApiKey,
	[string]$searchName,
	[string]$subscription,
	[string]$publishsettings
)

##""""""""""Functions""""""""""
	
Function UpdateConfigFile($workerName, $storageAccountName, $storageAccountKey, $publishDir){
		Write-Host "PUBLISH DIR:    $publishDir"
	#Read and update config files
	$cscfgFile = "$publishDir\ServiceConfiguration.Cloud.cscfg"

	Write-Host "CONFIGE FILE:    "$cscfgFile
	$configDoc = (Get-Content $cscfgFile) -as [Xml]
	#Update config
	$tmp = $configDoc.ServiceConfiguration.Role | where {$_.name -eq $workerName} 
	$obj = $tmp.ConfigurationSettings.Setting | where {$_.name -eq 'StorageConnectionString'}
	$obj.value = "DefaultEndpointsProtocol=https;AccountName=$storageAccountName;AccountKey=$storageAccountKey";
	#Save to cscfg
	$configDoc.Save($cscfgFile)
}

Function UpdateSearchApiConfig($workerName, $searchApiKey, $searchName, $publishDir){
	Write-Host "PUBLISH DIR:    $publishDir"
	#Read and update config files
	$cscfgFile = "$publishDir\ServiceConfiguration.Cloud.cscfg"

	Write-Host "CONFIGE FILE:    "$cscfgFile
	$configDoc = (Get-Content $cscfgFile) -as [Xml]
	#Update config
	$tmp = $configDoc.ServiceConfiguration.Role | where {$_.name -eq $workerName} 
	$obj = $tmp.ConfigurationSettings.Setting | where {$_.name -eq 'SearchApiKey'}
	$obj.value = $searchApiKey
	$obj2 = $tmp.ConfigurationSettings.Setting | where {$_.name -eq 'SearchServiceName'}
	$obj2.value = $searchName
	#Save to cscfg
	$configDoc.Save($cscfgFile)
}

Function Select-FolderDialog
{
	param([string]$Description="Browse to Geek Retreat source repository",[string]$RootFolder="MyComputer")
	
 	[System.Reflection.Assembly]::LoadWithPartialName("System.windows.forms") |
    Out-Null     

   	$objForm = New-Object System.Windows.Forms.FolderBrowserDialog
    $objForm.Rootfolder = $RootFolder
    $objForm.Description = $Description
    $Show = $objForm.ShowDialog()
    If ($Show -eq "OK")
	{
    	Return $objForm.SelectedPath
	}Else
	{
		Write-Error "Operation cancelled by user."
	}
}

Function Get-File($filter){
    [System.Reflection.Assembly]::LoadWithPartialName("System.windows.forms") | Out-Null
    $fd = New-Object system.windows.forms.openfiledialog
    $fd.MultiSelect = $false
    $fd.Filter = $filter
    [void]$fd.showdialog()
    return $fd.FileName
}
Function Get-Subscription(){
	$i = 1
	if (!$subscription){
		$subscriptions = Get-AzureSubscription
		foreach($s in $subscriptions){$s | select "[$i]`r", SubscriptionId, SubscriptionName, DefaultAccount | format-table; $i++}
		$c = Read-Host "Select which subscription to use"  
		# $subscription = Read-Host "Subscription (case-sensitive)"
		$subscription = $subscriptions[$c-1].SubscriptionId
	}
}

Function Get-PublishSettings(){
	if (!$publishsettings){    
		$publishsettings = Get-File "Azure publish settings (*.publishsettings)|*.publishsettings"
		$publishsettings -replace " ","` "
	}
}

Function PackageProject($out, $csprojpath){
	Invoke-Expression "msbuild $csprojpath /p:Configuration=Release /p:DebugType=None /p:Platform=AnyCpu /p:OutputPath=$out /p:TargetProfile=Cloud /t:publish"  
}
#"""""""""""""""""""""""""""""

##""""""Helper variables""""""
$currentDir = (Get-Item -Path ".\" -Verbose).FullName
$appPublish = "app.publish"
	
$tweetHandlerOut = "$currentDir\TweetHandlerOut\"
$tweetPublisherOut = "$currentDir\TweetPublisherOut\"
	
$geekRetreatRepoPath = "..\.."
$tweetHandlerCsproj = "$geekRetreatRepoPath\TweetHandlerService\TweetHandlerService.ccproj"
$tweetPublisherCsproj = "$geekRetreatRepoPath\TweetPublishService\TweetPublishService.ccproj"  

$tweetHandlerAppDir = "$tweetHandlerOut$appPublish"
$tweetPublisherAppDir= "$tweetPublisherOut$appPublish"
#"""""""""""""""""""""""""""""

##"""""Gather information""""""
$groupname = Read-Host "Enter group number or short group name (max 5 characters!)"  
if($groupname.Length -lt 3){
	$groupname = "GRGroup" + $groupname
}

$resourceGroupName = $groupname + "Resources"
$storageAccountName = $groupname.ToLower() + "storage"
$searchName = $groupname.ToLower() + "search"
#"""""""""""""""""""""""""""""


#Create resource group and get the new storage key
Write-Host "Creating resource group with name : $groupname"
Write-Host "Storage account : $storageAccountName"
Invoke-Expression -Command ".\Create-GRResourceGroup.ps1 $groupname $storageAccountName"
Write-Host "Fetching storage connection key"
$storageAccountKey = (Get-AzureStorageAccountKey -ResourceGroupName $resourceGroupName -Name $storageAccountName).Key1
# 
# #Package projects 
Write-Host "Packaging cloud services from:"
Write-Host "$tweetPublisherCsproj into $tweetPublisherOut and"
Write-Host "$tweetHandlerCsproj into $tweetHandlerOut"
PackageProject($tweetHandlerOut, $tweetHandlerCsproj)
PackageProject($tweetPublisherOut, $tweetPublisherCsproj)


$tweetHandlerCspkg = "$tweetHandlerOut$appPublish\TweetHandlerService.cspkg"
$tweetPublisherCspkg = "$tweetPublisherOut$appPublish\TweetPublishService.cspkg"

# 
# #Update configs
Write-Host "Updating configuration files for the cloud service projects"
UpdateConfigFile "SearchIndexWorker" $storageAccountName $storageAccountKey $tweetHandlerAppdir
UpdateConfigFile "TweetEventHandler" $storageAccountName $storageAccountKey $tweetHandlerAppdir
UpdateConfigFile "TweetrPublisher" $storageAccountName $storageAccountKey $tweetPublisherAppDir
# 
# #Ask for search API key and update this aswell
if(!$searchApiKey){
	$searchApiKey = Read-Host "Please enter searchApiKey, accessible from the Azure Portal"
}

Write-Host "Updating search configuration for SeaerchIndexWorker in TweetHandler service."
UpdateSearchApiConfig "SearchIndexWorker" $searchApiKey $searchName $tweetHandlerAppdir 
$tweetPublisher = "TweetPublisher"
$tweetHandler = "TweetHandler"




#Publish services
$i = 1
if (!$subscription){
	$subscriptions = Get-AzureSubscription
	foreach($s in $subscriptions){$s | select "[$i]`r", SubscriptionId, SubscriptionName, DefaultAccount | format-table; $i++}
	$c = Read-Host "Select which subscription to use"  
	# $subscription = Read-Host "Subscription (case-sensitive)"
	$subscription = $subscriptions[$c-1].SubscriptionId
}

if (!$publishsettings){    
	$publishsettings = Get-File "Azure publish settings (*.publishsettings)|*.publishsettings"
	$publishsettings = $publishsettings -replace ' ','` '		
}
Write-Host "publisheettings1:$publishsettings"
Write-Host "Publishing the Cloud Services."
Invoke-Expression -Command ".\Publish-CloudServiceFromPackage.ps1 -groupname $groupname -subscription $subscription -publishsettings $publishsettings -cloudServiceName $groupname$tweetPublisher -config $tweetPublisherOut$appPublish\ServiceConfiguration.Cloud.cscfg -package $tweetPublisherOut$appPublish\TweetPublishService.cspkg"
Invoke-Expression -Command ".\Publish-CloudServiceFromPackage.ps1 -groupname $groupname -subscription $subscription -publishsettings $publishsettings -cloudServiceName $groupname$tweetHandler -config $tweetHandlerOut$appPublish\ServiceConfiguration.Cloud.cscfg -package $tweetHandlerOut$appPublish\TweetHandlerService.cspkg"

##"""""""DONE"""""""
Write-Host "Finished provisioning, configuring, packaging and publishing!!!"



