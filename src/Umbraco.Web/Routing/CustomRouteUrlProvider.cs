﻿using System;
using System.Collections.Generic;
using System.Globalization;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// This url provider is used purely to deal with umbraco custom routes that utilize <see cref="UmbracoVirtualNodeRouteHandler"/> and will return
    /// the URL returned from the current PublishedContentRequest.PublishedContent (virtual node) if the request is in fact a virtual route and
    /// the id that is being requested matches the id of the current PublishedContentRequest.PublishedContent.
    /// </summary>
    internal class CustomRouteUrlProvider : IUrlProvider
    {
        /// <summary>
        /// This will return the URL that is returned by the assigned custom <see cref="IPublishedContent"/> if this is a custom route
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="id"></param>
        /// <param name="current"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public string GetUrl(UmbracoContext umbracoContext, int id, Uri current, UrlProviderMode mode, string culture = null)
        {
            if (umbracoContext == null) return null;
            if (umbracoContext.PublishedRequest == null) return null;
            if (umbracoContext.PublishedRequest.PublishedContent == null) return null;
            if (umbracoContext.HttpContext == null) return null;
            if (umbracoContext.HttpContext.Request == null) return null;
            if (umbracoContext.HttpContext.Request.RequestContext == null) return null;
            if (umbracoContext.HttpContext.Request.RequestContext.RouteData == null) return null;
            if (umbracoContext.HttpContext.Request.RequestContext.RouteData.DataTokens == null) return null;
            if (umbracoContext.HttpContext.Request.RequestContext.RouteData.DataTokens.ContainsKey(Umbraco.Core.Constants.Web.CustomRouteDataToken) == false) return null;

            //If we get this far, it means it's a custom route with published content assigned, check if the id being requested for is the same id as the assigned published content
            //NOTE: This looks like it might cause an infinite loop because PublishedContentBase.Url calls into UmbracoContext.Current.UrlProvider.GetUrl which calls back into the IUrlProvider pipeline
            // but the specific purpose of this is that a developer is using their own IPublishedContent that returns a specific Url and doesn't go back into the UrlProvider pipeline.
            //TODO: We could put a try/catch here just in case, else we could do some reflection checking to see if the implementation is PublishedContentBase and the Url property is not overridden.
            return id == umbracoContext.PublishedRequest.PublishedContent.Id
                ? umbracoContext.PublishedRequest.PublishedContent.Url
                : null;
        }

        /// <summary>
        /// This always returns null because this url provider is used purely to deal with Umbraco custom routes with
        /// UmbracoVirtualNodeRouteHandler, we really only care about the normal URL so that RedirectToCurrentUmbracoPage() works
        /// with SurfaceControllers
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="id"></param>
        /// <param name="current"></param>
        /// <returns></returns>
        public IEnumerable<string> GetOtherUrls(UmbracoContext umbracoContext, int id, Uri current)
        {
            return null;
        }
    }
}
