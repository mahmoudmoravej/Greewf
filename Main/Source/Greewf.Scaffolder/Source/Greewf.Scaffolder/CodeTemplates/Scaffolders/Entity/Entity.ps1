[T4Scaffolding.Scaffolder(Description = "complete scaffolder for entity")][CmdletBinding()]
param(        
	[Parameter(Mandatory=$true, ValueFromPipelineByPropertyName=$true)]$ModelType,
	[string]$PrimaryKey=$null,
	[string]$DbContextType,
	[string]$CodeLanguage,
	[string]$RepositoryProject,
	[string]$RepositoryInterfaceProject,
	[string]$RepositoryFolder='Repositories',
	[string]$RepositoryInterfaceFolder='RepositoryInterfaces',
	[string]$WebProject,
	[string]$ModelFolder='Models',
	[string]$EntityProjectName,
	[string]$PermissionEntityFileName='Enums\Permissions.cs',
	[string]$PermissionEntityEnumName='PermissionEntity',
	[string]$PermissionNameSpaceName,
	[string]$PermissionCoordinatorFileName = 'PermissionCoordinator.cs',
	[string]$PermissionCoordinatorClassName = 'PermissionCoordinator',
	[string]$PermissionCoordinatorLoaderFunctionName='LoadPermissionRelationships',
	[string]$PermissionAttributeClass = 'PermissionsAttribute',
	[string]$ViewFolder = 'Views',
	[string]$ControllerSubNamespace = 'Controllers',	
	[string]$CustomizedControllerClassName = 'CustomizedController',
	[string]$AutoMapNamespace,
	[string]$CustomHelperNamespace,
	[string]$CustomizedControllerBaseNamespace,
	[string]$LoggingNamespace,
	[string]$LogAttributeNamespace,
	[string]$LogPointsXmlFile='\Logs\LogPoints.xml',
	[string[]]$TemplateFolders,
	[string]$Area=$null,
	[switch]$Force = $false
)

#0 : making ready
$SubViewModelNameSpace='Models'
$ViewModelNameSpace = [T4Scaffolding.Namespaces]::Normalize((Get-Project $WebProject).Properties.Item("DefaultNamespace").Value + "." + $SubViewModelNameSpace)
$ViewType = $null
if ($Area -ne $null -and $Area -ne '')
{
	$Area = $Area.Trim('\').Trim('/')
	$Area = "Areas\$Area\"
}
else
{
	$Area =''
}

[string]$path = (Get-Project).properties.item('LocalPath').value
$path=$path+"\CodeTemplates\Scaffolders"

. "$path\Entity\Utilities.ps1"

$TemplateFolders=$TemplateFolders+"$path\T4Scaffolding.EFRepository" 
$TemplateFolders=$TemplateFolders+"$path\MvcScaffolding.Controller" 
$TemplateFolders=$TemplateFolders+"$path\MvcScaffolding.RazorView" 

#0 : make common properties ready
$foundModelType = Get-ProjectType $ModelType -Project $WebProject -BlockUi #it can get it from $WebProject becuase of added refrence 
$modelTypePluralized = Get-PluralizedWord $foundModelType.Name
$relatedEntities = [Array](Get-RelatedEntities $foundModelType.FullName -Project $WebProject) #it can get it from $WebProject becuase of added refrence 
if (!$relatedEntities) { $relatedEntities = @() }
$DefaultImportingNamespaces = @($PermissionNameSpaceName,(Get-Project $RepositoryProject).Properties.Item("DefaultNamespace").Value,(Get-Project $RepositoryInterfaceProject).Properties.Item("DefaultNamespace").Value,$ViewModelNameSpace,$AutoMapNamespace,$CustomizedControllerBaseNamespace,$CustomHelperNamespace,$LoggingNamespace,$LogAttributeNamespace)


#1st: Controller 
#1-1 : Entity Controller
ApplyTemplate -TemplateFileName "ControllerWithRepository" -Project:$WebProject -OutputPath:"$($Area)Controllers" -OutputFilePostName:'Controller' -OutputFilePreName:'' -RepositoryProject:$RepositoryProject -SubRepositoryNameSpace:'Repositories' -RepositoryInterfaceProject:$RepositoryInterfaceProject -SubRepositoryInterfaceNameSpace:'RepositoryInterfaces' -WebProject:$WebProject -SubViewModelNameSpace:$SubViewModelNameSpace -SubViewModelMetaDataNameSpace:'Models.MetaData' -ControllerSubNamespace:$ControllerSubNamespace -UsePluralNameInFileName -DefaultImportingNamespaces:$DefaultImportingNamespaces
$DefaultImportingNamespaces = $null

#1-2: Adding  entityRepositories to CustomizedController
Write-Host "Scaffolding CustomizedController Modifications..." -ForegroundColor blue
$e = Get-ProjectType $CustomizedControllerClassName -Project $WebProject  -BlockUi
if($e)
{
	$blocStartPoint = $e.GetStartPoint(16).CreateEditPoint()#16 = EnvDTE.vsCMPart.vsCMPartBody
	$blocEndPoint = $e.GetEndPoint(16).CreateEditPoint()	#16 = EnvDTE.vsCMPart.vsCMPartBody
	$templateFile = GetTemplateFile -templateFileName:"CustomizedController"
	
	$allNeededEntities =$relatedEntities+$foundModelType #to generate property for current repository too
	foreach ($relatedEntity in $allNeededEntities) 
	{
		$o = Invoke-ScaffoldTemplate -Template:$templateFile  -Model @{RelatedEntity=$relatedEntity} 
		
		if($o.Trim() -eq "") 
		{
			continue
		}		
		$name = $relatedEntity.RelatedEntityType.Name
		if ($name -eq $null)#when the item is $foundModelType
		{
			$name = $relatedEntity.Name
		}
		$name=$name+"Repository"
		
		if(-not($blocEndPoint.GetText($blocStartPoint).Contains($o.Trim()))) 
		{
			$blocEndPoint.Insert($o)
			Write-Host "New $name property added to $CustomizedControllerClassName class..." 
		}
		else
		{
			Write-Host "$CustomizedControllerClassName already has a property for '$name'! Passed...(-Force not works on this currently)" -ForegroundColor:Magenta
		}
	}	
}
else
{
	Write-Host "$CustomizedControllerClassName not found!!..." -ForegroundColor:Red
}

#1-3:adding Controller.DoCustomMapping() function call to Global.asax.cs file
$controllerName = "$($modelTypePluralized)Controller"
$e=Get-CodeItem -fileName: "Global.asax.cs" -projectName:$WebProject -typeName:"MvcApplication" -typeCode:1 #vsCMElement.vsCMElementClass = 1
if($e)
{
	$customMappingCall = Find-CodeItem -obj:$e -typeCode:2 -typeName: "DoCustomMAppings" #vsCMElementFunction = 2 (function)
	$blockEndPoint = $customMappingCall.GetEndPoint(16).CreateEditPoint()
	$o = ApplyTemplate -TemplateFileName "DoCustomMappings" -Project:$WebProject  -IsOutputText -RepositoryProject:$RepositoryProject -SubRepositoryNameSpace:'Repositories' -RepositoryInterfaceProject:$RepositoryInterfaceProject -SubRepositoryInterfaceNameSpace:'RepositoryInterfaces' -WebProject:$WebProject -SubViewModelNameSpace:$SubViewModelNameSpace -SubViewModelMetaDataNameSpace:'Models.MetaData' -ControllerSubNamespace:$ControllerSubNamespace -DefaultImportingNamespaces:$DefaultImportingNamespaces	

	if(-not($blockEndPoint.GetText($customMappingCall.GetStartPoint(16)).Contains($controllerName))) #16 = EnvDTE.vsCMPart.vsCMPartBody
	{
		$blockEndPoint.Insert($o)
		Write-Host "'$($controllerName).DoCustomMapping();' call added to MvcApplication.DoCustomMapping() function in Global.asax.cs file..." 
	}
	else
	{
		Write-Host "MvcApplication already has some call to '$($controllerName).DoCustomMappings();'! Passed...(-Force not works on this currently)" -ForegroundColor:Magenta
		Write-Host "you can add it manually :" -ForegroundColor:Magenta
		Write-Host "$o" -ForegroundColor:Magenta		
	}	
}
else
{
	Write-Host "MvcApplication not found in file Global.asax.cs of project:$WebProject!!..." -ForegroundColor:Red
}


#2nd: ViewModels
$GridFolder = "$($Area)$ModelFolder\"+$foundModelType.Name
$MetaDataFolder = "$($Area)$ModelFolder\"+$foundModelType.Name
$ViewModelFolder = "$($Area)$ModelFolder\"+$foundModelType.Name
$SearchCriteria = "$($Area)$ModelFolder\"+$foundModelType.Name

Write-Host "Scaffolding ViewModel Classes..." -ForegroundColor blue
ApplyTemplate -TemplateFileName "ViewModel" -Project:$WebProject -OutputPath:$ViewModelFolder -OutputFilePostName:'ViewModel' -OutputFilePreName:'' -RepositoryProject:$RepositoryProject -SubRepositoryNameSpace:'Repositories' -RepositoryInterfaceProject:$RepositoryInterfaceProject -SubRepositoryInterfaceNameSpace:'RepositoryInterfaces' -WebProject:$WebProject -SubViewModelNameSpace:$SubViewModelNameSpace -SubViewModelMetaDataNameSpace:'Models.MetaData' -ControllerSubNamespace:$ControllerSubNamespace -DefaultImportingNamespaces:$DefaultImportingNamespaces
ApplyTemplate -TemplateFileName "ViewModelMetaData" -Project:$WebProject -OutputPath:$MetaDataFolder -OutputFilePostName:'MetaData' -OutputFilePreName:'' -RepositoryProject:$RepositoryProject -SubRepositoryNameSpace:'Repositories' -RepositoryInterfaceProject:$RepositoryInterfaceProject -SubRepositoryInterfaceNameSpace:'RepositoryInterfaces' -WebProject:$WebProject -SubViewModelNameSpace:$SubViewModelNameSpace -SubViewModelMetaDataNameSpace:'Models.MetaData' -ControllerSubNamespace:$ControllerSubNamespace -DefaultImportingNamespaces:$DefaultImportingNamespaces
ApplyTemplate -TemplateFileName "GridViewModel" -Project:$WebProject -OutputPath:$GridFolder -OutputFilePostName:'GridViewModel' -OutputFilePreName:'' -RepositoryProject:$RepositoryProject -SubRepositoryNameSpace:'Repositories' -RepositoryInterfaceProject:$RepositoryInterfaceProject -SubRepositoryInterfaceNameSpace:'RepositoryInterfaces' -WebProject:$WebProject -SubViewModelNameSpace:$SubViewModelNameSpace -SubViewModelMetaDataNameSpace:'Models.MetaData' -ControllerSubNamespace:$ControllerSubNamespace -DefaultImportingNamespaces:$DefaultImportingNamespaces
ApplyTemplate -TemplateFileName "SearchCriteria" -Project:$WebProject -OutputPath:$SearchCriteria -OutputFilePostName:'SearchCriteria' -OutputFilePreName:'' -RepositoryProject:$RepositoryProject -SubRepositoryNameSpace:'Repositories' -RepositoryInterfaceProject:$RepositoryInterfaceProject -SubRepositoryInterfaceNameSpace:'RepositoryInterfaces' -WebProject:$WebProject -SubViewModelNameSpace:$SubViewModelNameSpace -SubViewModelMetaDataNameSpace:'Models.MetaData' -ControllerSubNamespace:$ControllerSubNamespace -DefaultImportingNamespaces:$DefaultImportingNamespaces


#3rd: Views
Write-Host "Scaffolding Views..." -ForegroundColor blue
$ViewModel = $foundModelType.Name + "ViewModel"
$GridViewModel = $foundModelType.Name + "GridViewModel"

$SearchViewModel = $foundModelType.Name + "SearchCriteria"
$ViewPath = "$($Area)Views\"+$modelTypePluralized

#grid 
$ViewType = $GridViewModel
$ViewDetailsType = $foundModelType.Name + "GridRowViewModel" 
ApplyTemplate -TemplateFileName "_List" -Project:$WebProject -OutputPath:$ViewPath -OutputFilePostName:'_List' -OutputFilePreName:'' -RepositoryProject:$RepositoryProject -SubRepositoryNameSpace:'Repositories' -RepositoryInterfaceProject:$RepositoryInterfaceProject -SubRepositoryInterfaceNameSpace:'RepositoryInterfaces' -WebProject:$WebProject -SubViewModelNameSpace:$SubViewModelNameSpace -SubViewModelMetaDataNameSpace:'Models.MetaData' -ControllerSubNamespace:$ControllerSubNamespace  -DefaultImportingNamespaces:$DefaultImportingNamespaces -IgnoreModelNameInFile #-UsePluralNameInFileName 
ApplyTemplate -TemplateFileName "_ListBreifView" -Project:$WebProject -OutputPath:$ViewPath -OutputFilePostName:'_ListBreifView' -OutputFilePreName:'' -RepositoryProject:$RepositoryProject -SubRepositoryNameSpace:'Repositories' -RepositoryInterfaceProject:$RepositoryInterfaceProject -SubRepositoryInterfaceNameSpace:'RepositoryInterfaces' -WebProject:$WebProject -SubViewModelNameSpace:$SubViewModelNameSpace -SubViewModelMetaDataNameSpace:'Models.MetaData' -ControllerSubNamespace:$ControllerSubNamespace  -DefaultImportingNamespaces:$DefaultImportingNamespaces -IgnoreModelNameInFile #-UsePluralNameInFileName 
ApplyTemplate -TemplateFileName "Index" -Project:$WebProject -OutputPath:$ViewPath -OutputFilePostName:'Index' -OutputFilePreName:'' -RepositoryProject:$RepositoryProject -SubRepositoryNameSpace:'Repositories' -RepositoryInterfaceProject:$RepositoryInterfaceProject -SubRepositoryInterfaceNameSpace:'RepositoryInterfaces' -WebProject:$WebProject -SubViewModelNameSpace:$SubViewModelNameSpace -SubViewModelMetaDataNameSpace:'Models.MetaData' -ControllerSubNamespace:$ControllerSubNamespace  -DefaultImportingNamespaces:$DefaultImportingNamespaces -IgnoreModelNameInFile
$ViewDetailsType = $null

#non-grid related
$ViewType = $ViewModel
ApplyTemplate -TemplateFileName "_CreateOrEdit" -Project:$WebProject -OutputPath:$ViewPath -OutputFilePostName:'_CreateOrEdit' -OutputFilePreName:'' -RepositoryProject:$RepositoryProject -SubRepositoryNameSpace:'Repositories' -RepositoryInterfaceProject:$RepositoryInterfaceProject -SubRepositoryInterfaceNameSpace:'RepositoryInterfaces' -WebProject:$WebProject -SubViewModelNameSpace:$SubViewModelNameSpace -SubViewModelMetaDataNameSpace:'Models.MetaData' -ControllerSubNamespace:$ControllerSubNamespace  -DefaultImportingNamespaces:$DefaultImportingNamespaces -IgnoreModelNameInFile
ApplyTemplate -TemplateFileName "_Details" -Project:$WebProject -OutputPath:$ViewPath -OutputFilePostName:'_Details' -OutputFilePreName:'' -RepositoryProject:$RepositoryProject -SubRepositoryNameSpace:'Repositories' -RepositoryInterfaceProject:$RepositoryInterfaceProject -SubRepositoryInterfaceNameSpace:'RepositoryInterfaces' -WebProject:$WebProject -SubViewModelNameSpace:$SubViewModelNameSpace -SubViewModelMetaDataNameSpace:'Models.MetaData' -ControllerSubNamespace:$ControllerSubNamespace  -DefaultImportingNamespaces:$DefaultImportingNamespaces -IgnoreModelNameInFile
ApplyTemplate -TemplateFileName "Create" -Project:$WebProject -OutputPath:$ViewPath -OutputFilePostName:'Create' -OutputFilePreName:'' -RepositoryProject:$RepositoryProject -SubRepositoryNameSpace:'Repositories' -RepositoryInterfaceProject:$RepositoryInterfaceProject -SubRepositoryInterfaceNameSpace:'RepositoryInterfaces' -WebProject:$WebProject -SubViewModelNameSpace:$SubViewModelNameSpace -SubViewModelMetaDataNameSpace:'Models.MetaData' -ControllerSubNamespace:$ControllerSubNamespace  -DefaultImportingNamespaces:$DefaultImportingNamespaces -IgnoreModelNameInFile
ApplyTemplate -TemplateFileName "Edit" -Project:$WebProject -OutputPath:$ViewPath -OutputFilePostName:'Edit' -OutputFilePreName:'' -RepositoryProject:$RepositoryProject -SubRepositoryNameSpace:'Repositories' -RepositoryInterfaceProject:$RepositoryInterfaceProject -SubRepositoryInterfaceNameSpace:'RepositoryInterfaces' -WebProject:$WebProject -SubViewModelNameSpace:$SubViewModelNameSpace -SubViewModelMetaDataNameSpace:'Models.MetaData' -ControllerSubNamespace:$ControllerSubNamespace  -DefaultImportingNamespaces:$DefaultImportingNamespaces -IgnoreModelNameInFile
ApplyTemplate -TemplateFileName "Delete" -Project:$WebProject -OutputPath:$ViewPath -OutputFilePostName:'Delete' -OutputFilePreName:'' -RepositoryProject:$RepositoryProject -SubRepositoryNameSpace:'Repositories' -RepositoryInterfaceProject:$RepositoryInterfaceProject -SubRepositoryInterfaceNameSpace:'RepositoryInterfaces' -WebProject:$WebProject -SubViewModelNameSpace:$SubViewModelNameSpace -SubViewModelMetaDataNameSpace:'Models.MetaData' -ControllerSubNamespace:$ControllerSubNamespace  -DefaultImportingNamespaces:$DefaultImportingNamespaces -IgnoreModelNameInFile
ApplyTemplate -TemplateFileName "DeleteError" -Project:$WebProject -OutputPath:$ViewPath -OutputFilePostName:'DeleteError' -OutputFilePreName:'' -RepositoryProject:$RepositoryProject -SubRepositoryNameSpace:'Repositories' -RepositoryInterfaceProject:$RepositoryInterfaceProject -SubRepositoryInterfaceNameSpace:'RepositoryInterfaces' -WebProject:$WebProject -SubViewModelNameSpace:$SubViewModelNameSpace -SubViewModelMetaDataNameSpace:'Models.MetaData' -ControllerSubNamespace:$ControllerSubNamespace  -DefaultImportingNamespaces:$DefaultImportingNamespaces -IgnoreModelNameInFile
ApplyTemplate -TemplateFileName "Details" -Project:$WebProject -OutputPath:$ViewPath -OutputFilePostName:'Details' -OutputFilePreName:'' -RepositoryProject:$RepositoryProject -SubRepositoryNameSpace:'Repositories' -RepositoryInterfaceProject:$RepositoryInterfaceProject -SubRepositoryInterfaceNameSpace:'RepositoryInterfaces' -WebProject:$WebProject -SubViewModelNameSpace:$SubViewModelNameSpace -SubViewModelMetaDataNameSpace:'Models.MetaData' -ControllerSubNamespace:$ControllerSubNamespace  -DefaultImportingNamespaces:$DefaultImportingNamespaces -IgnoreModelNameInFile

#search 
$ViewType = $SearchViewModel
ApplyTemplate -TemplateFileName "_SearchCriteria" -Project:$WebProject -OutputPath:$ViewPath -OutputFilePostName:'_SearchCriteria' -OutputFilePreName:'' -RepositoryProject:$RepositoryProject -SubRepositoryNameSpace:'Repositories' -RepositoryInterfaceProject:$RepositoryInterfaceProject -SubRepositoryInterfaceNameSpace:'RepositoryInterfaces' -WebProject:$WebProject -SubViewModelNameSpace:$SubViewModelNameSpace -SubViewModelMetaDataNameSpace:'Models.MetaData' -ControllerSubNamespace:$ControllerSubNamespace  -DefaultImportingNamespaces:$DefaultImportingNamespaces -IgnoreModelNameInFile
ApplyTemplate -TemplateFileName "Search" -Project:$WebProject -OutputPath:$ViewPath -OutputFilePostName:'Search' -OutputFilePreName:'' -RepositoryProject:$RepositoryProject -SubRepositoryNameSpace:'Repositories' -RepositoryInterfaceProject:$RepositoryInterfaceProject -SubRepositoryInterfaceNameSpace:'RepositoryInterfaces' -WebProject:$WebProject -SubViewModelNameSpace:$SubViewModelNameSpace -SubViewModelMetaDataNameSpace:'Models.MetaData' -ControllerSubNamespace:$ControllerSubNamespace  -DefaultImportingNamespaces:$DefaultImportingNamespaces -IgnoreModelNameInFile

### old : Scaffold 'View'  -ViewName '_CreateOrEdit' -Template '_CreateOrEdit'  -Controller $modelTypePluralized -ModelType $ViewModel  -Area $Area -Layout $Layout -SectionNames $SectionNames -PrimarySectionName $PrimarySectionName -ReferenceScriptLibraries:$ReferenceScriptLibraries -Project $WebProject -CodeLanguage $CodeLanguage -OverrideTemplateFolders $TemplateFolders -Force:$Force -BlockUi
$ViewType = $null #should do this after finishing view generation before passing control to other segments

#4th: Repository

#4-1: Repository Interface
Write-Host "Scaffolding Repository Interface..." -ForegroundColor blue
ApplyTemplate -TemplateFileName "IRepository" -Project:$RepositoryInterfaceProject -OutputPath:$RepositoryInterfaceFolder -OutputFilePostName:'Repository' -OutputFilePreName:'I' -RepositoryProject:$RepositoryProject -SubRepositoryNameSpace:'Repositories' -RepositoryInterfaceProject:$RepositoryInterfaceProject -SubRepositoryInterfaceNameSpace:'RepositoryInterfaces' -WebProject:$WebProject -SubViewModelNameSpace:$SubViewModelNameSpace -SubViewModelMetaDataNameSpace:'Models.MetaData' -ControllerSubNamespace:$ControllerSubNamespace -DefaultImportingNamespaces:$DefaultImportingNamespaces

#4-2: Repository Interface
Write-Host "Scaffolding Repository Class..." -ForegroundColor blue
ApplyTemplate -TemplateFileName "Repository" -Project:$RepositoryProject -OutputPath:$RepositoryFolder -OutputFilePostName:'Repository' -OutputFilePreName:'' -RepositoryProject:$RepositoryProject -SubRepositoryNameSpace:'Repositories' -RepositoryInterfaceProject:$RepositoryInterfaceProject -SubRepositoryInterfaceNameSpace:'RepositoryInterfaces' -WebProject:$WebProject -SubViewModelNameSpace:$SubViewModelNameSpace -SubViewModelMetaDataNameSpace:'Models.MetaData' -ControllerSubNamespace:$ControllerSubNamespace -DefaultImportingNamespaces:$DefaultImportingNamespaces


#5th: Permissions
Write-Host "Scaffolding Permissions..." -ForegroundColor blue
$permissionEnumName = $foundModelType.Name+'Permission'

#5-1:adding main enum item(enum{...} modification by adding item)
$e=Get-EnumItem -fileName: $PermissionEntityFileName -projectName:$entityProjectName -enumName:$PermissionEntityEnumName
if($e)
{
	try
	{
		$e.addmember($modelTypePluralized,$null,-1)
		Write-Host "'$modelTypePluralized' enum item added to '$permissionEnumName' enum..." 
	}
	catch [Exception]
	{
		if ( $_.Exception.ToString().Contains('already exists'))
		{
			Write-Host "'$modelTypePluralized' enum item already exists in '$permissionEnumName' enum! Passed...(-Force not works on this currently)" -ForegroundColor:Magenta
		}
		else
		{
			throw
		}
	}	
}
else
{
	Write-Host "$PermissionEntityEnumName not found in file $PermissionEntityFileName of project:$entityProjectName!!..." -ForegroundColor:Red
}

#5-2:adding the enum (adding new enum{...} to a namespace) 
$e=Get-CodeItem -fileName: $PermissionEntityFileName -projectName:$entityProjectName -typeName:$PermissionNameSpaceName -typeCode:5	#5=vsCMElementNamespace
if($e)
{
	$oldEnum=Find-CodeItem -obj:$e -typeCode:10 -typeName:$permissionEnumName
	if(-not($oldEnum))
	{
		$o = ApplyTemplate -TemplateFileName "Permission" -Project:$WebProject  -IsOutputText -RepositoryProject:$RepositoryProject -SubRepositoryNameSpace:'Repositories' -RepositoryInterfaceProject:$RepositoryInterfaceProject -SubRepositoryInterfaceNameSpace:'RepositoryInterfaces' -WebProject:$WebProject -SubViewModelNameSpace:$SubViewModelNameSpace -SubViewModelMetaDataNameSpace:'Models.MetaData'-ControllerSubNamespace:$ControllerSubNamespace -DefaultImportingNamespaces:$DefaultImportingNamespaces
		$e.GetEndPoint(16).CreateEditPoint().Insert($o) #16 = EnvDTE.vsCMPart.vsCMPartBody
		Write-Host "'$permissionEnumName' enum added..." 
	}
	else
	{
		Write-Host "$permissionEnumName enum already defined! Passed...(-Force not works on this currently)" -ForegroundColor:Magenta
	}	
}
else
{
	Write-Host "$PermissionNameSpaceName not found in file $PermissionEntityFileName of project:$entityProjectName!!..." -ForegroundColor:Red
}

#5-3:adding the permission limitters (function modification)
$e=Get-CodeItem -fileName: $PermissionCoordinatorFileName -projectName:$RepositoryProject -typeName:$PermissionCoordinatorClassName -typeCode:1 #vsCMElement.vsCMElementClass = 1
if($e)
{
	$constructor = Find-CodeItem -obj:$e -typeCode:2 -typeName: $PermissionCoordinatorLoaderFunctionName #vsCMElementFunction = 2 (function)
	$blockEndPoint = $constructor.GetEndPoint(16).CreateEditPoint()
	$o = ApplyTemplate -TemplateFileName "PermissionLimitters" -Project:$RepositoryProject  -IsOutputText -RepositoryProject:$RepositoryProject -SubRepositoryNameSpace:'Repositories' -RepositoryInterfaceProject:$RepositoryInterfaceProject -SubRepositoryInterfaceNameSpace:'RepositoryInterfaces' -WebProject:$WebProject -SubViewModelNameSpace:$SubViewModelNameSpace -SubViewModelMetaDataNameSpace:'Models.MetaData' -ControllerSubNamespace:$ControllerSubNamespace -DefaultImportingNamespaces:$DefaultImportingNamespaces
	
	if(-not($blockEndPoint.GetText($constructor.GetStartPoint(16)).Contains($permissionEnumName))) #16 = EnvDTE.vsCMPart.vsCMPartBody
	{
		$blockEndPoint.Insert($o)
		Write-Host "'$permissionEnumName' permission limitters added..." 
	}
	else
	{
		Write-Host "$PermissionCoordinatorClassName already has some refrences of '$permissionEnumName'! Passed...(-Force not works on this currently)" -ForegroundColor:Magenta
		Write-Host "you can add it manually :" -ForegroundColor:Magenta
		Write-Host "$o" -ForegroundColor:Magenta		
	}	
}
else
{
	Write-Host "$PermissionCoordinatorClassName not found in file $PermissionCoordinatorFileName of project:$RepositoryProject!!..." -ForegroundColor:Red
}

#5-4:adding the PermissionConstructor to PermissionAttribute
$e = Get-ProjectType $PermissionAttributeClass -Project $WebProject  -BlockUi
if($e)
{
	#$constructor = Find-CodeItem -obj:$e -typeCode:2 -typeName:$PermissionCoordinatorClassName #vsCMElementFunction = 2 (constructor)
	$blocStartPoint = $e.GetStartPoint(16).CreateEditPoint()#16 = EnvDTE.vsCMPart.vsCMPartBody
	$blocEndPoint = $e.GetEndPoint(16).CreateEditPoint()	#16 = EnvDTE.vsCMPart.vsCMPartBody
	$o = ApplyTemplate -TemplateFileName "PermissionAttribute" -Project:$RepositoryProject  -IsOutputText -RepositoryProject:$RepositoryProject -SubRepositoryNameSpace:'Repositories' -RepositoryInterfaceProject:$RepositoryInterfaceProject -SubRepositoryInterfaceNameSpace:'RepositoryInterfaces' -WebProject:$WebProject -SubViewModelNameSpace:$SubViewModelNameSpace -SubViewModelMetaDataNameSpace:'Models.MetaData' -ControllerSubNamespace:$ControllerSubNamespace -DefaultImportingNamespaces:$DefaultImportingNamespaces
	
	if(-not($blocEndPoint.GetText($blocStartPoint).Contains($o.Trim()))) 
	{
		$blocStartPoint.Insert($o)
		Write-Host "New constructor for '$permissionEnumName' added to $PermissionAttributeClass class..." 
	}
	else
	{
		Write-Host "$PermissionAttributeClass already has constructor for '$permissionEnumName'! Passed...(-Force not works on this currently)" -ForegroundColor:Magenta
		Write-Host "you can add it manually :" -ForegroundColor:Magenta
		Write-Host "$o" -ForegroundColor:Magenta
	}	
}
else
{
	Write-Host "$PermissionAttributeClass not found!!..." -ForegroundColor:Red
}

#6th: Logging
Write-Host "Scaffolding LogPoints..." -ForegroundColor blue
[string] $xmlLogFullPath = (Get-Project $WebProject).Properties.Item("LocalPath").Value+$LogPointsXmlFile
$newXmlData = ApplyTemplate -TemplateFileName "LogPoints" -Project:$WebProject  -IsOutputText -RepositoryProject:$RepositoryProject -SubRepositoryNameSpace:'Repositories' -RepositoryInterfaceProject:$RepositoryInterfaceProject -SubRepositoryInterfaceNameSpace:'RepositoryInterfaces' -WebProject:$WebProject -SubViewModelNameSpace:$SubViewModelNameSpace -SubViewModelMetaDataNameSpace:'Models.MetaData'-ControllerSubNamespace:$ControllerSubNamespace -DefaultImportingNamespaces:$DefaultImportingNamespaces

$xml = New-Object XML
$xml.Load($xmlLogFullPath )
[string] $oldXmlData= $xml.LogPoints.InnerXml

$xml.LogPoints.InnerXml = $oldXmlData + $newXmlData

$file = Get-ProjectItem  -project:$WebProject -path:$LogPointsXmlFile
$file.open()
$file.document.save() #helps to check-out if checked-in before
$xml.Save($xmlLogFullPath)

Write-Host "LogPoints Successfully Added to $LogPointsXmlFile file"

#7th: Transform All Templates
Write-Host "Transforming All Project Templates..." -ForegroundColor blue
# [Reflection.Assembly]::Load("EnvDTE") ## This line makes error in VS 2012 , also we don't need it too!
$DTE.ExecuteCommand("TextTransformation.TransformAllTemplates")	

Write-Host "Ohhh My God! Finished finally :) ..." -ForegroundColor blue
