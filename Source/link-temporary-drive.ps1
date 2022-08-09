# Parameters to file
param(
    [Parameter(Position=0,mandatory=$true)]
    [string] $DiskDrive,
    [Parameter(Position=1)]
    [bool] $LinkObjFolder=$true,
    [Parameter(Position=2)]
    [bool] $LinkBinFolder=$false
)

# If neither LinkBinFolder nor LinkObjFolder is specified, return with an error
if ($LinkBinFolder -eq $false -and $LinkObjFolder -eq $false)
{
    Write-Error "Neither LinkBinFolder and LinkObjFolder are specified"
    exit 1
}

# Find all project files for CSharp, FSharp and Basic
$projectFiles = Get-ChildItem -Include ("*.csproj", "*.fsproj", "*.vbproj") -Recurse 

# Setup directories if any projects found.
if($projectFiles.Length -gt 0)
{
    # Get project directories
    $projectDirectories = $projectFiles | ForEach-Object { $_.DirectoryName } | Get-Unique

    if ($LinkObjFolder -eq $true)
    {
        # Create a obj-directory on the drive
        $projectDirectories | ForEach-Object { New-Item -ItemType Directory -Force -Path "$($DiskDrive):$($_.Substring(2))\obj" } 

        # Remove existing obj-directories
        $projectDirectories | ForEach-Object {

            $objDirectory = Join-Path -Path $_ -ChildPath "obj"
            if(Test-Path $objDirectory)
            {
                # Remove-Item -Path $objDirectory -Force -Recurse
                $projectDirectories | ForEach-Object { cmd /c rmdir "$objDirectory" }
            }
        }
   
        # Link obj-directories to drive
        $projectDirectories | ForEach-Object { cmd /c mklink /D "$($_)\obj" "$($DiskDrive):$($_.Substring(2))\obj" }
    }

    if ($LinkBinFolder -eq $true)
    {
        # Create a bin-directory on the drive
        $projectDirectories | ForEach-Object { New-Item -ItemType Directory -Force -Path "$($DiskDrive):$($_.Substring(2))\bin" } 

        # Remove existing bin-directories
        $projectDirectories | ForEach-Object {

            $binDirectory = Join-Path -Path $_ -ChildPath "bin"
            if(Test-Path $binDirectory)
            {
                # Remove-Item -Path $binDirectory -Force -Recurse
                $projectDirectories | ForEach-Object { cmd /c rmdir "$binDirectory" }
            }
        }

        # Link bin-directories to drive
        $projectDirectories | ForEach-Object { cmd /c mklink /D "$($_)\bin" "$($DiskDrive):$($_.Substring(2))\bin" }
    }
}
