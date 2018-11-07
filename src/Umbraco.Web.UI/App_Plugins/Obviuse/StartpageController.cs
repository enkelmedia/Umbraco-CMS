using System.Linq;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.PublishedModels;

namespace Umbraco.Web.UI.App_Plugins.Obviuse
{
    public class StartpageController : RenderMvcController
    {
        private readonly IMarkusService _markusService;

        public StartpageController(IMarkusService markusService)
        {
            _markusService = markusService;
        }

        public ActionResult Index(ContentModel<Startpage> model)
        {
            var vm = new StartPageViewModel(model.Content);
            
            _markusService.DoStuff();
            var content = Umbraco.ContentAtRoot().First();

            vm.CustomProperty = content.Name;

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
