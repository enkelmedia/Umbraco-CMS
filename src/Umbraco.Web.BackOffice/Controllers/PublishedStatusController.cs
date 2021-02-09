using System;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web.BackOffice.Controllers
{
    public class PublishedStatusController : UmbracoAuthorizedApiController
    {
        private readonly IPublishedSnapshotStatus _publishedSnapshotStatus;

        public PublishedStatusController(IPublishedSnapshotStatus publishedSnapshotStatus)
        {
            _publishedSnapshotStatus = publishedSnapshotStatus ?? throw new ArgumentNullException(nameof(publishedSnapshotStatus));
        }

        [HttpGet]
        public string GetPublishedStatusUrl()
        {
            if (!string.IsNullOrWhiteSpace(_publishedSnapshotStatus.StatusUrl))
            {
                return _publishedSnapshotStatus.StatusUrl;
            }

            throw new NotSupportedException("Not supported: " + _publishedSnapshotStatus.GetType().FullName);
        }
    }
}
