using System.Linq;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.PublishedModels;

namespace Umbraco.Web.UI.App_Plugins.Obviuse
{
    public class StartpageController : RenderMvcController
    {
        private readonly IMarkusService _markusService;
        private readonly IContentService _contentService;

        public StartpageController(IMarkusService markusService, IContentService contentService)
        {
            _markusService = markusService;
            _contentService = contentService;
        }

        public ActionResult Index(ContentModel<Startpage> model)
        {
            var vm = new StartPageViewModel(model.Content);
            
            _markusService.DoStuff();
            var contentFromCache = Umbraco.ContentAtRoot().First();

            var con = Current.Services.ContentService.GetById(1071);
            vm.CustomProperty = con.Name + " " + contentFromCache.Name;

            vm.CustomProperty += ", service: " + _contentService.GetById(1071);

            // Do some stuff here, then return the base method
            return base.Index(vm);
        }

    }

    public class StartPageViewModel : ContentModel<Startpage>
    {
        public StartPageViewModel(Startpage content) : base(content)
        {
        }

        public string CustomProperty { get; set; }

    }

    public class ContactFormSurfaceController : SurfaceController
    {

        [ChildActionOnly]
        public ActionResult RenderForm()
        {
            var viewModel = new ContactFormModel {};
            return PartialView("ContactForm", viewModel);
        }

        [HttpPost]
        public ActionResult HandleForm(ContactFormModel model)
        {
            return RedirectToCurrentUmbracoPage();
        }
    }

    public class ContactFormModel
    {
        public string Name { get; set; }
        public string Phone { get; set; }
    }
}
