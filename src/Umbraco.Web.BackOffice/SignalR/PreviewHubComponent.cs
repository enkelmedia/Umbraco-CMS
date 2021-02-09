﻿using System;
using Microsoft.AspNetCore.SignalR;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Sync;
using Umbraco.Core.Cache;
using Umbraco.Core.Sync;
using Umbraco.Web.Cache;

namespace Umbraco.Web.BackOffice.SignalR
{
    public class PreviewHubComponent : IComponent
    {
        private readonly Lazy<IHubContext<PreviewHub, IPreviewHub>> _hubContext;

        // using a lazy arg here means that we won't create the hub until necessary
        // and therefore we won't have too bad an impact on boot time
        public PreviewHubComponent(Lazy<IHubContext<PreviewHub, IPreviewHub>> hubContext)
        {
            _hubContext = hubContext;
        }

        public void Initialize()
        {
            // ContentService.Saved is too soon - the content cache is not ready yet,
            // so use the content cache refresher event, because when it triggers
            // the cache has already been notified of the changes

            ContentCacheRefresher.CacheUpdated += HandleCacheUpdated;
        }

        public void Terminate()
        {
            ContentCacheRefresher.CacheUpdated -= HandleCacheUpdated;
        }

        private async void HandleCacheUpdated(ContentCacheRefresher sender, CacheRefresherEventArgs args)
        {
            if (args.MessageType != MessageType.RefreshByPayload) return;
            var payloads = (ContentCacheRefresher.JsonPayload[])args.MessageObject;
            var hubContextInstance = _hubContext.Value;
            foreach (var payload in payloads)
            {
                var id = payload.Id; // keep it simple for now, ignore ChangeTypes
                await hubContextInstance.Clients.All.refreshed(id);
            }
        }
    }
}
