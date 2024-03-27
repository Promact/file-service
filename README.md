# file-service
File Service is a versatile repository facilitating seamless file uploads to cloud storage providers such as AWS S3, Azure. With a unified interface, it simplifies the process of managing files across multiple platforms. Developed with modularity and ease of integration in mind.

# Usage

```
private IFileService _fileService

public async Task MyMethod()
{
    //Use the File Model According To your Service

    var fileModel = new S3FileModel
    {
        BucketName = "my_Bucket",
        KeyName = "FileName",
        FilePath = "path/to/file.txt"
    };

    var fileModel = new AzureBlobFileModel
    {
        ContainerName = "my_container",
        KeyName = "FileName",
        FilePath = "path/to/file.txt"
    };

    // Call the FileService method to upload the file
    _fileService.UploadFileAsync(fileModel).GetAwaiter().GetResult();

    // Call the FileService method to Get  the  content of file as String.
    _fileService.GetFileAsStringAsync(fileModel).GetAwaiter().GetResult();

    // Call the FileService method to delete the file
    _fileService.DeleteFileAsync(fileModel).GetAwaiter().GetResult();

    // In the Case of Azure You get the Link by GetSignedUrlAsync method but you did not have 
    // permission to Acess So for Acess You Go Azure Portal >> Your storage  >> Settings
    // >> Configuration >> Allow Blob anonymous access >> Enabled
    // Then After >> Your Container >>  Change Access Level >> Blob (Read Only)

    // Call the FileService method  with expiration time to Get  the  url of file
      TimeSpan expiration = TimeSpan.FromHours(1);
    _fileService.GetSignedUrlAsync(fileModel,expiration).GetAwaiter().GetResult();

    //Describe the pageno and pagesize for retriving keys in particular page

    var request = new GetAllKeysRequest
    {
        BucketOrContainer = "my_container",
        PageNumber = pgno,
        PageSize = Size_of_one_page,
    };

    var keys = _fileService.GetKeysAsync(request).GetAwaiter().GetResult();

    //foreach (var key in keys)
    //{
    //    Console.WriteLine(key);
    //}
}

```
# AWS

Add nuget package

```
Promact.FileService.AWS
```

## ASP.NET Core projects

Add below in `Program.cs` 

```
 builder.Services.AddAWSFileService(options =>
 {
     options.AccessKey = Configuration.GetSection("AWS:AccessKeyId").Value;
     options.SecretKey = Configuration.GetSection("AWS:SecretAccessKey").Value;
     options.Region = Configuration.GetSection("AWS:Region").Value;
 });

```

Add relevant appsettings.json values

```
"AWS": {
    "AccessKeyID": "",
    "SecretAccessKey": "",
    "Region":  ""
}
```

Inject `IFileService` in class constructor from where you want to perform various files operation.

```
public MyClass(IFileService fileService)
{
...
}
```

# Azure

Add nuget package

```
Promact.FileService.Azure
```

## ASP.NET Core projects

Add below in `Program.cs` 

```
// Register BlobServiceClient
builder.Services.AddAzureFileService(options =>
{
    options.ConnectionString = Configuration.GetSection("Azure:ConnectionString").Value;
});

```

Add relevant appsettings.json values

```
 "Azure": {
   "ConnectionString": ""
 },
```

Inject `IFileService` in class constructor from where you want to perform various files operation.

```
public MyClass(IFileService fileService)
{
...
}
```
