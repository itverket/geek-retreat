[CmdletBinding()]
Param(
	[string]$resourceGroupName,
	[string]$storageAccountName,
	[string]$geekRetreatRepoPath,
	[Parameter(Position=1)]
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

$ErrorActionPreference = 'stop'

##""""""""""Functions""""""""""
	
Function UpdateConfigFile($workerName, $storageAccountName, $storageAccountKey, $publishDir){
	#Read and update config files
	$cscfgFile = "$publishDir\ServiceConfiguration.Cloud.cscfg"
	$configDoc = (Get-Content $cscfgFile) -as [Xml]
	#Update config
	$tmp = $configDoc.ServiceConfiguration.Role | where {$_.name -eq $workerName} 
	$obj = $tmp.ConfigurationSettings.Setting | where {$_.name -eq 'StorageConnectionString'}
	$obj.value = "DefaultEndpointsProtocol=https;AccountName=$storageAccountName;AccountKey=$storageAccountKey";
	#Save to cscfg
	$configDoc.Save($cscfgFile)
}

Function UpdateSearchApiConfig($workerName, $searchApiKey, $searchName, $publishDir){
	#Read and update config files
	$cscfgFile = "$publishDir\ServiceConfiguration.Cloud.cscfg"
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
	$fd.Title = "Select azure publish settings file"
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
clear
$groupname = Read-Host "Enter group number or short group name (max 5 characters!)"  
if($groupname.Length -lt 3){
	$groupname = "GRGroup" + $groupname
}

$resourceGroupName = $groupname + "Resources"
$storageAccountName = $groupname.ToLower() + "storage"
$searchName = $groupname.ToLower() + "search"

$subscriptions = Get-AzureSubscription 
$subscription = $subscriptions[0].SubscriptionId	

#"""""""""""""""""""""""""""""

##"""""""""Run loop"""""""""
$1 = "1. Provision new resource group."
$2 = "2. Package projects."
$3 = "3. Update service configuration."
$4 = "4. Publish project (package first)"

$a = "a. Add Azure account."
$s = "s. Select Azure subscription"
$th = "th. Get TweetHandlerService status"
$tp = "tp. Get TweetPublisherService status"
$p = "p. Download Azure publishsettings file"

$GRCreated = ""
$PackageCreated =""
$ConfigsUpdated = ""
$ProjectPublished =""

do{
	if($taskComplete){
		clear
		Write-Host $taskComplete
		Read-Host "Press any key to continue..."
	}
	clear
	Get-AzureSubscription -Current | select SubscriptionId, SubscriptionName | format-table 
	Write-Host "-------------------------------------------------------------------`n"
	"{0,-2} {1, -40} {2,-1} {3, -20}" -f $GRCreated,$1,"|", $a
	"{0,-2} {1, -40} {2,-1} {3, -20}" -f $PackageCreated,$2,"|", $s
	"{0,-2} {1, -40} {2,-1} {3, -20}" -f $ConfigsUpdated,$3,"|", $th
	"{0,-2} {1, -40} {2,-1} {3, -20}" -f $ProjectPublished,$4,"|", $tp
	"{0,-2} {1, -40} {2,-1} {3, -20}" -f "","","|", $p
	Write-Host "`n"
	$res = Read-Host "Choice"
	
	switch($res){
		1 {
			clear;
			try{
				Write-Host "Creating resource group with name : $groupname"
				Write-Host "Storage account : $storageAccountName"
				# Invoke-Expression -Command ".\Create-GRResourceGroup.ps1 $groupname $storageAccountName"
				. ".\Create-GRResourceGroup.ps1" $groupname $storageAccountName
				Write-Host "Fetching storage connection key"
				$storageAccountKey = (Get-AzureStorageAccountKey -ResourceGroupName $resourceGroupName -Name $storageAccountName).Key1
				$rg = Get-AzureResourceGroup -Name $resourceGroupName
				$rg = Out-String -InputObject $rg
				
				$taskComplete = "Successfully created resource group named: $resourceGroupName, and fetched new storage keys. `r`n $rg" 
				$GRCreated = "Y"
			}
			Catch
			{
				$errorMessage = $_.Exception.Message
				$taskComplete = "Failed to create new resource group. `r`n $errorMessage"
				$GRCreated = "Ex"
			}
		}
		2 {
			clear;
			try{
				Write-Host "Packaging cloud services from:"
				Write-Host "$tweetPublisherCsproj into $tweetPublisherOut and"
				Write-Host "$tweetHandlerCsproj into $tweetHandlerOut"
				PackageProject($tweetHandlerOut, $tweetHandlerCsproj)
				PackageProject($tweetPublisherOut, $tweetPublisherCsproj)	
				$taskComplete = "Successfully packaged projects: `r`n $tweetHandlerCsproj into $tweetHandlerOut `r`n $tweetPublisherCsproj into $tweetPublisherOut"
				$PackageCreated = "Y"
			}
			Catch{
				$errorMessage = $_.Exception.Message
				$taskComplete = "Failed to package projects. `r`n $errorMessage"
				$PackageCreated = "Ex"
			}	
		}
		3 {
			clear
			try{
				#Update configs
				Write-Host "Updating configuration files for the cloud service projects"
				$storageAccountKey = (Get-AzureStorageAccountKey -ResourceGroupName $resourceGroupName -Name $storageAccountName).Key1
				UpdateConfigFile "SearchIndexWorker" $storageAccountName $storageAccountKey $tweetHandlerAppdir
				UpdateConfigFile "TweetEventHandler" $storageAccountName $storageAccountKey $tweetHandlerAppdir
				UpdateConfigFile "TweetrPublisher" $storageAccountName $storageAccountKey $tweetPublisherAppDir
				if(!$searchApiKey){
					$searchApiKey = Read-Host "Please enter searchApiKey, accessible from the Azure Portal"
				}
				
				Write-Host "Updating search configuration for SeaerchIndexWorker in TweetHandler service."
				UpdateSearchApiConfig "SearchIndexWorker" $searchApiKey $searchName $tweetHandlerAppdir 
				$tweetPublisher = "TweetPublisher"
				$tweetHandler = "TweetHandler"		
				$taskComplete = "Successfully updated service configuration fir roles: SearchIndexWorker, TweetEventHandler and TweetrPublisher"	
				$ConfigsUpdated ="Y"
			}
			Catch{
				$errorMessage = $_.Exception.Message
				$taskComplete = "Failed to update configs. `r`n $errorMessage"
				$ConfigsUpdated = "Ex"
			}	
		}
		4 {
			clear;
			try{
				if (!$publishsettings){    
					$publishsettings = Get-File "Azure publish settings (*.publishsettings)|*.publishsettings"
					$publishsettings = $publishsettings -replace ' ','` '		
				}
				
				Write-Host "Publishing the Cloud Services."
				$tweetHandlerCspkg = "$tweetHandlerOut$appPublish\TweetHandlerService.cspkg"
				$tweetPublisherCspkg = "$tweetPublisherOut$appPublish\TweetPublishService.cspkg"
				Invoke-Expression -Command ".\Publish-CloudServiceFromPackage.ps1 -groupname $groupname -subscription $subscription -publishsettings $publishsettings -cloudServiceName $groupname$tweetPublisher -config $tweetPublisherOut$appPublish\ServiceConfiguration.Cloud.cscfg -package $tweetPublisherOut$appPublish\TweetPublishService.cspkg"
				Invoke-Expression -Command ".\Publish-CloudServiceFromPackage.ps1 -groupname $groupname -subscription $subscription -publishsettings $publishsettings -cloudServiceName $groupname$tweetHandler -config $tweetHandlerOut$appPublish\ServiceConfiguration.Cloud.cscfg -package $tweetHandlerOut$appPublish\TweetHandlerService.cspkg"
				$taskComplete = "Successfully published projects: TweetHandler and TweetPublisher"
				$ProjectPublished ="Y"
			}
			Catch{
				$errorMessage = $_.Exception.Message
				$taskComplete = "Failed to publish projects. `r`n $errorMessage"
				$ProjectPublished = "Ex"
			}	
		}
		5 {
			
			
		}
		'a' {
			Add-AzureAccount
			$taskComplete = "Azure Account added."
		}
		's' {
			clear
			$i = 1
			$subscriptions = Get-AzureSubscription
			foreach($sub in $subscriptions){$sub | select "[$i]`r", SubscriptionId, SubscriptionName, DefaultAccount | format-table; $i++}
			$c = Read-Host "Select which subscription to use"  
			$subscription = $subscriptions[$c-1].SubscriptionId
			$taskComplete = ""
		}
		"th" {
			Switch-AzureMode AzureServiceManagement
			$deployment = Get-AzureDeployment -ServiceName $tweetHandler -Slot Production -ErrorAction silentlycontinue 
			$deployment = Out-String -InputObject $deployment
			$taskComplete = "Cloud Service - $tweetHandler `r`n $deployment"
		}
		"tp" {
			Switch-AzureMode AzureServiceManagement
			$deployment = Get-AzureDeployment -ServiceName $tweetPublisher -Slot Production -ErrorAction silentlycontinue
			$deployment = Out-String -InputObject $deployment	
			$taskComplete = "Cloud Service - $tweetPublisher `r`n $deployment"			
		}
		'p' {
			Get-AzurePublishSettingsFile
		}
		
	}	
		
}while($res -ne 'q')
exit 0


