using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simpchat.Shared.Config
{
    public class MinioSettings
    {
        /// <summary>
        /// Internal endpoint for server-to-server communication (e.g., simpchat.filestorage:9000)
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// Public endpoint for browser-accessible URLs (e.g., localhost:9000)
        /// If not set, falls back to Endpoint
        /// </summary>
        public string PublicEndpoint { get; set; }

        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public bool UseSsl { get; set; }
    }
}
