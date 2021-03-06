﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Android.Content;
using Android.Util;
using Java.IO;
using Mono.Contracts.Location.Mappers;
using SIO = System.IO;

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

        private readonly IMapToString<T> _stringMapper;

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
            _stringMapper = ResolveStringMapperFor<T>(typeof(T));
        }

        /// <summary>
        /// Looks at the type and determines whether there's a mapper for it (convention based, it takes the 
        /// name of the type you're passing, and suffixes "Mapper" to it). If it finds one, it then checks to 
        /// see if it implements IMapToString[type]). If it does, it'll give you back an instance of that object.
        /// </summary>
        /// <param name="type">The type of the object to resolve an IMapToString object for</param>
        private IMapToString<T> ResolveStringMapperFor<T1>(Type type)
        {
            Log.Info("TFPC.ResolveStringMapperFor", "Attempting to locate a string mapper for {0}", type.Name);
            string stringTypeMapperName = string.Format("{0}Mapper", type.Name);
            var contractsAssemblyName = Assembly.GetExecutingAssembly().GetReferencedAssemblies().ToList().First(gr => gr.Name.Contains("Contracts"));
            Type stringMapper =
                Assembly.Load(contractsAssemblyName)
                        .GetExportedTypes()
                        .FirstOrDefault(t => t.Name.Equals(stringTypeMapperName) && 
                              (t.GetInterface(typeof (IMapToString<T1>).Name) != null));

            if (stringMapper != null)
            {
                Log.Info("TFPC", "String mapper for {0} found: {1}", type.Name, stringMapper.Name);
                return (IMapToString<T>)Activator.CreateInstance(stringMapper);
            }

            string message = string.Format("No object named '{0}' implementing '{1}' (where `1 is a {2}) was found", 
                                            stringTypeMapperName, 
                                            typeof(IMapToString<T1>).Name, 
                                            typeof(T1).Name);

            Log.Error("TFPC.ResolveStringMapperFor", message);
            throw new ArgumentException(message);
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
            Log.Info("TFPC.SaveChanges", "Data will be appended to {0}", _dataFile.AbsolutePath);
        }

        /// <summary>
        /// Android uses an Environment eol/new-line character of '\n'. DOS\Windows uses '\r\n'.
        /// '\n' will be used as an 'end-of-record marker', since it'll be used if we ever read the data
        /// back. Then it'll be easier to manage it natively.
        /// </summary>
        /// <param name="data"></param>
        public override void Insert(T data)
        {
            var outputData = _stringMapper.MapToString(data); 
            _storageQueue.AppendFormat("{0}{1}", outputData, Environment.NewLine); // not using AppendLine! 
        }

        public override void SaveChanges()
        {
            Log.Info("TFPC.SaveChanges", "Persisting data");
            var encoding = new UTF8Encoding();
            var dataToWrite = encoding.GetBytes(_storageQueue.ToString()); 

            WriteTheDataAsync(dataToWrite);
            _storageQueue.Clear();
        }

        /// <summary>
        /// This will simply copy the text file from its private location, to the 'Download' folder, which is 
        /// pretty much publicly accessible (by the user and their apps, at least). From there it can be shared, 
        /// or copied across manually.
        /// </summary>
        public override string ExportData()
        {
            if (SIO.File.Exists(_dataFile.AbsolutePath))
            {
                Log.Info("TFPC.ExportData", "Exporting data...");
                if (Android.OS.Environment.ExternalStorageState == Android.OS.Environment.MediaMounted)
                {
                    // this will put it at /mnt/Android/Data/[your package name]/files/Downloads
                    File downloadsDir = GetExternalFilesDir(Android.OS.Environment.DirectoryDownloads);
                    var incrementedDataFileName = GetUniqueExportFileName(downloadsDir, SIO.Path.ChangeExtension(_dataFileName, string.Empty));

                    var exportFileName = new File(downloadsDir, incrementedDataFileName);
                    Log.Info("TFPC.SaveChanges", string.Format("Copying from {0} to {1}", _dataFile.AbsolutePath, exportFileName.AbsolutePath));
                    SIO.File.Copy(_dataFile.AbsolutePath, exportFileName.AbsolutePath);
                    _dataFile.Delete();
                    InitialiseStorage();
                    Log.Info("TFPC.SaveChanges", string.Format("Exported to {0}", exportFileName.AbsolutePath));
                    return exportFileName.AbsolutePath;
                }
            }
            Log.Info("TFPC.SaveChanges", string.Format("{0} doesn't exist, nothing to export", _dataFileName));
            return string.Empty;
        }

        private string GetUniqueExportFileName(File downloadsDir, string baseFileName)
        {
            int i = 0;
            const string formatString = "{0}{1:D4}.txt";
            string exportFileName = string.Format(formatString, baseFileName, i);
            while (SIO.File.Exists(SIO.Path.Combine(downloadsDir.AbsolutePath, exportFileName)))
            {
                exportFileName = string.Format(formatString, baseFileName, i++);
            }
            return exportFileName;
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
