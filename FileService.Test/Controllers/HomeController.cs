﻿using FileService.AWS;
using FileService.Test.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IO;

namespace FileService.Test.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IFileService _fileService;

        public HomeController(ILogger<HomeController> logger, IFileService fileService)
        {
            _logger = logger;
            _fileService = fileService;

            //Use the File Model According To your Service

            //var fileModel = new S3FileModel
            //{
            //    BucketName = "my_Bucket",
            //    KeyName = "FileName",
            //    FilePath = "path/to/file.txt"
            //};

            //var fileModel = new AzureBlobFileModel
            //{
            //    ContainerName = "my_container",
            //    KeyName = "FileName",
            //    FilePath = "path/to/file.txt"
            //};

            //// Call the FileService method to Get  the  content of file in bytes
            //_fileService.GetFileAsBytesAsync(fileModel).GetAwaiter().GetResult();

            //// Call the FileService method to upload the file
            //_fileService.UploadFileAsync(fileModel).GetAwaiter().GetResult();

            //// Call the FileService method to delete the file
            //_fileService.DeleteFileAsync(fileModel).GetAwaiter().GetResult();

            // In the Case of Azure You get the Link by downside method but you did not have 
            // permission to Acess So for Acess You Go Azure Portal >> Your storage  >> Settings
            // >> Configuration >> Allow Blob anonymous access >> Enabled

            // Then After >> Your Container >>  Change Access Level >> Blob (Read Only)

            //// Call the FileService method  with expiration time to Get  the  url of file
            //TimeSpan expiration = TimeSpan.FromHours(1);
            //_fileService.GetSignedUrlAsync(fileModel,expiration).GetAwaiter().GetResult();


            //Describe the pageno and pagesize for retriving keys in particular page
            //var request = new GetAllKeysRequest
            //{
            //    BucketOrContainer = "my_container",
            //    PageNumber = 5,
            //    PageSize = 3,
            //};

            //var keys = _fileService.GetKeysAsync(request).GetAwaiter().GetResult();

            //foreach (var key in keys)
            //{
            //    Console.WriteLine(key);
            //}
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}