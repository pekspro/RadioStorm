# Find all project files
$projectFiles = Get-ChildItem -Include ("*.csproj", "*.fsproj", "*.vbproj") -Recurse 

# Get project directories
$projectDirectories = $projectFiles | ForEach-Object { $_.DirectoryName } | Get-Unique

# Clear directories if any projects found.
if($projectFiles.Length -gt 0)
{
    # Unlink obj-directories to drive by removing them from the project directories.
    $projectDirectories | ForEach-Object {

        $objDirectory = Join-Path -Path $_ -ChildPath "obj"
        if(Test-Path $objDirectory)
        {
            # Remove-Item -Path $objDirectory -Force -Recurse
            cmd /c rmdir "$objDirectory" /s /q
        }
    }

    # Unlink bin-directories to drive by removing them from the project directories.
    $projectDirectories | ForEach-Object {

        $binDirectory = Join-Path -Path $_ -ChildPath "bin"
        if(Test-Path $binDirectory)
        {
            # Remove-Item -Path $binDirectory -Force -Recurse
            cmd /c rmdir "$binDirectory" /s /q
        }
    }
}
