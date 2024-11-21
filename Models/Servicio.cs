using System;
using System.Collections.Generic;

namespace Proyecto_Final_Estructura_De_Datos.Models;

public partial class Servicio
{
    public int IdServicio { get; set; }

    public string Descripcion { get; set; } = null!;

    public string NombreServicio { get; set; } = null!;

    public decimal Precio { get; set; }

    public virtual ICollection<ReservasServicio> ReservasServicios { get; set; } = new List<ReservasServicio>();
}
