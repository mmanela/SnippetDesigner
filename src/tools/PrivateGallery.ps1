<#
.SYNOPSIS
    Update a private gallery for self-hosted Visual Studio extensions.
.DESCRIPTION
    This PowerShell script is called from the post-build event to update the Private Gallery in two steps: 
    1. In the first step the VSIX file is copied into the Private Gallery folder. 
    2. In the second step the ATOM feed of the Private Gallery is updated.
.PARAMETER VsixFilePath 
    The full path to the VSIX file.
.PARAMETER PrivateGalleryFolder 
    The Private Gallery folder.
.EXAMPLE
    "%WINDIR%\System32\WindowsPowerShell\V1.0\powershell.exe" -file "$(SolutionDir)tools\PrivateGallery.ps1" -VsixFilePath "$(TargetDir)$(TargetName).vsix" -PrivateGalleryFolder "%USERPROFILE%\MyGallery"
.LINK 
    https://github.com/chstorb/SnippetDesigner
.NOTES
    1. Specifying build events https://docs.microsoft.com/de-de/visualstudio/ide/specifying-custom-build-events-in-visual-studio?view=vs-2019
    2. Post build event execute PowerShell https://stackoverflow.com/questions/6500320/post-build-event-execute-powershell
#>
[CmdletBinding()]
Param(
    [Parameter(Mandatory=$True, Position=0, HelpMessage="Full patch to the VSIX file.")]
    [string]$VsixFilePath,
    [Parameter(Mandatory=$True, Position=1, HelpMessage="Private Gallery folder.")]
    [string]$PrivateGalleryFolder
)

function CopyVsixFileTo-PrivateGallery
{
    <#
    .SYNOPSIS
        Copy the VSIX file to the Private Gallery folder.
    .PARAMETER VsixFilePath
        Full path to the VSIX file.
    .PARAMETER PrivateGalleryFolder
        Private Gallery folder.
    #>
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True, Position=0, HelpMessage="Full path to the VSIX file.")]
        [string]$VsixFilePath,
        [Parameter(Mandatory=$True, Position=1, HelpMessage="Private Gallery folder.")]
        [string]$PrivateGalleryFolder
    )
    Copy-Item $VsixFilePath -Destination $PrivateGalleryFolder
}

function Update-PrivateGallery
{
    <#
    .SYNOPSIS
        Update a private gallery for self-hosted Visual Studio extensions by using the open source tool Private Gallery Creator.
    .DESCRIPTION
        Private Gallery Creator is used here to create the ATOM feed for your private gallery. The PrivateGalleryCreator executable 
        must be in your private gallery folder, which contains the VSIX files that you want to include in the feed.
    .PARAMETER PrivateGalleryFolder The Private Gallery folder.
    .LINK 
        https://devblogs.microsoft.com/visualstudio/create-a-private-gallery-for-self-hosted-visual-studio-extensions/
    .NOTES
        How to run an EXE file in PowerShell with parameters with spaces and quotes
        https://stackoverflow.com/questions/1673967/how-to-run-an-exe-file-in-powershell-with-parameters-with-spaces-and-quotes
    #>
    [CmdletBinding()]
    Param(
        [Parameter(Mandatory=$True, Position=1, HelpMessage="Private Gallery folder.")]
        [string]$PrivateGalleryFolder
    )
    # Let PowerShell interpret the string as a command name by using the call operator (&)
    & "$($PrivateGalleryFolder)\PrivateGalleryCreator.exe" ("--terminate", "--latest-only")
}

if (!(Test-Path $PrivateGalleryFolder))
{
    Write-Host "The path '$($PrivateGalleryFolder)' does not exist."
}
else
{
    CopyVsixFileTo-PrivateGallery -VsixFilePath $VsixFilePath -PrivateGalleryFolder $PrivateGalleryFolder
    Update-PrivateGallery -PrivateGalleryFolder $PrivateGalleryFolder
}
