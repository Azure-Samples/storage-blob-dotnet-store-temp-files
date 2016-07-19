using System;
using System.Web;
using System.Web.Mvc;
using System.IO;

namespace CSAzureTempFiles.Controllers
{
    /// <summary>
    /// You can use Path.GetTempPath and Path.GetTempFilename() functions for the temp file name
    /// </summary>
    public class TempFileController : Controller
    {
        /// <summary>
        /// In this action,program will load all file from temp path
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index()
        {
            try
            {
                //this path is temp path
                string tempPath = Path.GetTempPath();

                //get file path list and add to the view bag
                ViewBag.Files = Directory.GetFiles(tempPath);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Error", new { Error = ex.Message });
            }
            return View();
        }

        [HttpGet]
        public ActionResult CreateTempFile()
        {
            return View();
        }

        /// <summary>
        /// Uplaod a new file to temp path
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public ActionResult CreateTempFile(HttpPostedFileBase file)
        {
            try
            {
                SaveTempFile(file);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Error", new { Error = ex.Message });
            }

            return Redirect("Index");
        }

        private void SaveTempFile(HttpPostedFileBase file)
        {
            //get the uploaded file name
            string fileName = file.FileName;

            //get temp directory path
            string tempPath = Path.GetTempPath();

            //init the file path
            string filePath = tempPath + fileName;

            //if the path is exists,delete old file
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            //and then save new file
            file.SaveAs(filePath);
        }
    }
}