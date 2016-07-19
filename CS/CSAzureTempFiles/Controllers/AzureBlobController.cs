using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CSAzureTempFiles.Controllers
{
    /// <summary>
    /// Maybe you need get the renference 'Microsoft.WindowsAzure.Storage' from Nuget, search 'WindowsAzure.Storage' in Nuget and install it.
    /// 
    /// this just simulate temp file in Azure.
    /// if limit is hit, enumerate all the temp files based on the last modified date on the blob, and delete them
    /// </summary>
    public class AzureBlobController : Controller
    {
        //this is storage credentials for Azure,the account name and access key can be find in your dash board of Azure.
        private static readonly StorageCredentials cred = new StorageCredentials("[Account name]", "[Access key]");

        //this is your storage container address in Azure, and it can be found in your dashboard as well.
        private static readonly CloudBlobContainer container = new CloudBlobContainer(new Uri("[Container address]"), cred);

        //100M
        // you can set your threshold of storage size to fufill your requirement
        private long TotalLimitSizeOfTempFiles = 1 * 1024 * 1024;

        [HttpGet]
        public ActionResult Index()
        {
            try
            {
                //list all blobs from container in order of 'LastModified'
                ViewBag.blobs = container.ListBlobs()
                    .OfType<CloudBlob>() //we need convert item type to get property 'LastModified'
                    .OrderByDescending(m => m.Properties.LastModified)
                    .ToList();
            }
            catch (StorageException ex)
            {
                return RedirectToAction("Index", "Error", new { Error = ex.Message + ",please check your StorageCredentials or your network." });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Error", new { Error = ex.Message });
            }

            //send the blobs to view
            return View();
        }

        [HttpGet]
        public ActionResult CreateTempFile()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> CreateTempFile(HttpPostedFileBase file)
        {
            try
            {
                await SaveTempFile(file.FileName, file.ContentLength, file.InputStream);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    return RedirectToAction("Index", "Error", new { Error = ex.InnerException.Message });
                }
                else
                {
                    return RedirectToAction("Index", "Error", new { Error = ex.Message });
                }
            }
            return Redirect("Index");
        }

        private async Task SaveTempFile(string fileName, long contentLenght, Stream inputStream)
        {
            try
            {
                //firstly, we need check the container if exists or not. And if not, we need to create one.
                await container.CreateIfNotExistsAsync();

                //init a blobReference
                CloudBlockBlob tempFileBlob = container.GetBlockBlobReference(fileName);

                //if the blobReference is exists, delete the old blob
                tempFileBlob.DeleteIfExists();

                //check the count of blob if over limit or not, if yes, clear them.
                await CleanStorageIfReachLimit(contentLenght);

                //and upload the new file in this
                tempFileBlob.UploadFromStream(inputStream);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    throw ex.InnerException;
                }
                else
                {
                    throw ex;
                }
            }
        }

        //check the count of blob if over limit or not, if yes, clear them. 
        private async Task CleanStorageIfReachLimit(long newFileLength)
        {
            List<CloudBlob> blobs = container.ListBlobs()
                .OfType<CloudBlob>()
                .OrderBy(m => m.Properties.LastModified)
                .ToList();

            //get total size of all blobs.
            long totalSize = blobs.Sum(m => m.Properties.Length);

            //calculate out the real limit size of before upload
            long realLimetSize = TotalLimitSizeOfTempFiles - newFileLength;

            //delete all,when the free size is enough, break this loop,and stop delete blob anymore
            foreach (CloudBlob item in blobs)
            {
                if (totalSize <= realLimetSize)
                {
                    break;
                }

                await item.DeleteIfExistsAsync();
                totalSize -= item.Properties.Length;
            }
        }
    }
}