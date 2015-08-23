using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Windows.Forms.VisualStyles;
using Id3;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Enc;
using Un4seen.Bass.AddOn.Mix;
using Un4seen.Bass.AddOn.Tags;
using Un4seen.Bass.AddOn.Wma;
using VMSS.Web.Models;

namespace VMSS.Web.Controllers
{
    public class HomeController : Controller
    {
        private int DeviceId;
        private static string dir;
        private static int count = 0;
        public ActionResult Index()
        {
           
            
            return View();
        }

        [HttpPost]
        public ActionResult Index(IEnumerable<HttpPostedFileBase> files)
        {

            foreach (var file in files)
            {
                if (file.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var path = Path.Combine(Server.MapPath("~/App_Data/uploads"), fileName);
                    file.SaveAs(path);
                }
            }

            

            return View();
        }

        static int DirSearch(string dir)
        {

            try
            {

                foreach (string f in Directory.GetFiles(dir))
                {
                    if (f.EndsWith(".mp3"))
                    {
                        Trace.WriteLine(f);
                        var mp3 = GetMetadata(f);
                        //var tag = BassTags.BASS_TAG_GetFromFile(f);
                        ReadFile(mp3);
                        //InsertMetadata(mp3);
                        // Trace.WriteLine(tag.artist);
                        count++;
                    }

                }

                foreach (string d in Directory.GetDirectories(dir))
                {
                    Console.WriteLine(d.ToString());
                    DirSearch(d);
                }


            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return count;
        }
        private static Mp3Data GetMetadata(string file)
        {

            /* if (Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_FREQ, IntPtr.Zero))
             {
                 BassFx.BASS_FX_GetVersion();
             }*/

            var metadata = new Mp3Data();
            int channel = Bass.BASS_StreamCreateFile(file, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT);
            //IntPtr tag = Bass.BASS_ChannelGetTags(channel, BASSTag.BASS_TAG_ID3V2);
            TAG_INFO tagInfo = new TAG_INFO(file);
            //string[] tags = Utils.IntPtrToArrayNullTermUtf8(tag);

            if (BassTags.BASS_TAG_GetFromFile(channel, tagInfo))
            {
                Trace.WriteLine("Album: " + tagInfo.album);
                Trace.WriteLine("Artist: " + tagInfo.artist);
                Trace.WriteLine("Title: " + tagInfo.title);
                Trace.WriteLine("Comment: " + tagInfo.comment);
                Trace.WriteLine("Genre: " + tagInfo.genre);
                Trace.WriteLine("Year: " + tagInfo.year);
                Trace.WriteLine("Track: " + tagInfo.track);

                metadata = AutoMapper.Mapper.Map<Mp3Data>(tagInfo);


            }
            metadata.Name = file;

            /*if (tags != null)
            {
                foreach (string ttag in tags)
                    Console.WriteLine("Tag: {0}\n", ttag);
            }*/

            return metadata;

        }

        private static void ReadFile(Mp3Data m)
        {
            FileStream fs = new FileStream(m.Name, FileMode.Open, FileAccess.Read);

            BinaryReader br = new BinaryReader(fs);
            Byte[] bytes = br.ReadBytes((Int32)fs.Length);
            br.Close();
            fs.Close();

            // string strQuery = "insert into mp3s(FileName, Data) values (@Name,  @Album, @Artist,  @Genre,@Year,,@Data)";
            string strQuery = "insert into mp3s(FileName,Album,Artist,Title,Comment,Genre, Year,Track,DateAdded,Data) values (@Name,@Album,@Artist,@Title,@Comment,@Genre,@Year,@Track, @Date,@Data)";
            SqlCommand cmd = new SqlCommand(strQuery);
            cmd.Parameters.Add("@Name", SqlDbType.VarChar).Value = m.Name;
            cmd.Parameters.Add("@Data", SqlDbType.VarBinary).Value = bytes;
            cmd.Parameters.Add("@Artist", SqlDbType.VarChar).Value = m.Artist;
            cmd.Parameters.Add("@Album", SqlDbType.VarChar).Value = m.Album;
            cmd.Parameters.Add("@Title", SqlDbType.VarChar).Value = m.Title;
            cmd.Parameters.Add("@Comment", SqlDbType.VarChar).Value = m.Comment;
            cmd.Parameters.Add("@Genre", SqlDbType.VarChar).Value = m.Genre;
            cmd.Parameters.Add("@Year", SqlDbType.VarChar).Value = m.Year;
            cmd.Parameters.Add("@Track", SqlDbType.VarChar).Value = m.Track;
            cmd.Parameters.Add("@Date", SqlDbType.VarChar).Value = DateTime.UtcNow;
            InsertUpdateData(cmd);

        }


        private static Boolean InsertUpdateData(SqlCommand cmd)
        {
            String strConnString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
            SqlConnection con = new SqlConnection(strConnString);
            cmd.CommandType = CommandType.Text;
            cmd.Connection = con;
            try
            {
                con.Open();
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                Trace.Write(ex.Message);
                return false;
            }
            finally
            {
                con.Close();
                con.Dispose();
            }
        }

        private static void InitializeAutomapper()
        {
            AutoMapper.Mapper.CreateMap<TAG_INFO, Mp3Data>().ReverseMap();
        }

       /* public void Play()
        {
            var dummyWindow = new System.Windows.Forms.Form();
            var result = Bass.BASS_Init(DeviceId, 44100, BASSInit.BASS_DEVICE_SPEAKERS, dummyWindow.Handle);

            Bass.BASS_StreamFree(_stream);

            Bass.BASS_SetDevice(DeviceId);

            if (VisualStyleElement.TrackBar.Track.EndsWith(".wma"))
            {
                _stream = BassWma.BASS_StreamCreateFile(VisualStyleElement.TrackBar.Track, 0, 0, Un4seen.Bass.BASSFlag.BASS_AAC_STEREO);
            }
            else if (VisualStyleElement.TrackBar.Track.ToLower().EndsWith(".mp3") || VisualStyleElement.TrackBar.Track.ToLower().EndsWith(".wav"))
            {
                _stream = Bass.BASS_StreamCreateFile(VisualStyleElement.TrackBar.Track, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_PRESCAN);
            }

            Bass.BASS_ChannelPlay(_stream, false);
        }

        public void Stop()
        {
            var dummyWindow = new System.Windows.Forms.Form();
            var result = Bass.BASS_Init(DeviceId, 44100, BASSInit.BASS_DEVICE_SPEAKERS, dummyWindow.Handle);

            Bass.BASS_SetDevice(DeviceId);

            if (VisualStyleElement.TrackBar.Track.ToLower().EndsWith(".wma"))
            {
                _stream = BassWma.BASS_WMA_StreamCreateFile(VisualStyleElement.TrackBar.Track, 0, 0, Un4seen.Bass.BASSFlag.BASS_AAC_STEREO);
            }
            else if (VisualStyleElement.TrackBar.Track.EndsWith(".mp3") || VisualStyleElement.TrackBar.Track.ToLower().EndsWith(".wav"))
            {
                VisualStyleElement.TrackBar.Track.Normal.
                _stream = Bass.BASS_StreamCreateFile(VisualStyleElement.TrackBar.Track, 0, 0, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_PRESCAN);
            }

            //Free the stream
            Bass.BASS_StreamFree(_stream);

            //Free BASS
            Bass.BASS_Free();
        }
*/


        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}