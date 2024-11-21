using System;
using System.Collections.Generic;

namespace Proyecto_Final_Estructura_De_Datos.Models;

public partial class Reserva
{
    public int IdReserva { get; set; }

    public int IdUsuario { get; set; }

    public int IdHabitacion { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly FechaFinal { get; set; }

    public decimal PrecioTotal { get; set; }

    public virtual Habitacion IdHabitacionNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

    public virtual ICollection<ReservasServicio> TblReservasServicios { get; set; } = new List<ReservasServicio>();
}
