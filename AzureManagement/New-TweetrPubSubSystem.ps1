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
	$grouptag = $groupname	
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

$handlerProjPath = "..\TweetHandlerService\TweetHandlerService.ccproj"
$publisherProjPath = "..\TweetPublishService\TweetPublishService.ccproj"
$currentDir = (Get-Item -Path ".\" -Verbose).FullName
$handlerOutpath = "$currentDir\HandlerOut\"	
$publisherOutpath = "$currentDir\PublisherOut\"	
	
try{	
	Write-Host "Creating new Azure resource group - $resourceGroupName `n Using template $templateFile"
	. .\Create-GRResourceGroup -groupname $groupname
}Catch{
	$errorMessage = $_.Exception.Message
	$errorMessage
}																										   


try{
	Write-Host "Packaging handler project"						
	. .\Package-CloudServiceProject -csprojpath $handlerProjPath -out $handlerOutpath
					
}Catch{
	$errorMessage = $_.Exception.Message
	$errorMessage
}

try{
	Write-Host "Packaging publish project"
	. .\Package-CloudServiceProject -csprojpath $publisherProjPath -out $publisherOutpath	
					
}Catch{
	$errorMessage = $_.Exception.Message
	$errorMessage
}



$publishDirHandler = "$currentDir\HandlerOut\app.publish"
$publishDirPublisher = "$currentDir\PublisherOut\app.publish"
	
try{
	Write-Host "1"
	. .\Set-ProjectCloudConfigurations.ps1 -groupname $grouptag -publishDir $publishDirHandler -workerName "TweetHandler" -storageAccountName $storageAccountName
	Write-Host "1"
	
	. .\Set-ProjectCloudConfigurations.ps1 -groupname $grouptag -publishDir $publishDirHandler -workerName "SearchIndexWorker" -storageAccountName $storageAccountName -searchName $searchName -s
	Write-Host "1"
	
	. .\Set-ProjectCloudConfigurations.ps1 -groupname $grouptag -publishDir $publishDirPublisher -workerName "TweetrPublisher" -storageAccountName $storageAccountName
}Catch{
	$errorMessage = $_.Exception.Message
	$errorMessage
}

