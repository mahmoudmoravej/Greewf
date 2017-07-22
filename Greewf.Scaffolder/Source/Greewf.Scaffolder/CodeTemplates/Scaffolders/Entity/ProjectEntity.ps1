[T4Scaffolding.Scaffolder(Description = "complete scaffolder for entity")][CmdletBinding()]
param(
[Parameter(Mandatory=$true, ValueFromPipelineByPropertyName=$true)]$ModelType,
[string]$PrimaryKey=$null,
[String]$ProjectCompanyName,
[String]$ProjectMainName,
[switch]$Force = $false,
[String]$Area = $null
)
[String] $ProjectDefaultNamespace = "$ProjectCompanyName.$ProjectMainName"

scaffold entity $ModelType `
-PrimaryKey $PrimaryKey `
-DbContextType "$ProjectDefaultNamespace.Dal.$($ProjectMainName)Context" `
-RepositoryProject "31-$ProjectMainName.Biz" `
-RepositoryInterfaceProject "31-$ProjectMainName.Biz" `
-WebProject:"50-$ProjectMainName.UI.Web" `
-EntityProjectName:"30-$ProjectMainName.Biz.Entities" `
-PermissionNameSpaceName "$ProjectDefaultNamespace.Biz.Entities.Enums.Permissions" `
-AutoMapNamespace "Greewf.BaseLibrary.MVC.Mappers" `
-CustomizedControllerBaseNamespace "Greewf.BaseLibrary.MVC" `
-CustomHelperNamespace "Greewf.BaseLibrary.MVC.CustomHelpers" `
-LoggingNamespace "Greewf.BaseLibrary.MVC.Logging" `
-LogAttributeNamespace "$ProjectDefaultNamespace.UI.Web.Logs" `
-Area:$Area `
-Force:$Force