using MediaToolkit;
using Newtonsoft.Json;
using VideoLibrary;

namespace YoutubeDownloader
{
    public class Program
    {
        public static readonly Lazy<HttpClient> LazyHttpClient = new();
        public static readonly HttpClient httpCLient = LazyHttpClient.Value;

        static async Task Main(string[] args)
        {
            Console.Title = "Youtube Downloader";

            string apiKey = "YOUR API KEY";            

            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\downloaded videos";

            Console.WriteLine("enter the link of youtube playlist");
            string id = Console.ReadLine()![38..];

            maxResult:
            Console.WriteLine("enter the max result");
            string maxResult = Console.ReadLine()!;

            maxResult ??= "1";

            if (int.TryParse(maxResult, out _))
            {
                Console.WriteLine("this is wrong");
                goto maxResult;
            }

            string jsonUrl = $"https://www.googleapis.com/youtube/v3/playlistItems?part=snippet&maxResults={maxResult}&playlistId={id}&key={apiKey}";

            var responseMessage = await httpCLient.GetAsync(jsonUrl);

            if (!responseMessage.IsSuccessStatusCode)
            {
                Console.WriteLine("somethings wrong");
                return;
            }

            var content = await responseMessage.Content.ReadAsStringAsync();
            Root playlistInfo = JsonConvert.DeserializeObject<Root>(content);

            Console.WriteLine("Videos ; \n");
            foreach (var item in playlistInfo.Items)
            {
                Console.WriteLine(item.Snippet.Title);
            }

            Console.WriteLine("Press any key to continue");
            Console.ReadKey();

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var yt = YouTube.Default;

            foreach (var item in playlistInfo.Items)
            {
                try
                {
                    var video = await yt.GetVideoAsync("https://www.youtube.com/watch?v=" + item.Snippet.ResourceId?.VideoId);
                    File.WriteAllBytes(path + "\\" + video.FullName, await video.GetBytesAsync());

                    await Task.Run(() =>
                    {
                        var inputFile = new MediaToolkit.Model.MediaFile { Filename = path + "\\" + video.FullName };
                        var outputFile = new MediaToolkit.Model.MediaFile { Filename = (path + "\\" + video.FullName + ".mp3").Replace(".mp4", "") };

                        using (var enging = new Engine())
                        {
                            enging.GetMetadata(inputFile);
                            enging.Convert(inputFile, outputFile);
                        }

                        File.Delete(path + "\\" + video.FullName);
                    });

                    await Task.Run(() => Console.WriteLine("Downloaded video : " + item.Snippet.Title));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"!!! [{ex.Source}] => {ex.Message} !!!");
                }
            }

            Console.ReadKey();
        }
    }
}
