using System;
using System.Collections.Generic;

namespace Proyecto_Final_Estructura_De_Datos.Models;

public partial class ReservasServicio
{
    public int IdReservaServicio { get; set; }

    public int IdReserva { get; set; }

    public int IdServicio { get; set; }

    public virtual Reserva IdReservaNavigation { get; set; } = null!;

    public virtual Servicio IdServicioNavigation { get; set; } = null!;
}
