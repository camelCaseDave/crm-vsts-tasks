> Tasks to automate Dynamics 365 post-deployment steps

## 1. Tasks

### 1.1 Data transfer
Transfer configuration data between environments. Code adapted from [ConfigDataMover](https://github.com/lucasalexander/AlexanderDevelopment.ConfigDataMover).

#### 1.1.1 Export to file
Runs a given fetchXml query to retrieve data from CRM and store it as `json` at a given directory.

#### 1.1.2 Import to CRM
Reads data as `json` from a given directory and imports it into CRM.

### 1.2 Duplicate detection rules
Establish which duplicate detection rules are published before a deployment, and then republish them after a deployment.

#### 1.2.1 Get published duplicate detection rules
Gets all published duplicate detection rules in CRM and stores them as a `json` file.

#### 1.2.2 Publish duplicate detection rules
Reads a `json` file to determine which duplicate detection rules should be republished and republishes them.

### 1.3 Deactivate forms
Deactivates all forms in CRM matching any of the ids given as a comma separated string.

### 1.4 Update SLA
Conditionally sets an SLA as being active and/or default. Can also retrieve business hours (calendar) and assign it to the SLA.

### 1.5 Assign workflows to user
Assigns all workflows in CRM to a given user.

## 2. Usage

- Compile code in release mode.
- Create an empty release task. See [Microsoft HowTo](https://docs.microsoft.com/en-us/vsts/extend/develop/add-build-task).

How you execute the VstsExtensions.dll is your choice. This example uses PowerShell to deploy the task  _"Export to file"_:

- In your tasks folder, add `ExportToFile.ps1`:

```ps
[CmdletBinding(DefaultParameterSetName = 'None')]
param (
    [String] [Parameter(Mandatory = $true)] 
    $Connection,

    [String] [Parameter(Mandatory = $true)]
    $WorkingDirectory,

    [String] [Parameter(Mandatory = $true)]
    $FetchXml
)

$ScriptPath = Split-Path -parent $MyInvocation.MyCommand.Definition
$ExtensionsPath = $ScriptPath + "\VstsExtensions.dll"
$ExtensionsCorePath = $ScriptPath + "\VstsExtensions.Core.dll"

Write-Verbose "Entering ExportToFile.ps1"
Write-Verbose "Connection is $Connection"
Write-Verbose "Working direcory is $WorkingDirectory"

Get-ChildItem path -recurse

Add-Type -Path $ExtensionsPath
Add-Type -Path $ExtensionsCorePath

$ExportToFile = New-Object -TypeName VstsExtensions.ExportToFile -ArgumentList $Connection, $WorkingDirectory, $FetchXml
$ExportToFile.Run()

Write-Verbose "Leaving ExportToFile.ps1"
```

- In your `task.json`, specify input parameters:
```js
"inputs": [
    {
        "name": "Connection",
        "type": "string",
        "label": "Connection",
        "defaultValue": "",
        "required": true,
        "helpMarkDown": "CRM connection string"
    },
    {
        "name": "WorkingDirectory",
        "type": "string",
        "label": "Working Directory",
        "defaultValue": "",
        "required": true,
        "helpMarkDown": "Default working directory. Must match the directory used to read from when importing data to CRM."
    },
    {
        "name": "FetchXml",
        "type": "string",
        "label": "FetchXml",
        "defaultValue": "",
        "required": true,
        "helpMarkDown": "Query used to read data from CRM."
    }
]
```
Also specify your execution:
```js
"execution": {
    "PowerShell": {
        "target": "$(currentDirectory)\\ExportToFile.ps1",
        "workingDirectory": "$(currentDirectory)"
    }
}
```

- Run your script locally to confirm it's working.
- Build your VSTS task with `tfx extension create --manifest-globs vss-extension.json`. It will compile to a .tfix file.
- Publish your extension to VSTS. See [Microsoft HowTo](https://docs.microsoft.com/en-us/vsts/extend/publish/overview).
