﻿using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.Azure
{
    public class AzureFileService : IFileService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public AzureFileService(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient ?? throw new ArgumentNullException(nameof(blobServiceClient));
        }

        /// <summary>
        /// Checks if a blob exists in the specified Azure Blob Storage container asynchronously.
        /// </summary>
        /// <param name="containerName">The name of the container where the blob resides.</param>
        /// <param name="blobName">The name of the blob to check for existence.</param>
        /// <returns>A task representing the asynchronous operation. The task result indicates whether the blob exists (true) or not (false).</returns>
        public async Task<bool> CheckIfBlobExistAsync(string containerName, string blobName)
        {
            try
            {
                var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = blobContainerClient.GetBlobClient(blobName);
                return await blobClient.ExistsAsync();
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return false;
            }
        }

        public async Task UploadFileAsync(FileModelBase file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            if (!(file is AzureBlobFileModel azureBlobFile))
                throw new ArgumentException("File model is not of type AzureBlobFileModel");

            try
            {
                var blobClient = _blobServiceClient.GetBlobContainerClient(azureBlobFile.ContainerName).GetBlobClient(azureBlobFile.KeyName);
                await blobClient.UploadAsync(azureBlobFile.FilePath, true);
            }
            catch (RequestFailedException ex)
            {
                throw new Exception($"Error uploading file to Azure Blob Storage: {ex.Message}");
            }
        }

        public async Task<byte[]> GetFileAsBytesAsync(FileModelBase file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            if (!(file is AzureBlobFileModel azureBlobFile))
                throw new ArgumentException("File model is not of type AzureBlobFileModel");

            try
            {
                var blobClient = _blobServiceClient.GetBlobContainerClient(azureBlobFile.ContainerName).GetBlobClient(azureBlobFile.KeyName);
                var response = await blobClient.DownloadAsync();

                using (var memoryStream = new MemoryStream())
                {
                    await response.Value.Content.CopyToAsync(memoryStream);
                    return memoryStream.ToArray();
                }
            }
            catch (RequestFailedException ex)
            {
                throw new Exception($"Error getting file from Azure Blob Storage: {ex.Message}");
            }
        }

        public async Task DeleteFileAsync(FileModelBase file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            if (!(file is AzureBlobFileModel azureBlobFile))
                throw new ArgumentException("File model is not of type AzureBlobFileModel");

            try
            {
                bool blobExists = await CheckIfBlobExistAsync(azureBlobFile.ContainerName, azureBlobFile.KeyName);
                if (!blobExists)
                {
                    throw new InvalidOperationException($" The specified blob does not exist Else Blob '{azureBlobFile.KeyName}' does not exist in container '{azureBlobFile.ContainerName}'");
                }

                var blobClient = _blobServiceClient.GetBlobContainerClient(azureBlobFile.ContainerName).GetBlobClient(azureBlobFile.KeyName);
                await blobClient.DeleteIfExistsAsync();
            }
            catch (RequestFailedException ex)
            {
                throw new Exception($"Error deleting file from Azure Blob Storage: {ex.Message}");
            }
        }

        public async Task<string> GetSignedUrlAsync(FileModelBase file, TimeSpan expiration)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            if (!(file is AzureBlobFileModel azureBlobFile))
                throw new ArgumentException("File model is not of type AzureBlobFileModel");

            try
            {
                bool blobExists = await CheckIfBlobExistAsync(azureBlobFile.ContainerName, azureBlobFile.KeyName);
                if (!blobExists)
                {
                    throw new InvalidOperationException($"The specified blob does not exist Else Blob '{azureBlobFile.KeyName}' does not exist in container '{azureBlobFile.ContainerName}'");
                }

                var blobContainerClient = _blobServiceClient.GetBlobContainerClient(azureBlobFile.ContainerName);
                var blobClient = blobContainerClient.GetBlobClient(azureBlobFile.KeyName);

                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = azureBlobFile.ContainerName,
                    BlobName = azureBlobFile.KeyName,
                    Resource = "b",
                    StartsOn = DateTimeOffset.UtcNow,
                    ExpiresOn = DateTimeOffset.UtcNow.Add(expiration), 
                };

                sasBuilder.SetPermissions(BlobSasPermissions.Read);

                var sasToken = blobClient.GenerateSasUri(sasBuilder);

                return sasToken.ToString();
            }
            catch (RequestFailedException ex)
            {
                throw new Exception($"Error getting signed URL for Azure Blob Storage: {ex.Message}");
            }
        }

        public async Task<List<string>> GetKeysAsync(GetAllKeysRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.BucketOrContainer == null)
                throw new ArgumentNullException(nameof(request.BucketOrContainer));

            if (request.PageNumber <= 0 || request.PageSize <= 0)
                throw new ArgumentException("Page number and page size must be greater than zero");

            var keys = new List<string>();

            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(request.BucketOrContainer);

                int startIndex = (request.PageNumber - 1) * request.PageSize;

                int keysCount = 0;

                await foreach (Page<BlobItem> page in containerClient.GetBlobsAsync().AsPages())
                {
                    foreach (BlobItem blobItem in page.Values)
                    {
                        keysCount++;

                        if (keysCount <= startIndex)
                            continue;

                        keys.Add(blobItem.Name);

                        if (keys.Count >= request.PageSize)
                            break;
                    }

                    if (keys.Count >= request.PageSize)
                        break;
                }

                return keys;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving keys from Azure Blob Storage.", ex);
            }
        }
    }
}
