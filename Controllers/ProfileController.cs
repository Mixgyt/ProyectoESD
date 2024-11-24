using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Proyecto_Final_Estructura_De_Datos.Models;
using Proyecto_Final_Estructura_De_Datos.Models.ViewModels;

namespace Proyecto_Final_Estructura_De_Datos.Controllers;

public class ProfileController : Controller
{
    private readonly DbHotelContext _context;

    public ProfileController(DbHotelContext context)
    {
        _context = context;
    }

    public IActionResult Perfil()
    {
        var cookie = ControllerContext.HttpContext.Request.Cookies["UserID"];
        int userId;
        Usuario usuario = new Usuario();
        if (cookie != null)
        {
            userId = int.Parse(cookie);
            usuario = _context.Usuarios.FirstOrDefault(u => u.IdUsuario == userId);
        }
        
        return View(usuario);
    }
    
    public async Task<IActionResult> EditUser(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null)
        {
            return NotFound();
        }
        return View(usuario);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(int id, [Bind("IdUsuario,Nombre,Email,Clave,Rol")] Usuario usuario)
    {
        if (id != usuario.IdUsuario)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(usuario);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsuarioExists(usuario.IdUsuario))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Perfil));
        }
        return View(usuario);
    }

    public IActionResult Reservas()
    {
        var cookie = ControllerContext.HttpContext.Request.Cookies["UserID"];
        if (cookie == null)
        {
            return RedirectToAction(nameof(Perfil));
        }
        
        int userId = int.Parse(cookie);
        Usuario usuario = _context.Usuarios.FirstOrDefault(u => u.IdUsuario == userId);
        
        var dbHotelContext = _context.Reservas.Where(x=>x.IdUsuario == usuario.IdUsuario);
        var reservas =  dbHotelContext.ToList();
        
        List<Habitacion> habitaciones = new List<Habitacion>();
        foreach (var item in reservas)
        {
            int idHabitacion = item.IdHabitacion;
            Habitacion habitacion = _context.Habitaciones.Where(x => x.IdHabitacion == idHabitacion).FirstOrDefault();
            habitaciones.Add(habitacion);
        }
            
        ViewData["Habitaciones"] = habitaciones;
        return View(reservas);
    }

    public async Task<IActionResult> EditReserva(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var reserva = await _context.Reservas.FindAsync(id);
        if (reserva == null)
        {
            return NotFound();
        }
        var reservasServicios = _context.ReservasServicios.Where(x=>x.IdReserva==id).ToList();
        List<Servicio> serviciosSelec = new List<Servicio>();
        foreach (var item in reservasServicios)
        {
            Servicio servicio = _context.Servicios.Find(item.IdServicio);
            serviciosSelec.Add(servicio);
        }

        ReservasViewModel reservasViewModel = new ReservasViewModel()
        {
            IdReserva = reserva.IdReserva,
            FechaInicio = reserva.FechaInicio,
            FechaFinal = reserva.FechaFinal,
            IdHabitacion = reserva.IdHabitacion,
            IdUsuario = reserva.IdUsuario,
            PrecioTotal = reserva.PrecioTotal,
            ServiciosReservados= serviciosSelec
        };
            
        ViewData["Habitaciones"] = new List<Habitacion>(_context.Habitaciones);
        ViewData["Usuario"] = _context.Usuarios.Find(reserva.IdUsuario);
        ViewData["Servicios"] = new List<Servicio>(_context.Servicios);
        return View(reservasViewModel);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ActualizarReserva(ReservasViewModel reserva, List<int> serviciosid)
    {
        List<Servicio> servicios = new List<Servicio>();
        foreach (var item in serviciosid)
        {
            Servicio servicio = _context.Servicios.Find(item);
            servicios.Add(servicio);
        }
            
        int noches = reserva.FechaInicio.CompareTo(reserva.FechaFinal);
        if (noches <= 0)
        {
            noches = 1;
        }
            
        Habitacion habitacion = _context.Habitaciones.Find(reserva.IdHabitacion);
        Usuario usuario = _context.Usuarios.Find(reserva.IdUsuario);

        reserva.PrecioTotal = (habitacion.PrecioNoche * noches) + servicios.Sum(s => s.Precio);
        reserva.ServiciosReservados= servicios;
        ViewData["Noches"] = noches;
        ViewData["Habitacion"] = habitacion;
        ViewData["Usuario"] = usuario;
            
        return View(reserva);
    }
    
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditReserva([FromForm]ReservasViewModel reserva)
        {
            if (reserva != null)
            {
                try
                {
                    Reserva reservaLista = _context.Reservas.Find(reserva.IdReserva);
                    reservaLista.FechaInicio = reserva.FechaInicio;
                    reservaLista.FechaFinal = reserva.FechaFinal;
                    reservaLista.IdHabitacion = reserva.IdHabitacion;
                    reservaLista.IdUsuario = reserva.IdUsuario;
                    reservaLista.PrecioTotal = reserva.PrecioTotal;
                    
                    _context.Reservas.Update(reservaLista);
                    await _context.SaveChangesAsync();
                    
                    List<ReservasServicio> serviciosOld = _context.ReservasServicios.ToList().FindAll(x=>x.IdReserva==reserva.IdReserva);
                    foreach (var servicio in serviciosOld)
                    {
                        _context.Remove(servicio);
                       await _context.SaveChangesAsync();
                    }

                    if (reserva.ServiciosReservados != null)
                    {
                        foreach (var servicio in reserva.ServiciosReservados)
                        {
                            ReservasServicio pivote = new ReservasServicio()
                            {
                                IdReserva = reserva.IdReserva,
                                IdServicio = servicio.IdServicio,
                            };
                            _context.ReservasServicios.Add(pivote);
                        }
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservaExists(reserva.IdReserva))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Reservas));
            }
            ViewData["Habitaciones"] = new List<Habitacion>(_context.Habitaciones);
            ViewData["Usuarios"] = new List<Usuario>(_context.Usuarios);
            ViewData["Servicios"] = new List<Servicio>(_context.Servicios);
            return View(reserva);
        }
        
    
        public async Task<IActionResult> DetallesReserva(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas
                .Include(r => r.IdHabitacionNavigation)
                .Include(r => r.IdUsuarioNavigation)
                .FirstOrDefaultAsync(m => m.IdReserva == id);
            if (reserva == null)
            {
                return NotFound();
            }
            Habitacion habitacion = _context.Habitaciones.Find(reserva.IdHabitacion);
            Usuario usuario = _context.Usuarios.Find(reserva.IdUsuario);
            List<ReservasServicio> serviciosRe = _context.ReservasServicios.Where(x=>x.IdReserva==reserva.IdReserva).ToList();
            List<Servicio> servicios = new List<Servicio>();
            foreach (var servicio in serviciosRe)
            {
                Servicio servicioRe = _context.Servicios.Find(servicio.IdServicio);
                servicios.Add(servicioRe);
            }
            
            ViewData["Habitacion"] = habitacion;
            ViewData["Usuario"] = usuario;
            ViewData["Servicios"] = servicios;
            return View(reserva);
        }
        
        public async Task<IActionResult> Cancelar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas
                .Include(r => r.IdHabitacionNavigation)
                .Include(r => r.IdUsuarioNavigation)
                .FirstOrDefaultAsync(m => m.IdReserva == id);
            if (reserva == null)
            {
                return NotFound();
            }
            Habitacion habitacion = _context.Habitaciones.Find(reserva.IdHabitacion);
            Usuario usuario = _context.Usuarios.Find(reserva.IdUsuario);
            List<ReservasServicio> serviciosRe = _context.ReservasServicios.Where(x=>x.IdReserva==reserva.IdReserva).ToList();
            List<Servicio> servicios = new List<Servicio>();
            foreach (var servicio in serviciosRe)
            {
                Servicio servicioRe = _context.Servicios.Find(servicio.IdServicio);
                servicios.Add(servicioRe);
            }
            
            ViewData["Habitacion"] = habitacion;
            ViewData["Usuario"] = usuario;
            ViewData["Servicios"] = servicios;
            return View(reserva);
        }
        
        [HttpPost, ActionName("Cancelar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarConfirmed(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            var serviciosReservados = _context.ReservasServicios.Where(x => x.IdReserva == id).ToList();

            foreach (var servicio in serviciosReservados)
            {
                _context.ReservasServicios.Remove(servicio);
            }
            
            if (reserva != null)
            {
                _context.Reservas.Remove(reserva);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Reservas));
        }
        
    private bool ReservaExists(int id)
    {
        return _context.Reservas.Any(e => e.IdReserva == id);
    }
    
    private bool UsuarioExists(int id)
    {
        return _context.Usuarios.Any(e => e.IdUsuario == id);
    }
}