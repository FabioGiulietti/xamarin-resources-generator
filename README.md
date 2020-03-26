# xamarin-resources-generator
A collection of MsBuild Tasks for Xamarin resources generation.

# Available in Nuget
https://www.nuget.org/packages/Xamarin.Resources.Generator/
```
Install-Package Xamarin.Resources.Generator -Version 1.0.0
```

# Usage
- Install Nuget package in PCL project
- Open your PCL.csproj file with a text editor and update the following lines:

Declare the build tasks
```
<UsingTask TaskName="AndroidLanguagesGenerator" AssemblyFile="$(NuGetPackageRoot)xamarin.resources.generator/1.0.0/lib/netstandard2.1/XamarinResourcesGenerator.dll" />
<UsingTask TaskName="LocalizationEnumGenerator" AssemblyFile="$(NuGetPackageRoot)xamarin.resources.generator/1.0.0/lib/netstandard2.1/XamarinResourcesGenerator.dll" />
```
Create a PreBuild target and customize the tasks
```
<Target Name="PreBuild" BeforeTargets="PreBuildEvent">
    <AndroidLanguagesGenerator OriginFolder="../PCL/Localization" DestinationFolder="../Droid/Resources" />
    <LocalizationEnumGenerator OriginResxFolder="../PCL/Localization" PackageName="PCL.Localization" AssemblyName="PCL" />
</Target>
```

# Available Build Tasks

### LocalizationEnumGenerator
Generates a StringManager class that provides localized strings from multiple .resx files.

Required properties:
- OriginResxFolder folder used for fetch all .resx files with the strings
- PackageName package name used for the StringManager and LocalizedString class generation
- AssemblyName assembly name needed for .resx fetch

Usage:

- Place a .resx file in PCL/Localization folder with the following content
```
<?xml version="1.0" encoding="utf-8"?>
<root>
    <resheader name="resmimetype">
        <value>text/microsoft-resx</value>
    </resheader>
    <resheader name="version">
        <value>2.0</value>
    </resheader>
    <resheader name="reader">
        <value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
    </resheader>
    <resheader name="writer">
        <value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
    </resheader>
     
    <data name="helloWorld">
        <value>Hello World!</value>
    </data>
    .
    .
    .
    .
    .
</root>
```

- Define the task in the PCL .csproj
```
<LocalizationEnumGenerator OriginResxFolder="../PCL/Localization" PackageName="PCL.Localization" AssemblyName="PCL" />
```

- Build the project, now you can fetch the string with the StringManager:
```
var enumGenerated = PCL.Localization.LocalizedString.helloWorld;
var helloWorldMessage = PCL.Localization.StringManager.Instance.GetString(enumGenerated);
```

Multi languages support:
- create one .resx file for each language you need, for example resources.resx, resources.it.resx, resources.fr.resx ...
- StringManager class, when initialized, set the default Culture set by the user, if you would like to change at runtime you can call StringManager.SetCulture(CultureInfo cultureInfo)

### AndroidLanguagesGenerator
task that transform all .resx files in a specified folder into the corresponding android string format files, for example:
- resources.resx -> values/strings.xml
- resources.fr.resx -> values-fr/strings.xml
- resources.it.resx -> values-it/strings.xml

Required properties:
- OriginFolder Path to the source folder from which the task retrieves all the .resx files to be transformed
- DestinationFolder Path to the target Android folder where the task generates the values folders

Usage:
- Place a .resx file in PCL/Localization folder
- Define the task in the PCL .csproj
```
<AndroidLanguagesGenerator OriginFolder="../PCL/Localization" DestinationFolder="../Droid/Resources" />
```

- Build the project, now you can see the string.xml files generated for each language in the res directory:
```
var helloWorldString = Activity.GetString(Resource.String.helloWorld);
```
