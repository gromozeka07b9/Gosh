using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using QuestHelper.Server.Models;

namespace QuestHelper.Server.Managers
{
    public class MediaManager
    {
        private string _pathToMediaCatalog;
        private string _pathToMediaCatalogShared;
        public MediaManager()
        {
            _pathToMediaCatalog = System.Environment.GetEnvironmentVariable("GOSH_IMAGES_PATH");
            _pathToMediaCatalogShared = System.Environment.GetEnvironmentVariable("GOSH_IMAGES_SHARED_PATH");
        }

        public string PathToMediaCatalog
        {
            get
            {
                return _pathToMediaCatalog; 
            }
        }
        public string PathToSharedMediaCatalog
        {
            get
            {
                return _pathToMediaCatalogShared;
            }
        }
        internal bool MediaFileExist(string filename)
        {
            return File.Exists(Path.Combine(_pathToMediaCatalog, filename));
        }
        internal bool SharedMediaFileExist(string filename)
        {
            return File.Exists(Path.Combine(_pathToMediaCatalogShared, filename));
        }

        internal void DownloadToStream(MemoryStream memoryStream, string filename)
        {
            using (FileStream fileStream = File.OpenRead(Path.Combine(_pathToMediaCatalog, filename)))
            {
                memoryStream.SetLength(fileStream.Length);
                fileStream.Read(memoryStream.GetBuffer(), 0, (int) fileStream.Length);
            }
        }

        internal bool CopyMediaFileToSharedCatalog(string filename)
        {
            bool result = false;
            try
            {
                System.IO.File.Copy(Path.Combine(_pathToMediaCatalog, filename), Path.Combine(_pathToMediaCatalogShared, filename), true);
                Console.WriteLine($"Copied preview: imgFileName:{filename}");
                result = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return result;
        }

        public string ConvertMediafileToBase64(string filename)
        {
            try
            {
                Byte[] imgBytes = System.IO.File.ReadAllBytes(Path.Combine(_pathToMediaCatalog, filename));
                return Convert.ToBase64String(imgBytes);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return String.Empty;
        }
        
        public void PublishImages(List<RoutePointMediaObject> mediaObjects)
        {
            foreach (var media in mediaObjects)
            {
                string filePrefix = media.MediaType == MediaObjectTypeEnum.Image ? "img" : "audio";
                string fileExtension = media.MediaType == MediaObjectTypeEnum.Image ? "jpg" : "3gp";
                string imgFileName = $"{filePrefix}_{media.RoutePointMediaObjectId.ToLowerInvariant()}.{fileExtension}";
                if (!SharedMediaFileExist(imgFileName))
                {
                    bool copied = CopyMediaFileToSharedCatalog(imgFileName);
                    if (!copied)
                    {
                        Console.WriteLine("Error while coping file:" + imgFileName);
                    }
                }

                if (media.MediaType == MediaObjectTypeEnum.Image)
                {
                    string imgPreviewFileName = $"{filePrefix}_{media.RoutePointMediaObjectId.ToLowerInvariant()}_preview.{fileExtension}";
                    if (!SharedMediaFileExist(imgPreviewFileName))
                    {
                        CopyMediaFileToSharedCatalog(imgPreviewFileName);
                    }
                }
            }
        }
    }
}
