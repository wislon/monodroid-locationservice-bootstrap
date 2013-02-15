using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Util;
using Java.IO;
using Newtonsoft.Json;
using SIO = System.IO;

namespace MonoDroid.LocationService.Bootstrap.Services
{

    [Service(Label = "LMS Upload Service")]
    public class UploadService : IntentService
    {
        private bool _uploadingData;

        private const string _apiBaseUrl = "http://your.website.here/api";

        protected override void OnHandleIntent(Intent intent)
        {
            if (!_uploadingData)
            {
                _uploadingData = true;
                try
                {
                    if (ExternalMediaMounted())
                    {
                        var filesToUpload = GetListOfFilesToUpload().ToList();
                        if (filesToUpload.Any())
                        {
                            ProcessFiles(filesToUpload.ToArray());
                        }
                    }
                }
                finally
                {
                    _uploadingData = false;
                }

            }
        }

        private void ProcessFiles(string[] filesToUpload)
        {
            var tasks = new Task[filesToUpload.Length];
            for (int i = 0; i < filesToUpload.Length; i++)
            {
                string fileNameToUpload  = filesToUpload[i];
                tasks[i] = Task.Factory.StartNew(() => UploadFileToWebAPIHost(fileNameToUpload));
            }
            Task.WaitAll(tasks);
        }

        private void UploadFileToWebAPIHost(string fileNameToUpload)
        {
            var contents = ReadTheDataAsync(fileNameToUpload).Result;

            // this is just way, WAY easier than doing battle with stuff like base-64 encoding, 
            // formurlencoding, setting multipart form boundaries etc.
            string jsonContents = JsonConvert.SerializeObject(contents); 

            string uploadUrl = string.Format("{0}/{1}", _apiBaseUrl, "UploadDataFile");

            var request = new HttpWebRequest(new Uri(uploadUrl))
                              {
                                  ContentType = "application/json",
                                  Method = "POST",
                                  ContentLength = jsonContents.Length
                              };

            using (var sw = new SIO.StreamWriter(request.GetRequestStream(), Encoding.ASCII))
            {
                sw.Write(jsonContents);
                sw.Close();
            }

            request.BeginGetResponse(ProcessUploadResponse, request);
        }


        #region Example destination Web API controller method which can handle this data
        // A (working) sample of a Web API controller method which will accept the data and present
        // it to you for use. 
        // [HttpPost]
        // public HttpResponseMessage UploadDataFile([FromBody]string dataFile)
        // {
        //    string test = dataFile;
        //    Debug.WriteLine(test.Substring(0, 500));
        //    var response = Request.CreateResponse(HttpStatusCode.OK);
        //    return response;
        // }
        #endregion

        private void ProcessUploadResponse(IAsyncResult ar)
        {
            var request = (HttpWebRequest) ar.AsyncState;
            try
            {
                var response = request.EndGetResponse(ar);
                Log.Info("UPS.PUR", "Request appears to have succeeded. Or at least it didn't fail badly enough to throw");
            }
            catch (Exception ex)
            {
                Log.Info("UPS.PUR", "Response message: {0} ", ex.Message);
            }
        }


        /// <summary>
        /// Probably doesn't look like we need an 'async' version here, since we essentially await it almost
        /// immediately. But more's coming...
        /// </summary>
        /// <param name="fileNameToUpload"></param>
        /// <returns></returns>
        private Task<string> ReadTheDataAsync(string fileNameToUpload)
        {
            return Task.Factory.StartNew(() =>
                                             {
                                                 string contents;
                                                 using (var fis = new SIO.FileStream(fileNameToUpload, SIO.FileMode.Open,
                                                                                  SIO.FileAccess.Read,
                                                                                  SIO.FileShare.None))
                                                 {
                                                     using (var sr = new SIO.StreamReader(fis)) // this has previously been written using the UTF8Encoding().GetBytes, already
                                                     {
                                                         contents = sr.ReadToEnd();
                                                         sr.Close();
                                                     }
                                                     fis.Close();
                                                 }
                                                 return contents;
                                             });
        }

        private bool ExternalMediaMounted()
        {
            return Android.OS.Environment.ExternalStorageState == Android.OS.Environment.MediaMounted;
        }

        private IEnumerable<string> GetListOfFilesToUpload()
        {

            Log.Info("TFPC.ExportData", "Checking if media is mounted");
            // this will look at /mnt/sdcard/Android/Data/[your package name]/files/Downloads/applicationData.*.txt
            File downloadsDir = GetExternalFilesDir(Android.OS.Environment.DirectoryDownloads);

            Log.Info("TFPC.ExportData", "Searching for exported data in {0} ...", downloadsDir.AbsolutePath);
            var fileSystemEntries = SIO.Directory.GetFileSystemEntries(downloadsDir.AbsolutePath,
                                                                        "applicationData.*.txt");
            if (fileSystemEntries.Any())
            {
                foreach (var fileSystemEntry in fileSystemEntries)
                {
                    Log.Info("LMS.US.UploadData", fileSystemEntry);
                }
                return fileSystemEntries;
            }
            return new string[] { };
        }
    }
}