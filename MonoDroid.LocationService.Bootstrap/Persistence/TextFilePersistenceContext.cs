using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Android.Content;
using Android.Util;
using Java.IO;

namespace MonoDroid.LocationService.Bootstrap.Persistence
{
    /// <summary>
    /// Uses a text file on the file system to persist the text records, one per line. 
    /// Only very limited implementation (adding a line). No lookups/loads/deletes/updates supported
    /// </summary>
    /// <typeparam name="T">The type of data to be persisted</typeparam>
    public class TextFilePersistenceContext<T> : PersistenceContextBase<T>
    {

        private readonly StringBuilder _storageQueue;
        private readonly string _dataFileName;
        private File _dataFile;

        /// <summary>
        /// Default constructor required to pass in the context of the application. Otherwise
        /// we can't get access to things like the file system.
        /// </summary>
        /// <param name="context">Caller's context</param>
        /// <param name="dataFileName">Optional file name to store data in. File will be created if not present, 
        /// and appended to if found. You may want to give it an extension too</param>
        public TextFilePersistenceContext(Context context, string dataFileName = "applicationData.txt") : base(context)
        {
            Log.Info("TFPC.ctor", "TextFilePersistenceContext will be used for storage");
            _dataFileName = dataFileName;
            _storageQueue = new StringBuilder();
            InitialiseStorage();
        }

        /// <summary>
        /// This will get a directory named /data/data/[app name]/app_Data/ if on the internal. it will be created if it doesn't exist.
        /// In this implementation you'll only be able to manually get to this file on the device itself 
        /// if you have root access.
        /// No support yet for using a different path on the internal or external drives. Maybe later...
        /// </summary>
        private void InitialiseStorage()
        {
            Log.Info("TFPC.InitialiseStorage", "Setting up storage");
            var path = GetDir("Data", FileCreationMode.Private); // it prefixes 'app_' to whatever directory name you give it.
            _dataFile = new File(path, _dataFileName);
            Log.Info("TFPC.SaveChanges", "Data will be saved to {0}", _dataFile.AbsolutePath);
        }

        /// <summary>
        /// Android uses an Environment eol/new-line character of '\n'. DOS\Windows uses '\r\n'.
        /// '\n' will be used as an 'end-of-record marker', since it'll be used if we ever read the data
        /// back. Then it'll be easier to manage it natively.
        /// </summary>
        /// <param name="data"></param>
        public override void Insert(T data)
        {
            _storageQueue.AppendFormat("{0}{1}", data, Environment.NewLine); // not using AppendLine! 
        }

        public override void SaveChanges()
        {
            Log.Info("TFPC.SaveChanges", "Persisting data");
            var encoding = new UTF8Encoding();
            var dataToWrite = encoding.GetBytes(_storageQueue.ToString()); 

            WriteTheDataAsync(dataToWrite);
            _storageQueue.Clear();
        }

        private void WriteTheDataAsync(byte[] dataToWrite)
        {
            Task.Factory.StartNew(() =>
                                      {
                                          using (var fos = new FileOutputStream(_dataFile, true))
                                          {
                                              fos.Write(dataToWrite);
                                              fos.Flush();
                                              fos.Close();
                                          }
                                      });
        }

        public override void CancelChanges()
        {
            _storageQueue.Clear();
        }

        public override T GetById(string id)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<T> List()
        {
            throw new NotImplementedException();
        }
    }
}