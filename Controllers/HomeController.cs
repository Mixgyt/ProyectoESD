using Microsoft.AspNetCore.Mvc;
using Proyecto_Final_Estructura_De_Datos.Models;
using System.Diagnostics;
using System.Dynamic;
using Microsoft.AspNetCore.Http;
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
			var userId = ControllerContext.HttpContext.Request.Cookies["UserID"];
			if (userId != null)
			{
				Usuario usuario = _context.Usuarios.FirstOrDefault(u => u.IdUsuario == int.Parse(userId));
				ViewData["Usuario"] = new Usuario()
				{
					IdUsuario = 1,
					Nombre = "Hola",
				};
			}
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                usuario.Rol = RolUsuario.Usuario;
                _context.Add(usuario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Login));
            }
            return View(usuario);
        }

        public IActionResult Login()
		{
			return View();
		}
		
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Login([FromForm] Usuario usuario)
		{
			Usuario usuarioRetornado = _context.Usuarios.FirstOrDefault(u => u.Nombre == usuario.Nombre && u.Clave == usuario.Clave);
			if (usuarioRetornado != null)
			{
				CookieOptions option = new CookieOptions();
				option.Expires = DateTime.Now.AddDays(7);
				option.Path = "/";
				Response.Cookies.Append("UserID", usuarioRetornado.IdUsuario.ToString(), option);
				return RedirectToAction(nameof(Index));
			}

			usuario.Clave = "";
			ViewData["Error"] = "Error usuario o contraseña incorrecta";
			return View(usuario);
		}
		
		public IActionResult Logout()
		{
			CookieOptions option = new CookieOptions();
			option.Expires = DateTime.Now.AddDays(7);
			option.Path = "/";
			Response.Cookies.Append("UserID", (-1).ToString(), option);
			return RedirectToAction(nameof(Index));
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
