using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Proyecto_Final_Estructura_De_Datos.Models;

public partial class DbHotelContext : DbContext
{
    public DbHotelContext()
    {
    }

    public DbHotelContext(DbContextOptions<DbHotelContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Habitacion> Habitaciones { get; set; }

    public virtual DbSet<Reserva> Reservas { get; set; }

    public virtual DbSet<ReservasServicio> ReservasServicios { get; set; }

    public virtual DbSet<Servicio> Servicios { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Habitacion>(entity =>
        {
            entity.HasKey(e => e.IdHabitacion).HasName("PK__tblHabit__D9D53BE2D17BB0B4");

            entity.ToTable("tblHabitaciones");

            entity.Property(e => e.IdHabitacion).HasColumnName("idHabitacion");
            entity.Property(e => e.CantidadPersona).HasColumnName("cantidadPersona");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.PrecioNoche)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("precioNoche");
            entity.Property(e => e.Tipo).HasColumnName("tipo");
        });

        modelBuilder.Entity<Reserva>(entity =>
        {
            entity.HasKey(e => e.IdReserva).HasName("PK__tblReser__94D104C8200BCF33");

            entity.ToTable("tblReservas");

            entity.Property(e => e.IdReserva).HasColumnName("idReserva");
            entity.Property(e => e.FechaFinal).HasColumnName("fechaFinal");
            entity.Property(e => e.FechaInicio).HasColumnName("fechaInicio");
            entity.Property(e => e.IdHabitacion).HasColumnName("idHabitacion");
            entity.Property(e => e.IdUsuario).HasColumnName("idUsuario");
            entity.Property(e => e.PrecioTotal)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("precioTotal");

            entity.HasOne(d => d.IdHabitacionNavigation).WithMany(p => p.Reservas)
                .HasForeignKey(d => d.IdHabitacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tblReserv__idHab__403A8C7D");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Reservas)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tblReserv__idUsu__412EB0B6");
        });

        modelBuilder.Entity<ReservasServicio>(entity =>
        {
            entity.HasKey(e => e.IdReservaServicio).HasName("PK__tblReser__E14C1BD0D91835B1");

            entity.ToTable("tblReservas_Servicios");

            entity.Property(e => e.IdReservaServicio).HasColumnName("idReserva_Servicio");
            entity.Property(e => e.IdReserva).HasColumnName("idReserva");
            entity.Property(e => e.IdServicio).HasColumnName("idServicio");

            entity.HasOne(d => d.IdReservaNavigation).WithMany(p => p.TblReservasServicios)
                .HasForeignKey(d => d.IdReserva)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tblReserv__idRes__4222D4EF");

            entity.HasOne(d => d.IdServicioNavigation).WithMany(p => p.ReservasServicios)
                .HasForeignKey(d => d.IdServicio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__tblReserv__idSer__4316F928");
        });

        modelBuilder.Entity<Servicio>(entity =>
        {
            entity.HasKey(e => e.IdServicio).HasName("PK__tblServi__CEB98119FBA34603");

            entity.ToTable("tblServicios");

            entity.Property(e => e.IdServicio).HasColumnName("idServicio");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.NombreServicio)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombreServicio");
            entity.Property(e => e.Precio)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("precio");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__tblUsuar__645723A6E4815CB0");

            entity.ToTable("tblUsuarios");

            entity.HasIndex(e => e.Email, "UQ__tblUsuar__AB6E61645BA78142").IsUnique();

            entity.Property(e => e.IdUsuario).HasColumnName("idUsuario");
            entity.Property(e => e.Clave)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("clave");
            entity.Property(e => e.Email)
                .HasMaxLength(40)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Nombre)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Rol).HasColumnName("rol");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
