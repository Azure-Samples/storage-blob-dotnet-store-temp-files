---
services: blobs
platforms: c#
author: zhang kun
---

# Store temp files by Azure Blob storage on Azure applications
This sample demonstrates how to store temp files by Azure Blob storage in Azure applications

## Introduction 
This sample demonstrates how to store temp file in Azure applications. 


We have two solutions： 

1. First one is traditional way to call ‘Path.GetTempPath()’ to store temp file in all windows platform 
2. The other is azure specific way to use blob object in Azure to simulate temp file.  

## Building the Sample
1. Double click CSAzureTempFiles.sln file to open this sample solution by using Microsoft Visual Studio 2012 or the later version(s). 
2. Restore nugget packages in the solution 
![restore-nuget-package](images/restore-nuget-package.png)
