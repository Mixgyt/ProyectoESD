using Microsoft.AspNetCore.Mvc;

namespace Proyecto_Final_Estructura_De_Datos.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
