﻿using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Web.Common.Attributes;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Web.BackOffice.Controllers
{
    /// <summary>
    /// The API controller used for retrieving available stylesheets
    /// </summary>
    [PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
    public class StylesheetController : UmbracoAuthorizedJsonController
    {
        private readonly IFileService _fileService;

        public StylesheetController(IFileService fileService)
        {
            _fileService = fileService;
        }

        public IEnumerable<Stylesheet> GetAll()
        {
            return _fileService.GetStylesheets()
                .Select(x =>
                    new Stylesheet() {
                        Name = x.Alias,
                        Path = x.VirtualPath
                    });
        }

        public IEnumerable<StylesheetRule> GetRulesByName(string name)
        {
            var css = _fileService.GetStylesheetByName(name.EnsureEndsWith(".css"));
            if (css == null)
                return Enumerable.Empty<StylesheetRule>();

            return css.Properties.Select(x => new StylesheetRule() { Name = x.Name, Selector = x.Alias });
        }
    }

}
