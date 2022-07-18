# Find all project files
$projectFiles = Get-ChildItem -Include ("*.csproj", "*.fsproj", "*.vbproj") -Recurse 

# Get project directories
$projectDirectories = $projectFiles | ForEach-Object { $_.DirectoryName } | Get-Unique

# Clear directories if any projects found.
if($projectFiles.Length -gt 0)
{
    # Unlink bin-directories to ramdisk
    $projectDirectories | ForEach-Object { cmd /c rmdir "$($_)\bin"   }

    # Unlink obj-directories to ramdisk
    $projectDirectories | ForEach-Object { cmd /c rmdir "$($_)\obj"   }
}

# Find all projects files for C++.
$projectFiles = Get-ChildItem -Include ("*.vcxproj") -Recurse 

# Setup C++-directories is any found.
if($projectFiles.Length -gt 0)
{
    # Get project directories
    $projectDirectories = $projectFiles | ForEach-Object { $_.DirectoryName } | Get-Unique


    # Unlink x64-directories to ramdisk
    $projectDirectories | ForEach-Object { cmd /c rmdir "$($_)\x64"   }

    # Unlink Debug-directories to ramdisk
    $projectDirectories | ForEach-Object { cmd /c rmdir "$($_)\Debug"   }

    # Unlink Release-directories to ramdisk
    $projectDirectories | ForEach-Object { cmd /c rmdir "$($_)\Release"   }


    # For C++-project, directories in the same folder as the solution file are used for outputs.
    # Find all solution files.
    $projectFiles = Get-ChildItem -Include ("*.sln") -Recurse 

    # Setup C++-directories is any solution file found.
    if($projectFiles.Length -gt 0)
    {
        # Get project directories
        $projectDirectories = $projectFiles | ForEach-Object { $_.DirectoryName } | Get-Unique


        # Unlink x64-directories to ramdisk
        $projectDirectories | ForEach-Object { cmd /c rmdir "$($_)\x64"   }

        # Unlink Debug-directories to ramdisk
        $projectDirectories | ForEach-Object { cmd /c rmdir "$($_)\Debug"   }

        # Unlink Release-directories to ramdisk
        $projectDirectories | ForEach-Object { cmd /c rmdir "$($_)\Release"   }
    }
}
