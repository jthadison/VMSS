using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Id3;
using NLog;
using NLog.Fluent;

namespace VMSS.Web.Controllers
{
   public class UploadMp3sController : Controller
    {
        Logger logger = LogManager.GetCurrentClassLogger();

        // GET: UploadMp3s
        public ActionResult Index()
        {


            return View();
        }

        [HttpPost]
        public ActionResult Index(IEnumerable<HttpPostedFileBase> files)
        {
            var genre = string.Empty;
            var webroot = Server.MapPath("~");
            var musicFolder = @"E:\HostingSpaces\vbreeden\vibestreams.com\Music\";
            

            foreach (var file in files)
            {
                if (file.ContentLength > 0)
                {
                    
                    var fileName = Path.GetFileName(file.FileName);
                    genre = GetGenre(file);
                    logger.Info(musicFolder + genre);
                    bool exists = System.IO.Directory.Exists(musicFolder+genre);

                    if (!exists)
                        System.IO.Directory.CreateDirectory(musicFolder);

                    var saveDir = musicFolder;

                    var path = Path.Combine(webroot, saveDir + fileName);
                    file.SaveAs(path);
                }
            }

            Process process = new Process();
            /*var processPath =
                @"d:\Users\junior\Documents\Visual Studio 2013\Projects\LoadMp3s\ConsoleApplication1\bin\Debug\";*/
            var processPath = @"E:\HostingSpaces\vbreeden\vibestreams.com\BassAudioConsoleApp\";
            logger.Info("Console App Path: " + processPath);
            process.StartInfo.FileName = processPath + "ConsoleApplication1.exe";
            process.Start();
            return View();
        }

        private string GetGenre(HttpPostedFileBase file)
        {
            var genre = string.Empty;
            using (var mp3 = new Mp3Stream(file.InputStream))
            {
                Id3Tag tag = mp3.GetTag(Id3TagFamily.FileStartTag);
                genre = tag.Genre;
                Console.WriteLine("Title: {0}", tag.Title.Value);
                Console.WriteLine("Artist: {0}", tag.Artists.Value);
                Console.WriteLine("Album: {0}", tag.Album.Value);

            }

            return genre;
        }
    }
}