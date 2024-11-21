using System;
using System.Collections.Generic;

namespace Proyecto_Final_Estructura_De_Datos.Models;

public partial class Habitacion
{
    public int IdHabitacion { get; set; }

    public TipoHabitacion Tipo { get; set; }

    public decimal PrecioNoche { get; set; }

    public string Descripcion { get; set; } = null!;

    public int CantidadPersona { get; set; }

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
