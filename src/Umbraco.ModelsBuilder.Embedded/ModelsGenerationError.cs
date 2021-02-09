using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Core.Configuration;

namespace Umbraco.ModelsBuilder.Embedded
{
    public sealed class ModelsGenerationError
    {
        private readonly ModelsBuilderSettings _config;
        private readonly IHostingEnvironment _hostingEnvironment;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelsGenerationError"/> class.
        /// </summary>
        public ModelsGenerationError(IOptions<ModelsBuilderSettings> config, IHostingEnvironment hostingEnvironment)
        {
            _config = config.Value;
            _hostingEnvironment = hostingEnvironment;
        }

        public void Clear()
        {
            var errFile = GetErrFile();
            if (errFile == null)
            {
                return;
            }

            // "If the file to be deleted does not exist, no exception is thrown."
            File.Delete(errFile);
        }

        public void Report(string message, Exception e)
        {
            var errFile = GetErrFile();
            if (errFile == null)
            {
                return;
            }

            var sb = new StringBuilder();
            sb.Append(message);
            sb.Append("\r\n");
            sb.Append(e.Message);
            sb.Append("\r\n\r\n");
            sb.Append(e.StackTrace);
            sb.Append("\r\n");

            File.WriteAllText(errFile, sb.ToString());
        }

        public string GetLastError()
        {
            var errFile = GetErrFile();
            if (errFile == null)
            {
                return null;
            }

            try
            {
                return File.ReadAllText(errFile);
            }
            catch
            {
                // accepted
                return null;
            }
        }

        private string GetErrFile()
        {
            var modelsDirectory = _config.ModelsDirectoryAbsolute(_hostingEnvironment);
            if (!Directory.Exists(modelsDirectory))
            {
                return null;
            }

            return Path.Combine(modelsDirectory, "models.err");
        }
    }
}
