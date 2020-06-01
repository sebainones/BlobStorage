using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using NoSqlApp.Utils;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace NoSqlApp
{
    class Program
    {

        private static string BlockBlobName;
        private static string ContainerName;

        public static void Main(string[] args)
        {
            MainAsync().Wait();
        }

        static async Task MainAsync()
        {

            IConfigurationBuilder configBuilder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            IConfigurationRoot config = configBuilder.Build();

            ContainerName = config["BlobContainerName"];
            BlockBlobName = config["BlockBlobName"];

            byte[] imageByteArray = GetImageByteArray();

            await Program.UploadImageAsync(imageByteArray, BlockBlobName);
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
        private static async Task UploadImageAsync(byte[] imageByteArray, string blobName)
        {

            try
            {
                // Frist get container Reference
                CloudBlobContainer containerReference = Shared.CloudBlobClient.GetContainerReference(ContainerName);

                //DatabaseResponse databaseResponse = await Shared.Client.CreateDatabaseIfNotExistsAsync(DatabaseId);

                //Till this point NO server side request have been made!!!
                await containerReference.CreateIfNotExistsAsync(BlobContainerPublicAccessType.Blob, null, null);

                //Now that is has been created
                CloudBlockBlob cloudBlockBlob = containerReference.GetBlockBlobReference(blobName);

                var isExistentBlob = await cloudBlockBlob.ExistsAsync();

                if (!isExistentBlob)
                    await cloudBlockBlob.UploadFromByteArrayAsync(imageByteArray, 0, imageByteArray.Length);
                else
                    Console.WriteLine($"Blob already exists!: {cloudBlockBlob.Uri}");

            }
            catch (Exception e)
            {
                //TODO: propery log any exception
                throw;
            }
        }
    }
}