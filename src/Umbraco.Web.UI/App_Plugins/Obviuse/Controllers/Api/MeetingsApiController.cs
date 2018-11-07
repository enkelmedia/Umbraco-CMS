using System.Collections.Generic;
using System.Web.Http;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.UI.App_Plugins.Obviuse.Controllers.Api
{
    // Routes to: /Umbraco/Api/Meetings/{action}
    public class MeetingsController : UmbracoApiController
    {
        private readonly IMarkusService _markusService;

        public MeetingsController(IMarkusService markusService)
        {
            _markusService = markusService;
        }

        [HttpGet]
        public List<string> GetAll()
        {
            _markusService.DoStuff();
            return new List<string>(){"foo","bar","yada","umbraco"};
        }

    }
}
