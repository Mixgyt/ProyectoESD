using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Proyecto_Final_Estructura_De_Datos.Models;
using Proyecto_Final_Estructura_De_Datos.Models.ViewModels;

namespace Proyecto_Final_Estructura_De_Datos.Controllers.AdminControllers
{
    public class ReservasController : Controller
    {
        private readonly DbHotelContext _context;

        public ReservasController(DbHotelContext context)
        {
            _context = context;
        }

        // GET: Reservas
        public async Task<IActionResult> Index()
        {
            var dbHotelContext = _context.Reservas.Include(r => r.IdHabitacionNavigation).Include(r => r.IdUsuarioNavigation);
            var reservas = await dbHotelContext.ToListAsync();
            
            List<Usuario> usuarios = new List<Usuario>();
            List<Habitacion> habitaciones = new List<Habitacion>();
            foreach (var item in dbHotelContext.ToList())
            {
                int idHabitacion = item.IdHabitacion;
                int idUsuario = item.IdUsuario;
                Habitacion habitacion = _context.Habitaciones.Where(x => x.IdHabitacion == idHabitacion).FirstOrDefault();
                habitaciones.Add(habitacion);
                Usuario usuario = _context.Usuarios.Where(u => u.IdUsuario == idUsuario).FirstOrDefault();
                usuarios.Add(usuario);
            }
            
            ViewData["Habitaciones"] = habitaciones;
            ViewData["Usuarios"] = usuarios;
            return View(reservas);
        }

        // GET: Reservas/Details/5
        public async Task<IActionResult> Details(int? id)
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

        // GET: Reservas/Create
        public IActionResult Create()
        {
            ViewData["Habitaciones"] = new List<Habitacion>(_context.Habitaciones);
            ViewData["Usuarios"] = new SelectList(_context.Usuarios, "IdUsuario", "Nombre");
            
            ViewData["Servicios"] = _context.Servicios.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Guardar(ReservasViewModel reserva, List<int> serviciosid)
        {
            List<Servicio> servicios = new List<Servicio>();
            foreach (var id in serviciosid)
            {
                Servicio servicio = _context.Servicios.Find(id);
                servicios.Add(servicio);
            }

            TimeSpan tiempo = reserva.FechaFinal.ToDateTime(TimeOnly.Parse("00:00")) - reserva.FechaInicio.ToDateTime(TimeOnly.Parse("00:00"));
            int noches = (int)tiempo.TotalDays;
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

        // POST: Reservas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm]ReservasViewModel reserva)
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
                    return View(reserva);
                }
            }  
            return RedirectToAction(nameof(Index));
        }

        // GET: Reservas/Edit/5
        public async Task<IActionResult> Edit(int? id)
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
            ViewData["Usuarios"] = new List<Usuario>(_context.Usuarios);
            ViewData["Servicios"] = new List<Servicio>(_context.Servicios);
            return View(reservasViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Actualizar(ReservasViewModel reserva, List<int> serviciosid)
        {
            List<Servicio> servicios = new List<Servicio>();
            foreach (var item in serviciosid)
            {
                Servicio servicio = _context.Servicios.Find(item);
                servicios.Add(servicio);
            }
            
            TimeSpan tiempo = reserva.FechaFinal.ToDateTime(TimeOnly.Parse("00:00")) - reserva.FechaInicio.ToDateTime(TimeOnly.Parse("00:00"));
            int noches = (int)tiempo.TotalDays;
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

        // POST: Reservas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm]ReservasViewModel reserva)
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
                return RedirectToAction(nameof(Index));
            }
            ViewData["Habitaciones"] = new List<Habitacion>(_context.Habitaciones);
            ViewData["Usuarios"] = new List<Usuario>(_context.Usuarios);
            ViewData["Servicios"] = new List<Servicio>(_context.Servicios);
            return View(reserva);
        }

        // GET: Reservas/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

        // POST: Reservas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
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
            return RedirectToAction(nameof(Index));
        }

        private bool ReservaExists(int id)
        {
            return _context.Reservas.Any(e => e.IdReserva == id);
        }
    }
}
