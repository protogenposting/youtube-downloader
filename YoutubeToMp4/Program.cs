using YoutubeExplode;

namespace YouTubeDownloader
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Set the output directory path here
            string outputDirectory = @"D:\YouTubeVideo";

            // List of YouTube video URLs to download
            List<string> videoUrls = new List<string>
            {

                "https://www.youtube.com/watch?v=YT8rY_o5VhY&ab_channel=ZeeMusicCompany",
                "https://www.youtube.com/watch?v=HFX6AZ5bDDo&ab_channel=ZeeMusicCompany",
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
            }
            else
            {
                Console.WriteLine($"No suitable video stream found for {video.Title}.");
            }
        }
    }
}