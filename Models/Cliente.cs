using System;
using System.Collections.Generic;

namespace MultiserviciosPiscinas.Models;

public partial class Cliente
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public string? Notas { get; set; }

    public virtual ICollection<Carrito> Carrito { get; set; } = new List<Carrito>();

    public virtual ICollection<Cotizacion> Cotizacion { get; set; } = new List<Cotizacion>();

    public virtual ICollection<DireccionCliente> DireccionCliente { get; set; } = new List<DireccionCliente>();

    public virtual ICollection<Factura> Factura { get; set; } = new List<Factura>();

    public virtual ICollection<Piscina> Piscina { get; set; } = new List<Piscina>();

    public virtual ICollection<ProyectoConstruccion> ProyectoConstruccion { get; set; } = new List<ProyectoConstruccion>();

    public virtual ICollection<TelefonosCliente> TelefonosCliente { get; set; } = new List<TelefonosCliente>();

    public virtual Usuario Usuario { get; set; } = null!;
}
