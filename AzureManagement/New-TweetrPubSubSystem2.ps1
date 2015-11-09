[CmdletBinding()]
Param(
	[Parameter(Position=1)]
	[string]$groupname,
	[string]$resourceGroupName,
	[string]$tweetPublishServiceName,
	[string]$tweetHandlerServiceName,
	[string]$storageAccountName,
	[string]$deploymentStorageName,
	[string]$searchName,
	[string]$templateFile = ".\tweet-publish-subscribe.json"

)

if (!$groupname){
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
$hostingPlanName = $groupname + "HostingPlan"
$websiteName = $groupname + "Web"

$publisherProjPath = "..\TweetHandlerService\TweetHandlerService.ccproj"
$handlerProjPath = "..\TweetPublishService\TweetPublishService.ccproj"
$currentDir = (Get-Item -Path ".\" -Verbose).FullName
$handlerOutpath = "$currentDir\HandlerOut\"	
$publisherOutpath = "$currentDir\PublisherOut\"	
	
# try{	
# 	Write-Host "Creating new Azure resource group - $resourceGroupName `n Using template $templateFile"
# 	Switch-AzureMode -Name AzureResourceManager
# 	New-AzureResourceGroup -ErrorAction Stop -Name $resourceGroupName -Location "North Europe" -TemplateFile $templateFile `
# 													-tweetPublishServiceName $tweetPublishServiceName `
# 													-tweetHandlerServiceName $tweetHandlerServiceName `
# 													-tweetHandlerStorageAccountName $storageAccountName `
# 													-searchName $searchName `
# 													-deploymentStorageName $deploymentStorageName `
# 													-hostingPlanName $hostingPlanName `
# 													-websiteName $websiteName
# }Catch{
# 	$errorMessage = $_.Exception.Message
# 	$errorMessage
# }																										   

try{
	Write-Host "Packaging handler project"						
	msbuild $handlerProjPath /p:Configuration=Release `
									/p:DebugType=None `
									/p:Platform=AnyCpu `
									/p:OutputPath=$handlerOutpath `
									/p:TargetProfile=Cloud `
									/t:publish  	
					
}Catch{
	$errorMessage = $_.Exception.Message
	$errorMessage
}

try{
	Write-Host "Packaging publish project"
	msbuild $publisherProjPath /p:Configuration=Release `
									/p:DebugType=None `
									/p:Platform=AnyCpu `
									/p:OutputPath=$publisherOutpath `
									/p:TargetProfile=Cloud `
									/t:publish  	
					
}Catch{
	$errorMessage = $_.Exception.Message
	$errorMessage
}
