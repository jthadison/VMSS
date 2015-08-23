using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Id3;

namespace VMSS.Web.Controllers
{
    public class UploadMp3sController : Controller
    {
        // GET: UploadMp3s
        public ActionResult Index()
        {


            return View();
        }

        [HttpPost]
        public ActionResult Index(IEnumerable<HttpPostedFileBase> files)
        {
            var genre = string.Empty;
            foreach (var file in files)
            {
                if (file.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var webroot = Server.MapPath("~");

                    genre = GetGenre(file);

                    var path = Path.Combine(webroot, @"..\Music\" + genre + @"\" + fileName);
                    file.SaveAs(path);
                }
            }

            Process process = new Process();
            var processPath =
                @"d:\Users\junior\Documents\Visual Studio 2013\Projects\LoadMp3s\ConsoleApplication1\bin\Debug\";
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