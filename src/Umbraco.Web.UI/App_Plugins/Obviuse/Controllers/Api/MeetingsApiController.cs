using System.Collections.Generic;
using System.Web.Http;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.UI.App_Plugins.Obviuse.Controllers.Api
{
    // Routes to: /Umbraco/Api/Meetings/{action}
    public class MeetingsController : UmbracoApiController
    {
        private readonly IMeetingsService _meetingsService;

        public MeetingsController(IMeetingsService meetingsService)
        {
            _meetingsService = meetingsService;
        }

        [HttpGet]
        public List<string> GetAll()
        {
            return new List<string>(){"foo","bar","yada","umbraco"};
        }

    }
}
