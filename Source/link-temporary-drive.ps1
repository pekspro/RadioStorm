$ramDiskDrive = "D:"

# Find all project files for CSharp, FSharp and Basic
$projectFiles = Get-ChildItem -Include ("*.csproj", "*.fsproj", "*.vbproj") -Recurse 

# Setup directories if any projects found.
if($projectFiles.Length -gt 0)
{
    # Get project directories
    $projectDirectories = $projectFiles | ForEach-Object { $_.DirectoryName } | Get-Unique


    # Create a bin-directory on the RAM-drive
    $projectDirectories | ForEach-Object { New-Item -ItemType Directory -Force -Path "$ramDiskDrive$($_.Substring(2))\bin" } 

    # Remove existing bin-directories
    $projectDirectories | ForEach-Object { Remove-Item "$($_)\bin" -Force -Recurse }

    # Link bin-directories to ramdisk
    $projectDirectories | ForEach-Object { cmd /c mklink /D "$($_)\bin" "$ramDiskDrive$($_.Substring(2))\bin" }


    # Create a obj-directory on the RAM-drive
    $projectDirectories | ForEach-Object { New-Item -ItemType Directory -Force -Path "$ramDiskDrive$($_.Substring(2))\obj" } 

    # Remove existing obj-directories
    $projectDirectories | ForEach-Object { Remove-Item "$($_)\obj" -Force -Recurse }

    # Link obj-directories to ramdisk
    $projectDirectories | ForEach-Object { cmd /c mklink /D "$($_)\obj" "$ramDiskDrive$($_.Substring(2))\obj" }
}


# Find all projects files for C++.
$projectFiles = Get-ChildItem -Include ("*.vcxproj") -Recurse 

# Setup C++-directories is any found.
if($projectFiles.Length -gt 0)
{
    # Get project directories
    $projectDirectories = $projectFiles | ForEach-Object { $_.DirectoryName } | Get-Unique


    $projectDirectories | ForEach-Object { New-Item -ItemType Directory -Force -Path "$ramDiskDrive$($_.Substring(2))\x64" } 

    # Remove existing x64-directories
    $projectDirectories | ForEach-Object { Remove-Item "$($_)\x64" -Force -Recurse }

    # Link x64-directories to ramdisk
    $projectDirectories | ForEach-Object { cmd /c mklink /D "$($_)\x64" "$ramDiskDrive$($_.Substring(2))\x64" }


    $projectDirectories | ForEach-Object { New-Item -ItemType Directory -Force -Path "$ramDiskDrive$($_.Substring(2))\Debug" } 

    # Remove existing Debug-directories
    $projectDirectories | ForEach-Object { Remove-Item "$($_)\Debug" -Force -Recurse }

    # Link Debug-directories to ramdisk
    $projectDirectories | ForEach-Object { cmd /c mklink /D "$($_)\Debug" "$ramDiskDrive$($_.Substring(2))\Debug" }


    $projectDirectories | ForEach-Object { New-Item -ItemType Directory -Force -Path "$ramDiskDrive$($_.Substring(2))\Release" } 

    # Remove existing Release-directories
    $projectDirectories | ForEach-Object { Remove-Item "$($_)\Release" -Force -Recurse }

    # Link Release-directories to ramdisk
    $projectDirectories | ForEach-Object { cmd /c mklink /D "$($_)\Release" "$ramDiskDrive$($_.Substring(2))\Release" }



    # For C++-project, directories in the same folder as the solution file are used for outputs.
    # Find all solution files.
    $projectFiles = Get-ChildItem -Include ("*.sln") -Recurse 

    # Setup C++-directories is any solution file found.
    if($projectFiles.Length -gt 0)
    {
        # Get project directories
        $projectDirectories = $projectFiles | ForEach-Object { $_.DirectoryName } | Get-Unique


        $projectDirectories | ForEach-Object { New-Item -ItemType Directory -Force -Path "$ramDiskDrive$($_.Substring(2))\x64" } 

        # Remove existing x64-directories
        $projectDirectories | ForEach-Object { Remove-Item "$($_)\x64" -Force -Recurse }

        # Link x64-directories to ramdisk
        $projectDirectories | ForEach-Object { cmd /c mklink /D "$($_)\x64" "$ramDiskDrive$($_.Substring(2))\x64" }


        $projectDirectories | ForEach-Object { New-Item -ItemType Directory -Force -Path "$ramDiskDrive$($_.Substring(2))\Debug" }

        # Remove existing Debug-directories
        $projectDirectories | ForEach-Object { Remove-Item "$($_)\Debug" -Force -Recurse }

        # Link Debug-directories to ramdisk
        $projectDirectories | ForEach-Object { cmd /c mklink /D "$($_)\Debug" "$ramDiskDrive$($_.Substring(2))\Debug" }


        $projectDirectories | ForEach-Object { New-Item -ItemType Directory -Force -Path "$ramDiskDrive$($_.Substring(2))\Release" } 

        # Remove existing Release-directories
        $projectDirectories | ForEach-Object { Remove-Item "$($_)\Release" -Force -Recurse }

        # Link Release-directories to ramdisk
        $projectDirectories | ForEach-Object { cmd /c mklink /D "$($_)\Release" "$ramDiskDrive$($_.Substring(2))\Release" }
    }
}