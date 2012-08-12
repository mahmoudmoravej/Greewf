$source = @"
public class MyHelper
{
    public static string ToLowerFirstChar(string input)
    {
        return char.ToLower(input[0]) + input.Substring(1);
    }
}
"@

Add-Type -TypeDefinition $source

$variableTag = [MyHelper]::ToLowerFirstChar($ModelType)

function GetTemplateFile([string]$templateFileName)
{
	return Find-ScaffolderTemplate $templateFileName -TemplateFolders $TemplateFolders -CodeLanguage $CodeLanguage -ErrorIfNotFound
}


function ApplyTemplate(
	[string]$TemplateFileName, 
	[string]$Project,
	[string]$outputPath,
	[string]$OutputFilePostName,
	[string]$OutputFilePreName,
	[string]$RepositoryProject,
	[string]$RepositoryInterfaceProject,
	[string]$SubRepositoryNameSpace,
	[string]$SubRepositoryInterfaceNameSpace,
	[string]$WebProject,
	[string]$SubViewModelNameSpace,
	[string]$SubViewModelMetaDataNameSpace,
	[string]$ControllerSubNamespace,
	[string]$PrimaryKey=$PrimaryKey, # NOTE : This helps to avoid passing PrimaryKey parameter for each call. This assignment retrieve $PrimaryKey value for "current" context from $PrimaryKey of the "caller" context(no error exception thrown if not found).
	[string[]]$DefaultImportingNamespaces=$null,
	[switch]$UsePluralNameInFileName=$false,
	[switch]$IgnoreModelNameInFile=$false,
	[switch]$IsOutputText=$false,
	[switch]$ReferenceScriptLibraries=$false)
{	
	$templateFile = GetTemplateFile -templateFileName:$templateFileName 
	if ($templateFile) {

		#making the variables ready
		$foundModelType = Get-ProjectType $ModelType -Project $Project -BlockUi
		if (!$foundModelType) { return }
		
		$foundViewType =$null
		$foundViewTypeName = $null
		if ($ViewType -ne $null){
			$foundViewType = Get-ProjectType $ViewType -Project $Project -BlockUi
			$foundViewTypeName =$foundViewType.Name
		}

		if (!$PrimaryKey){
			$primaryKey = Get-PrimaryKey $foundModelType.FullName -Project $Project -ErrorIfNotFound
		}
		else{#check to see if the passed PK is valid or not
			if (($foundModelType.Members | foreach { if ($_.Name -eq $PrimaryKey){ return $true }}) -ne $true)
			{
				Write-Host "The passed PrimaryKey parameter('$PrimaryKey') not found!" -ForegroundColor:Red
				$primaryKey=$null;				
			}
		}
		if (!$primaryKey) { return }

		#some projects may not have context 
		$foundDbContextType = Get-ProjectType $DbContextType -Project $Project -ErrorAction SilentlyContinue
		if ($foundDbContextType){
			$DbContextNamespace =  $foundDbContextType.Namespace.FullName
			$DbContextType = [MarshalByRefObject]$foundDbContextType
		}
		

		$modelTypePluralized = Get-PluralizedWord $foundModelType.Name
		$defaultNamespace = (Get-Project $Project).Properties.Item("DefaultNamespace").Value
		
		$defaultRepositoryProjectNamespace = (Get-Project $RepositoryProject).Properties.Item("DefaultNamespace").Value
		$repositoryNamespace = [T4Scaffolding.Namespaces]::Normalize($defaultRepositoryProjectNamespace + "." + $SubRepositoryNameSpace)
		$defaultRepositoryInterfaceProjectNamespace = (Get-Project $RepositoryInterfaceProject).Properties.Item("DefaultNamespace").Value
		$repositoryInterfaceNamespace = [T4Scaffolding.Namespaces]::Normalize($defaultRepositoryInterfaceProjectNamespace + "." + $SubRepositoryInterfaceNameSpace)
		$defaultWebProjectNamespace = (Get-Project $WebProject).Properties.Item("DefaultNamespace").Value		
		$ViewModelNameSpace = [T4Scaffolding.Namespaces]::Normalize($defaultWebProjectNamespace + "." + $SubViewModelNameSpace)
		$ViewModelMetaDataNameSpace = [T4Scaffolding.Namespaces]::Normalize($defaultWebProjectNamespace + "." + $SubViewModelMetaDataNameSpace)
		$modelTypeNamespace = [T4Scaffolding.Namespaces]::GetNamespace($foundModelType.FullName)
		$ControllerNamespace = [T4Scaffolding.Namespaces]::Normalize($defaultWebProjectNamespace + "." + $ControllerSubNamespace)
		$ControllerName = $modelTypePluralized + "Controller"
		$repositoryName = $foundModelType.Name + "Repository"
		if ($IsOutputText) 
		{
			$outputFile = ''
		}
		else
		{
			$baseFileName = $foundModelType.Name
			if ($UsePluralNameInFileName)
			{
				$baseFileName = $modelTypePluralized
			}
			if ($IgnoreModelNameInFile) {
				$baseFileName =''
			}
			
			$outputFile = $outputPath+'\'+$outputFilePreName+$baseFileName+$outputFilePostName
		}
#		$relatedEntities = [Array](Get-RelatedEntities $foundModelType.FullName -Project $project)
#		if (!$relatedEntities) { $relatedEntities = @() }

		$wroteFile = Invoke-ScaffoldTemplate -Template $templateFile -Model @{ 
				ViewName =$TemplateFileName;
				ViewDataTypeName = $foundViewTypeName;
				ViewDataType=[MarshalByRefObject]$foundViewType;
				ModelType = [MarshalByRefObject]$foundModelType; 
				PrimaryKey = [string]$primaryKey; 
				PrimaryKeyName = [string]$primaryKey;
				DefaultNamespace = $defaultNamespace; 
				RepositoryNamespace = $repositoryNamespace;
				RepositoriesNamespace =$repositoryNamespace;
				RepositoryInterfaceNamespace = $repositoryInterfaceNamespace;
				ControllerNamespace  = $ControllerNamespace;
				ControllerName = $ControllerName;
				ModelTypeNamespace = $modelTypeNamespace; 
				ModelTypePluralized = [string]$modelTypePluralized; 
				DbContextNamespace =  $DbContextNamespace;
				DbContextType = $DbContextType;
				ViewModelNameSpace = $ViewModelNameSpace;
				ViewModelMetaDataNameSpace = $ViewModelMetaDataNameSpace;
				RelatedEntities = $relatedEntities;
				Repository=$repositoryName;
				DefaultImportingNamespaces=$DefaultImportingNamespaces;
				ReferenceScriptLibraries=[bool] $ReferenceScriptLibraries;
			 } -Project $Project -OutputPath $outputFile -Force:$Force
		if($wroteFile -and -not $IsOutputText) {
			Write-Host "Added $Project '$wroteFile'"
			$fullPath = (Get-Project -Name $Project).properties.item('FullPath').value + $wroteFile
			Set-Content -Encoding UTF8 -Path $fullPath -value (Get-Content -Path $fullPath -Encoding UTF8) 		
		}
		return $wroteFile		
	}
}
	function Get-EnumItem(
		[string]$fileName,
		[string]$projectName,
		[string]$enumName)
		{
			return Get-CodeItem -fileName:$fileName -projectName:$projectName -typeName:$enumName -typeCode:10
		
		}

	function Get-CodeItem(
		[string]$fileName,
		[string]$projectName,
		[string]$typeName,
		[int]$typeCode)
		{
			$file = Get-ProjectItem -project:$projectName -path:$fileName
			if($file)
			{
				if($file.filecodemodel.codeelements.count -gt 0)
				{	
					foreach ($i in $file.filecodemodel.codeelements) {
						$z=Find-CodeItem -obj:$i -typeCode:$typeCode -typeName:$typeName
						if($z) {
							return $z
						}
					}
				}
			}
			else
			{
				Write-Host "$fileName not found in project:$projectName!!..." -ForegroundColor:Red
			}
		}
		
	function Find-CodeItem([Object]$obj,[int]$typeCode,[string]$typeName)
	# $typeCode comes from EnvDTE.vsCMElement enum , 
	#    check this urls: 1- http://msdn.microsoft.com/en-us/library/envdte.vscmelement.aspx
	#					  2- http://mvcscaffolding.codeplex.com/discussions/266595
	{
		if ($obj){
			if(($obj.kind -eq $typeCode) -and ($typeName -eq $obj.name))
			{
				return $obj;
			}
			if($obj.members.count -gt 0)
			{
				foreach($i in $obj.members)
				{
					if($i.kind -ne 0)
					{
						$z= Find-CodeItem -obj:$i -typeCode:$typeCode -typeName:$typeName
						if($z)
						{
							return $z
						}
					}
				}
			}
			
		}
	}
	
