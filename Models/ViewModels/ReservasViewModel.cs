using System.ComponentModel.DataAnnotations;

namespace Proyecto_Final_Estructura_De_Datos.Models.ViewModels
{
    public class ReservasViewModel
    {
        public int IdReserva { get; set; }
        
        [Required]
        [Display(Name="Usuario")]
        public int IdUsuario { get; set; }

        [Required]
        [Display(Name = "Habitación")]
        public int IdHabitacion { get; set; }

        [Required]
        [Display(Name = "Fecha de Inicio")]
        public DateOnly FechaInicio { get; set; }

        [Required]
        [Display(Name = "Fecha de Final")]
        public DateOnly FechaFinal { get; set; }

        [Display(Name = "Precio Total")]
        public decimal PrecioTotal { get; set; }

        [Display(Name = "Servicios a reservar")]
        public List<Servicio> ServiciosReservados { get; set; }
    }
}
