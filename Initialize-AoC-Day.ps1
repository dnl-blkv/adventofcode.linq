param (
    [Parameter(Mandatory=$True)]
    [int]$Year,

    [Parameter(Mandatory=$True)]
    [int]$Day
)

$TemplateFilePath = ".\AoC_Template.linq"

if (-not (Test-Path -Path $TemplateFilePath))
{
    Write-Error "Template file $TemplateFilePath not found."
    exit
}

$FolderPath = ".\${year}_AdventOfCode"
$DayFilePath = "$FolderPath\Day_${day}.linq"
$InputFilePath = "$FolderPath\Day_${day}_Input.txt"

if (Test-Path -Path $DayFilePath)
{
    Write-Error "The file $DayFilePath already exists."
    exit
}

if (Test-Path -Path $InputFilePath)
{
    Write-Error "The file $InputFilePath already exists."
    exit
}

New-Item -ItemType Directory -Path $FolderPath -Force
Copy-Item -Path $TemplateFilePath -Destination $DayFilePath
New-Item -Path $InputFilePath -ItemType File

$DayUrl = "https://adventofcode.com/$Year/day/$Day"
Start-Process $DayUrl

$InputUrl = "$DayUrl/input"
Start-Process $InputUrl