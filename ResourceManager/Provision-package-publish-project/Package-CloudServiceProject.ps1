[CmdletBinding()]
Param(
    [Parameter(Mandatory=$True,Position=1)]
    [string]$csprojpath,
    [string]$outpath = (Get-Item -Path ".\" -Verbose).FullName

)

msbuild $csprojpath /p:Configuration=Release `
                                /p:DebugType=None `
                                /p:Platform=AnyCpu `
                                /p:OutputPath=$outpath"\appack\" `
                                /p:TargetProfile=Cloud `
                                /t:publish  