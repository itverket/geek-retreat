[CmdletBinding()]
Param(
	[string]$resourceGroupName,
	[string]$tweetPublishServiceName,
	[string]$tweetHandlerServiceName,
	[Parameter(Position=2)]
	[string]$storageAccountName,
	[string]$deploymentStorageName,
	[string]$searchName,
	[Parameter(Position=1)]
	[string]$groupname
)

if (!$groupName){
	$groupname = Read-Host "Enter group number or short group name (max 5 characters!)" 	
}
	 
if($groupname.Length -lt 3){
	$groupname = "GRGroup" + $groupname
}     
$resourceGroupName = $groupname + "Resources"
$tweetPublishServiceName = $groupname + "TweetPublisher"
$tweetHandlerServiceName = $groupname + "TweetHandler"
$searchName = $groupname.ToLower() + "search"
$storageAccountName = $groupname.ToLower() + "storage"
$deploymentStorageName = $groupname.ToLower() + "deployments"

if (!$resourceGroupName){    
	$resourceGroupName = Read-Host "Resource group name, i.e. GeekRetreatGroupX"
	$deploymentStorageName = $resourceGroupName.ToLower() + "deploy"
}
if (!$tweetPublishServiceName){    
	$tweetPublishServiceName = Read-Host "Tweet publish service name, i.e. tweetPublisherGroupX. (Must be unique)"
}
if (!$tweetHandlerServiceName){    
	$tweetHandlerServiceName = Read-Host "Tweet handler name, i.e. tweetEventHandlerGroupX. (Must be unique)"
}
if (!$storageAccountName){    
	$storageAccountName = Read-Host "Tweet storage account name, i.e. tweetstoragegroupx. (Must be unique, and only contain lower case letters)"
}
if (!$searchName){    
	$searchName = Read-Host "Search name,  i.e. groupxsearch. (Must be unique, only contain lower case letters and not be longer than 15 characters)"
}

Switch-AzureMode -Name AzureResourceManager

New-AzureResourceGroup -ErrorAction Stop -Name $resourceGroupName -Location "North Europe" -TemplateFile .\tweet-publish-subscribe.json `
																		   -tweetPublishServiceName $tweetPublishServiceName `
																		   -tweetHandlerServiceName $tweetHandlerServiceName `
																		   -tweetHandlerStorageAccountName $storageAccountName `
																		   -searchName $searchName `
																		   -deploymentStorageName $deploymentStorageName


