using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sistema_ERP.ViewComponents
{
    public class SidebarViewComponent : ViewComponent
    {
        private readonly IAuthorizationService _authorizationService;

        public SidebarViewComponent(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {



            return View();
        }
    }
}
