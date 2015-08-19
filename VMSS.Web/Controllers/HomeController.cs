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
           // try
            ///**/{
           
           // InitBass();
                dir = @"d:\music";

            string[] musicFiles = Directory.GetFiles(@"d:\music", "*.mp3");
            foreach (string musicFile in musicFiles)
            {
                using (var mp3 = new Mp3File(musicFile))
                {
                    Id3Tag tag = mp3.GetTag(Id3TagFamily.FileStartTag);
                    Console.WriteLine("Title: {0}", tag.Title.Value);
                    Console.WriteLine("Artist: {0}", tag.Artists.Value);
                    Console.WriteLine("Album: {0}", tag.Album.Value);
                }
            }
            /* if (Bass.BASS_Init(0, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero))
                 throw new Exception("Error initializing audio mixer");*/

            /* int mixer = BassMix.BASS_Mixer_StreamCreate(44100, 2, BASSFlag.BASS_MIXER_END | BASSFlag.BASS_STREAM_DECODE);

             int backgroundStream = Bass.BASS_StreamCreateFile(backgroundFile, 0L, 0L, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_STREAM_PRESCAN);
             int vocalStream = Bass.BASS_StreamCreateFile(vocalFile, 0L, 0L, BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_STREAM_PRESCAN);

             bool backgroundOk = BassMix.BASS_Mixer_StreamAddChannel(mixer, backgroundStream, BASSFlag.BASS_STREAM_AUTOFREE | BASSFlag.BASS_MIXER_DOWNMIX);
             bool vocalOk = BassMix.BASS_Mixer_StreamAddChannel(mixer, vocalStream, BASSFlag.BASS_STREAM_AUTOFREE | BASSFlag.BASS_MIXER_DOWNMIX);

             if (!vocalOk)
                 throw new Exception("Error reading vocal stream");
             else if (!backgroundOk)
                 throw new Exception("Error reading background stream");

             //audio volume adjustment
             Bass.BASS_ChannelSetAttribute(vocalStream, BASSAttribute.BASS_ATTRIB_VOL, (float)1.0);
             Bass.BASS_ChannelSetAttribute(backgroundStream, BASSAttribute.BASS_ATTRIB_VOL, (float)0.3);

             string cmdLine = HostingEnvironment.MapPath("\\bin\\lame.exe") + " --alt-preset standard - " + outFile;
             int encoder = BassEnc.BASS_Encode_Start(mixer, cmdLine, 0, null, IntPtr.Zero);

             while (Bass.BASS_ChannelIsActive(mixer) == BassEnc.BASS_Encode_IsActive(encoder))
             {
                 Byte[] buf = new Byte[50000];
                 Bass.BASS_ChannelGetData(mixer, buf, buf.Length);
             }

             BassEnc.BASS_Encode_Stop(mixer);
             Bass.BASS_StreamFree(mixer);
         }
         catch (Exception ex)
         {
             throw ex;
         }
         finally
         {
             //release resources
             Bass.BASS_Free();
             Bass.FreeMe();

             BassEnc.FreeMe();
             BassMix.FreeMe();
         }*/

            return View();
        }

        private string InitBass()
        {
            string _ret = "";

            BassNet.Registration("jthadison@gmail.com", "2X19181419152222");


            
            if (!Bass.BASS_Init(0, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero))
            {
                // Bass_Init Error!            
                _ret = Bass.BASS_ErrorGetCode().ToString();
            }
            else
            {
                _ret = "BASS_OK";
            }

            return _ret;
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