using Microsoft.AspNetCore.Mvc;
using Proyecto_Final_Estructura_De_Datos.Models;
using System.Diagnostics;
using System.Dynamic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Proyecto_Final_Estructura_De_Datos.Controllers
{
	public class HomeController : Controller
	{
		private readonly DbHotelContext _context;

        public HomeController(DbHotelContext context)
        {
            _context = context;
        }

		public IActionResult Index()
		{
			dynamic model = new ExpandoObject();
			model.Habitaciones = getHabitacionesMedianas();
            return View(model);
		}

		public IActionResult Servicios() 
		{
			return View();
		}

        [HttpGet]
        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registro(Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                usuario.Rol = RolUsuario.Usuario;
                _context.Add(usuario);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(usuario);
        }

        public IActionResult Login()
		{
			return View();
		}

		public IActionResult Habitaciones()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

        private List<Habitacion> getHabitacionesMedianas()
        {
			return _context.Habitaciones.ToList().FindAll(x=>x.Tipo==TipoHabitacion.Mediana);
        }

        private List<Habitacion> getHabitacionesGrandes()
        {
            return _context.Habitaciones.ToList().FindAll(x=>x.Tipo==TipoHabitacion.Grande);
        }

        private List<Habitacion> getHabitacionesPresidenciales()
        {
            return _context.Habitaciones.ToList().FindAll(x => x.Tipo == TipoHabitacion.Presidencial);
        }
    }
}
