using log4net;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using NoSqlApp.Utils;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using static System.Console;

namespace NoSqlApp
{
    class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));


        private const string MetadataVehcileType = "vehicleType";
        private static string BlockBlobName;
        private static string ContainerName;

        // Frist get container Reference
        static CloudBlobContainer ContainerReference => Shared.CloudBlobClient.GetContainerReference(ContainerName);
        static CloudBlockBlob CurrentCloudBlockBlob;


        public static void Main()
        {
            InitializeConfiguration();

            MainAsync().Wait();
        }

        private static void InitializeConfiguration()
        {
            IConfigurationBuilder configBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            IConfigurationRoot config = configBuilder.Build();

            ContainerName = config["BlobContainerName"];
            BlockBlobName = config["BlockBlobName"];
        }

        static async Task MainAsync()
        {
            byte[] imageByteArray = GetImageByteArray();

            CurrentCloudBlockBlob = await GetBlockBlobAsync(BlockBlobName);

            if (CurrentCloudBlockBlob != null)
            {
                await UploadImageAsync(imageByteArray);

                await SetMetadataAsync("sedan");

                await FetchBlobInformation();

                await DeleteImageAsync();
            }
            else
            {
                WriteLine("There is no Blob");
            }
        }

        private static async Task<CloudBlockBlob> GetBlockBlobAsync(string blockBlobName)
        {
            //DatabaseResponse databaseResponse = await Shared.Client.CreateDatabaseIfNotExistsAsync(DatabaseId);

            //Till this point NO server side request have been made!!!
            await ContainerReference.CreateIfNotExistsAsync(BlobContainerPublicAccessType.Blob, null, null);

            //Now that is has been created
            return ContainerReference.GetBlockBlobReference(blockBlobName);

        }

        private static async Task FetchBlobInformation()
        {
            await CurrentCloudBlockBlob.FetchAttributesAsync();
            WriteLine(CurrentCloudBlockBlob.Metadata[MetadataVehcileType]);

        }

        private async static Task SetMetadataAsync(string vehicleType)
        {
            try
            {
                CurrentCloudBlockBlob.Metadata[MetadataVehcileType] = vehicleType;

                await CurrentCloudBlockBlob.SetMetadataAsync();
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }

        }

        private static async Task DeleteImageAsync()
        {
            try
            {
                await CurrentCloudBlockBlob.DeleteIfExistsAsync();
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }
        }

        private static byte[] GetImageByteArray()
        {
            byte[] imageByteArray;
            using (var image = Image.FromFile(@"C:\Codigo\BlobStorage\NoSqlApp\Resources\treeswing.png"))
            {
                using (var ms = new MemoryStream())
                {
                    image.Save(ms, image.RawFormat);
                    imageByteArray = ms.ToArray();
                }
            }
            return imageByteArray;
        }

        /// <summary>
        /// Create the Containter if it does not exist and then upload an image as a blob
        /// </summary>
        private static async Task UploadImageAsync(byte[] imageByteArray)
        {
            try
            {
                //If we know it is an image
                //https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/MIME_types/Common_types
                CurrentCloudBlockBlob.Properties.ContentType = "image/png";

                var isExistentBlob = await CurrentCloudBlockBlob.ExistsAsync();

                if (!isExistentBlob)
                    await CurrentCloudBlockBlob.UploadFromByteArrayAsync(imageByteArray, 0, imageByteArray.Length);
                else
                    WriteLine($"Blob {CurrentCloudBlockBlob.Name} already exists in this URI: {CurrentCloudBlockBlob.Uri}");

            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }
        }
    }
}