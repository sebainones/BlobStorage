using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using System;

namespace NoSqlApp.Utils
{
    public static class Shared
    {
        private static string BlobStorageConnectionString => Environment.GetEnvironmentVariable("BlobStorageConnectionString");

        private static readonly CloudStorageAccount cloudStorageAccount;

        public static CloudBlobClient CloudBlobClient;

        static Shared()
        {
            cloudStorageAccount = CloudStorageAccount.Parse(BlobStorageConnectionString);

            CloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
        }
    }
}
