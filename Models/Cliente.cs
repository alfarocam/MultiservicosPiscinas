using System;
using System.Collections.Generic;

namespace MultiserviciosPiscinas.Models;

public partial class Cliente
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public string? Notas { get; set; }

    public virtual ICollection<Carrito> Carritos { get; set; } = new List<Carrito>();

    public virtual ICollection<Cotizacion> Cotizacions { get; set; } = new List<Cotizacion>();

    public virtual ICollection<DireccionCliente> DireccionClientes { get; set; } = new List<DireccionCliente>();

    public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();

    public virtual ICollection<Piscina> Piscinas { get; set; } = new List<Piscina>();

    public virtual ICollection<ProyectoConstruccion> ProyectoConstruccions { get; set; } = new List<ProyectoConstruccion>();

    public virtual ICollection<TelefonosCliente> TelefonosClientes { get; set; } = new List<TelefonosCliente>();

    public virtual Usuario Usuario { get; set; } = null!;
}
