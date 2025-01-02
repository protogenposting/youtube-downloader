using YoutubeExplode;
using FFMpegCore;

namespace YouTubeDownloader
{
    class Program
    {
        static YoutubeClient youtube = new YoutubeClient();
        static List<string> exports = new List<string>{};
        static async Task Main(string[] args)
        {
            // Set the output directory path here
            string outputDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location)+"/videos";
            var convert = new NReco.VideoConverter.FFMpegConverter();
            Directory.CreateDirectory(outputDirectory);
            Console.WriteLine("Started Program");
            // List of YouTube video URLs to download

            List<string> videoUrls = GetUrls();

            Console.WriteLine("Urls:");
            foreach (var videoUrl in videoUrls)
            {
                Console.WriteLine(videoUrl);
            }
            try
            {
                foreach (var videoUrl in videoUrls)
                {
                    try
                    {
                        Console.WriteLine("-----------------");
                        await DownloadYouTubeVideo(videoUrl, outputDirectory);
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("Error downloading "+videoUrl);
                        Console.WriteLine(e.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while downloading the videos: " + ex.Message);
            }
            Console.WriteLine("Do you want mp3 files? (y/n)");
            string read = Console.ReadLine();
            while(read != "y" && read != "n")
            {
                read = Console.ReadLine();
            }

            if(read == "y")
            {
                Console.WriteLine("-----------------");
                Console.WriteLine("MP3 TIEM!!!");
                //convert them all to mp3's
                //delete this for loop if you don't want that
                foreach (var video in exports)
                {
                    Console.WriteLine("-----------------");
                    Console.WriteLine("Converting "+video);
                    convert.ConvertMedia(
                        video,
                        video+".mp3",
                        "mp3");
                    File.Delete(video);
                }
            }
            Console.WriteLine("Done!");
        }
        static List<string> GetUrls(){
            string[] lines = File.ReadAllLines(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location)+"/urls.txt");

            List<string> returnList = new List<string>();

            foreach (var videoUrl in lines)
            {
                returnList.Add(videoUrl);
            }

            return returnList;
        }
        static async Task DownloadYouTubeVideo(string videoUrl, string outputDirectory)
        {
            Console.WriteLine("Waiting...");
            var video = await youtube.Videos.GetAsync(videoUrl);
            Console.WriteLine("Got Video!");

            // Sanitize the video title to remove invalid characters from the file name
            string sanitizedTitle = string.Join("_", video.Title.Split(Path.GetInvalidFileNameChars()));

            Console.WriteLine("Started: "+sanitizedTitle);
            Console.WriteLine("Getting Streams...");
            // Get all available muxed streams
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
            var muxedStreams = streamManifest.GetMuxedStreams().OrderByDescending(s => s.VideoQuality).ToList();

            if (muxedStreams.Any())
            {
                var streamInfo = muxedStreams.First();
                using var httpClient = new HttpClient();
                var stream = await httpClient.GetStreamAsync(streamInfo.Url);
                var datetime = DateTime.Now;
                
                Console.WriteLine("Downloading...");
                string outputFilePath = Path.Combine(outputDirectory, $"{sanitizedTitle}.{streamInfo.Container}");
                if(!File.Exists(outputFilePath))
                {
                    using var outputStream = File.Create(outputFilePath);
                    await stream.CopyToAsync(outputStream);
                }

                Console.WriteLine("Download completed!");
                Console.WriteLine($"Video saved as: {outputFilePath}{datetime}");

                exports.Add(outputFilePath);
            }
            else
            {
                Console.WriteLine($"No suitable video stream found for {video.Title}.");
            }
        }
    }
}