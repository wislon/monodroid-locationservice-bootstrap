﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Java.IO;
using MonoDroid.LocationService.Bootstrap.Helpers;
using Newtonsoft.Json;
using SIO = System.IO;

namespace MonoDroid.LocationService.Bootstrap.Services
{

    [Service(Label = "LMS Upload Service")]
    public class UploadService : IntentService
    {
        private bool _uploadingData;

        private string _apiBaseUrl = string.Empty;
        private string _apiControllerName = string.Empty;
        private string _uploadMethodName = string.Empty;


        private readonly Handler _handler = new Handler();

        protected override void OnHandleIntent(Intent intent)
        {
            if (_uploadingData) return;

            _apiBaseUrl = PreferencesHelper.GetPreferenceAsString(this, AppConstants.PrefsKeyBaseWebAPIUrl);
            _apiControllerName = PreferencesHelper.GetPreferenceAsString(this, AppConstants.PrefsKeyAPIControllerName);
            _uploadMethodName = PreferencesHelper.GetPreferenceAsString(this, AppConstants.PrefsKeyUploadMethodName);
            _uploadingData = true;
            try
            {
                if (ExternalMediaMounted())
                {
                    var filesToUpload = GetListOfFilesToUpload().ToList();
                    if (filesToUpload.Any())
                    {

                        NotificationHelper.SendBroadcastForToastMessage(this, string.Format("LMS: Uploading {0} files...", filesToUpload.Count));
                        ProcessFiles(filesToUpload.ToArray());
                        NotificationHelper.SendBroadcastForToastMessage(this, "LMS: Upload complete");
                    }
                }
            }
            finally
            {
                _uploadingData = false;
            }
        }

        private void ProcessFiles(string[] filesToUpload)
        {
            var tasks = new Task[filesToUpload.Length];
            for (int i = 0; i < filesToUpload.Length; i++)
            {
                string fileNameToUpload  = filesToUpload[i];
                // TODO possible thread explosion here, need a parallel.for/partition or something
                tasks[i] = Task.Factory.StartNew(() => UploadFileToWebAPIHost(fileNameToUpload));
            }
            Task.WaitAll(tasks);
        }

        private void UploadFileToWebAPIHost(string fileNameToUpload)
        {
            _handler.Post(() => Log.Info("LMS.US.UFTWAH", "Starting upload of {0}", fileNameToUpload));
            var contents = ReadTheDataAsync(fileNameToUpload).Result;

            // this is just way, WAY easier than doing battle with stuff like base-64 encoding, 
            // formurlencoding, setting multipart form boundaries etc.
            string jsonContents = JsonConvert.SerializeObject(contents); 

            string uploadUrl = string.Format("{0}/{1}/{2}", _apiBaseUrl, _apiControllerName, _uploadMethodName);
            _handler.Post(() => Log.Info("LMS.US.UFTWAH", "Uploading to: {0}", uploadUrl));

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

            // TODO we need a better way of garbage collecting the data we've successfully uploaded...
            var requestParameters = new RequestParameters {RequestObject = request, SentFileName = fileNameToUpload};

            request.BeginGetResponse(ProcessUploadResponse, requestParameters);
        }


        #region Example destination Web API controller method which can handle this data
        // A (working) sample of a Web API controller method which will accept 
        // (and automatically deserialise) the data. 
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
            var requestParameters = (RequestParameters)ar.AsyncState;
            var originalRequest = requestParameters.RequestObject;
            try
            {
                var response = (HttpWebResponse)originalRequest.EndGetResponse(ar);
                if ((int)response.StatusCode >= 200 && (int)response.StatusCode <= 299) // 
                {
                    _handler.Post(() => Log.Info("LMS.US.PUR", "Upload succeeded: Status {0} - {1}", (int)response.StatusCode, response.StatusDescription));
                    _handler.Post(() => Log.Info("LMS.US.PUR", "Deleting file {0}", requestParameters.SentFileName));
                    SIO.File.Delete(requestParameters.SentFileName);
                    return;
                }
                _handler.Post(() => Log.Info("LMS.US.PUR", "Problem with upload: Status {0} - {1}", (int)response.StatusCode, response.StatusDescription));
            }
            catch (Exception ex)
            {
                _handler.Post(() => Log.Info("LMS.US.PUR", "Response message: {0} ", ex.Message));
                NotificationHelper.SendBroadcastForToastMessage(this, string.Format("LMS: Upload failed: {0} ", ex.Message));
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
                                                 using (var fis = new SIO.FileStream(fileNameToUpload, 
                                                                                     SIO.FileMode.Open,
                                                                                     SIO.FileAccess.Read,
                                                                                     SIO.FileShare.None))
                                                 {
                                                     // this has previously been written using the UTF8Encoding().GetBytes, already
                                                     using (var sr = new SIO.StreamReader(fis)) 
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

            Log.Info("LMS.US.GLOFTU", "Checking if media is mounted");
            // this will look at /mnt/sdcard/Android/Data/[your package name]/files/Downloads/applicationData.*.txt
            File downloadsDir = GetExternalFilesDir(Android.OS.Environment.DirectoryDownloads);

            Log.Info("LMS.US.GLOFTU", "Searching for exported data in {0} ...", downloadsDir.AbsolutePath);
            var fileSystemEntries = SIO.Directory.GetFileSystemEntries(downloadsDir.AbsolutePath,
                                                                        "applicationData.*.txt");
            if (fileSystemEntries.Any())
            {
                foreach (var fileSystemEntry in fileSystemEntries)
                {
                    Log.Info("LMS.US.GLOFTU", fileSystemEntry);
                }
                return fileSystemEntries;
            }
            return new string[] { };
        }
    }

    /// <summary>
    /// Just a wrapper for some miscellaneous request-specific info, so when the callback is invoked, 
    /// we can get access to the request object as well as the name of file it just uploaded.
    /// </summary>
    internal class RequestParameters
    {
        public HttpWebRequest RequestObject { get; set; }
        public string SentFileName { get; set; }
    }
}