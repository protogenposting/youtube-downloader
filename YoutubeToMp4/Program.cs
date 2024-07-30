using YoutubeExplode;
using FFMpegCore;

namespace YouTubeDownloader
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Set the output directory path here
            string outputDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location)+"/videos";
            Directory.CreateDirectory(outputDirectory);

            // List of YouTube video URLs to download
            List<string> videoUrls = new List<string>
            {
                "https://youtu.be/dHlpa5utQzQ?si=p9dfdsyYug3quKux",
                "https://youtu.be/weZKm1kTrpc?si=RQnT4j9LiauyfAUP",
                "https://youtu.be/kXMwZNRiPe0?si=QLE5J30SjXtyTwNS",
                "https://youtu.be/9Zj0JOHJR-s?si=0_57adxXuFW4_0rx",
                "https://youtu.be/EWjZOxs87yg?si=-TbodDxcSEStDB0w",
                "https://youtu.be/rRzDNQPzbs4?si=kRRfgx_FQIhoWR_M",
                "https://youtu.be/lCaun_EiJZQ?si=68xxVoxdNU3vgKH4"
                // Add more video URLs as needed
            };
            try
            {
                foreach (var videoUrl in videoUrls)
                {
                    await DownloadYouTubeVideo(videoUrl, outputDirectory);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while downloading the videos: " + ex.Message);
            }
        }
        static async Task DownloadYouTubeVideo(string videoUrl, string outputDirectory)
        {
            var youtube = new YoutubeClient();
            var video = await youtube.Videos.GetAsync(videoUrl);

            // Sanitize the video title to remove invalid characters from the file name
            string sanitizedTitle = string.Join("_", video.Title.Split(Path.GetInvalidFileNameChars()));

            // Get all available muxed streams
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
            var muxedStreams = streamManifest.GetMuxedStreams().OrderByDescending(s => s.VideoQuality).ToList();

            if (muxedStreams.Any())
            {
                var streamInfo = muxedStreams.First();
                using var httpClient = new HttpClient();
                var stream = await httpClient.GetStreamAsync(streamInfo.Url);
                var datetime = DateTime.Now;

                string outputFilePath = Path.Combine(outputDirectory, $"{sanitizedTitle}.{streamInfo.Container}");
                using var outputStream = File.Create(outputFilePath);
                await stream.CopyToAsync(outputStream);

                Console.WriteLine("Download completed!");
                Console.WriteLine($"Video saved as: {outputFilePath}{datetime}");
                FFMpegArguments
                .FromFileInput(outputFilePath)
                .OutputToFile(outputFilePath+".mp3")
                .ProcessSynchronously();
            }
            else
            {
                Console.WriteLine($"No suitable video stream found for {video.Title}.");
            }
        }
    }
}