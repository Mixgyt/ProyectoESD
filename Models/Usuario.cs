using System;
using System.Collections.Generic;

namespace Proyecto_Final_Estructura_De_Datos.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public string Nombre { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Clave { get; set; } = null!;

    public int? Rol { get; set; }

    public virtual ICollection<Reserva> TblReservas { get; set; } = new List<Reserva>();
}
