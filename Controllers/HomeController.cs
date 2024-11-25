using Microsoft.AspNetCore.Mvc;
using Proyecto_Final_Estructura_De_Datos.Models;
using System.Diagnostics;
using System.Dynamic;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Proyecto_Final_Estructura_De_Datos.Models.ViewModels;

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
				ViewData["Usuario"] = usuario;
			}
			
			return View();
		}

		public IActionResult Servicios()
		{
			ViewData["Servicios"] = _context.Servicios.ToList();
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
			Usuario usuarioRetornado =
				_context.Usuarios.FirstOrDefault(u => u.Nombre == usuario.Nombre && u.Clave == usuario.Clave);
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

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult ReservarHabitacion(DateOnly check_in, DateOnly check_out)
		{
			List<Reserva> reservas = _context.Reservas.Where(x => x.FechaInicio < check_out && x.FechaFinal > check_in)
				.ToList();
			var habitaciones = _context.Habitaciones.ToList();
			;
			foreach (Reserva reserva in reservas)
			{
				Habitacion habitacion = habitaciones.Find(x => x.IdHabitacion == reserva.IdHabitacion);
				habitaciones.Remove(habitacion);
			}

			ReservasViewModel reservasViewModel = new ReservasViewModel()
			{
				FechaInicio = check_in,
				FechaFinal = check_out,
			};

			ViewData["Habitaciones"] = habitaciones;
			ViewData["Servicios"] = _context.Servicios.ToList();
			return View(reservasViewModel);
		}

		public IActionResult ReservarFecha()
		{
			return View();
		}

		public IActionResult Reservar(ReservasViewModel reservasViewModel, List<int> serviciosid)
		{
			Habitacion habitacion = _context.Habitaciones.Find(reservasViewModel.IdHabitacion);
			if (habitacion == null)
			{
				return RedirectToAction(nameof(Index));
			}
			List<Servicio> servicios = new List<Servicio>();
			TimeSpan tiempo = reservasViewModel.FechaFinal.ToDateTime(TimeOnly.Parse("00:00")) -
			                  reservasViewModel.FechaInicio.ToDateTime(TimeOnly.Parse("00:00"));
			int noches = (int)tiempo.TotalDays;
			if (noches <= 0)
			{
				noches = 1;
			}

			decimal precioTotal = habitacion.PrecioNoche * noches;

			if (serviciosid != null || serviciosid.Count > 0)
			{
				foreach (var ids in serviciosid)
				{
					Servicio servicio = _context.Servicios.Find(ids);
					servicios.Add(servicio);
				}
			}

			reservasViewModel.ServiciosReservados = servicios;

			foreach (var servicio in reservasViewModel.ServiciosReservados)
			{
				precioTotal += servicio.Precio;
			}

			var cookie = ControllerContext.HttpContext.Request.Cookies["UserID"];
			Usuario usuario = new Usuario();
			if (cookie != null)
			{
				usuario = _context.Usuarios.FirstOrDefault(u => u.IdUsuario == int.Parse(cookie));
			}

			reservasViewModel.IdUsuario = usuario.IdUsuario;
			reservasViewModel.PrecioTotal = precioTotal;
			ViewData["Habitacion"] = habitacion;
			ViewData["Noches"] = noches;
			return View(reservasViewModel);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> GuardarReservar(ReservasViewModel reserva)
		{
			try
			{
				Reserva reservaLista = new Reserva()
				{
					FechaInicio = reserva.FechaInicio,
					FechaFinal = reserva.FechaFinal,
					IdHabitacion = reserva.IdHabitacion,
					IdUsuario = reserva.IdUsuario,
					PrecioTotal = reserva.PrecioTotal
				};
				_context.Reservas.Add(reservaLista);
				await _context.SaveChangesAsync();
				Reserva reservaDB = _context.Reservas.ToList().Find(x =>
					x.IdHabitacion == reserva.IdHabitacion && x.FechaFinal == reserva.FechaFinal &&
					x.FechaInicio == reserva.FechaInicio);
				foreach (var servicio in reserva.ServiciosReservados)
				{
					ReservasServicio pivote = new ReservasServicio()
					{
						IdReserva = reservaDB.IdReserva,
						IdServicio = servicio.IdServicio,
					};
					_context.ReservasServicios.Add(pivote);
				}

				await _context.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				if (reserva == null)
				{
					ViewData["Habitaciones"] = new List<Habitacion>(_context.Habitaciones);
					ViewData["Usuarios"] = new SelectList(_context.Usuarios, "IdUsuario", "Nombre");
					ViewData["Servicios"] = _context.Servicios.ToList();
					return RedirectToAction(nameof(Index));
				}
			}

			return RedirectToAction(nameof(Index));
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
