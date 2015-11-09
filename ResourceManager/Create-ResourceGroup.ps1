Param(
	[Parameter(Mandatory=$true)]
	[string]$prefix
	)

Switch-AzureMode -Name AzureResourceManager

New-AzureResourceGroup -Name $prefix"GeekRetreatResources" -Location "North Europe" -TemplateFile .\geekretreat.json -tweetrCloudServiceName $prefix"grtweetr" -tweetHandlerCloudServiceName $prefix"grtweethandler" -serviceBusName $prefix"grsbus" -tweetrStorageAccountName $prefix"grtweetrs" -tweetHandlerStorageAccountName $prefix"grtweethandlers" -searchName $prefix"grsearch"
