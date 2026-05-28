using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MultiservicosPiscinas.Models;

public partial class PiscinasYMultiserviciosContext : DbContext
{
    public PiscinasYMultiserviciosContext()
    {
    }

    public PiscinasYMultiserviciosContext(DbContextOptions<PiscinasYMultiserviciosContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Anuncio> Anuncios { get; set; }

    public virtual DbSet<BitacoraAuditorium> BitacoraAuditoria { get; set; }

    public virtual DbSet<Canton> Cantons { get; set; }

    public virtual DbSet<Carrito> Carritos { get; set; }

    public virtual DbSet<CategoriaGastoOperativo> CategoriaGastoOperativos { get; set; }

    public virtual DbSet<CategoriaProducto> CategoriaProductos { get; set; }

    public virtual DbSet<Citum> Cita { get; set; }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<Cotizacion> Cotizacions { get; set; }

    public virtual DbSet<DetalleCotizacion> DetalleCotizacions { get; set; }

    public virtual DbSet<DetalleFactura> DetalleFacturas { get; set; }

    public virtual DbSet<DireccionCliente> DireccionClientes { get; set; }

    public virtual DbSet<Distrito> Distritos { get; set; }

    public virtual DbSet<Encuestum> Encuesta { get; set; }

    public virtual DbSet<Equipamiento> Equipamientos { get; set; }

    public virtual DbSet<Factura> Facturas { get; set; }

    public virtual DbSet<GastoOperativo> GastoOperativos { get; set; }

    public virtual DbSet<Inspeccion> Inspeccions { get; set; }

    public virtual DbSet<ItemCarrito> ItemCarritos { get; set; }

    public virtual DbSet<Piscina> Piscinas { get; set; }

    public virtual DbSet<PiscinaEquipamiento> PiscinaEquipamientos { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<Provincium> Provincia { get; set; }

    public virtual DbSet<ProyectoConstruccion> ProyectoConstruccions { get; set; }

    public virtual DbSet<Rol> Rols { get; set; }

    public virtual DbSet<RutaOptimizadum> RutaOptimizada { get; set; }

    public virtual DbSet<Servicio> Servicios { get; set; }

    public virtual DbSet<TareaServicio> TareaServicios { get; set; }

    public virtual DbSet<TelefonosCliente> TelefonosClientes { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Vehiculo> Vehiculos { get; set; }

    public virtual DbSet<VisitaRutum> VisitaRuta { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=Piscinas_Y_Multiservicios;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Anuncio>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ANUNCIO__3213E83FBE07E040");

            entity.ToTable("ANUNCIO", "crm");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AutorId).HasColumnName("autor_id");
            entity.Property(e => e.Contenido)
                .HasColumnType("text")
                .HasColumnName("contenido");
            entity.Property(e => e.FechaCaducidad).HasColumnName("fecha_caducidad");
            entity.Property(e => e.FechaPublicacion).HasColumnName("fecha_publicacion");
            entity.Property(e => e.Prioridad)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("prioridad");
            entity.Property(e => e.Titulo)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("titulo");

            entity.HasOne(d => d.Autor).WithMany(p => p.Anuncios)
                .HasForeignKey(d => d.AutorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ANUNCIO_AUTOR");
        });

        modelBuilder.Entity<BitacoraAuditorium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__BITACORA__3213E83FD494B8F9");

            entity.ToTable("BITACORA_AUDITORIA", "aud");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Accion)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("accion");
            entity.Property(e => e.FechaHora)
                .HasColumnType("datetime")
                .HasColumnName("fecha_hora");
            entity.Property(e => e.RegistroId).HasColumnName("registro_id");
            entity.Property(e => e.TablaAfectada)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("tabla_afectada");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");
            entity.Property(e => e.ValorAnterior)
                .HasColumnType("text")
                .HasColumnName("valor_anterior");
            entity.Property(e => e.ValorNuevo)
                .HasColumnType("text")
                .HasColumnName("valor_nuevo");

            entity.HasOne(d => d.Usuario).WithMany(p => p.BitacoraAuditoria)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BITACORA_USUARIO");
        });

        modelBuilder.Entity<Canton>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CANTON__3213E83FDD32ABCB");

            entity.ToTable("CANTON", "geo");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.ProvinciaId).HasColumnName("provincia_id");

            entity.HasOne(d => d.Provincia).WithMany(p => p.Cantons)
                .HasForeignKey(d => d.ProvinciaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CANTON_PROVINCIA");
        });

        modelBuilder.Entity<Carrito>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CARRITO__3213E83F5EE198E8");

            entity.ToTable("CARRITO", "ven");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.CreadoEn)
                .HasColumnType("datetime")
                .HasColumnName("creado_en");

            entity.HasOne(d => d.Cliente).WithMany(p => p.Carritos)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CARRITO_CLIENTE");
        });

        modelBuilder.Entity<CategoriaGastoOperativo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CATEGORI__3213E83FF9237933");

            entity.ToTable("CATEGORIA_GASTO_OPERATIVO", "fin");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.NombreCategoria)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre_categoria");
        });

        modelBuilder.Entity<CategoriaProducto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CATEGORI__3213E83FECBD5E09");

            entity.ToTable("CATEGORIA_PRODUCTO", "inv");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.NombreCategoria)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre_categoria");
        });

        modelBuilder.Entity<Citum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CITA__3213E83F7BE5BB6B");

            entity.ToTable("CITA", "ops");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("estado");
            entity.Property(e => e.FechaHora)
                .HasColumnType("datetime")
                .HasColumnName("fecha_hora");
            entity.Property(e => e.Notas)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("notas");
            entity.Property(e => e.PiscinaId).HasColumnName("piscina_id");
            entity.Property(e => e.TecnicoId).HasColumnName("tecnico_id");
            entity.Property(e => e.Tipo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("tipo");

            entity.HasOne(d => d.Piscina).WithMany(p => p.Cita)
                .HasForeignKey(d => d.PiscinaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CITA_PISCINA");

            entity.HasOne(d => d.Tecnico).WithMany(p => p.Cita)
                .HasForeignKey(d => d.TecnicoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CITA_TECNICO");
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CLIENTE__3213E83F640B272D");

            entity.ToTable("CLIENTE", "cli");

            entity.HasIndex(e => e.UsuarioId, "UQ_CLIENTE_USUARIO").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Notas)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("notas");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.Usuario).WithOne(p => p.Cliente)
                .HasForeignKey<Cliente>(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CLIENTE_USUARIO");
        });

        modelBuilder.Entity<Cotizacion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__COTIZACI__3213E83F4F02A015");

            entity.ToTable("COTIZACION", "ven");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.DescuentoTotal)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("descuento_total");
            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("estado");
            entity.Property(e => e.FechaEmision).HasColumnName("fecha_emision");
            entity.Property(e => e.FechaVigencia).HasColumnName("fecha_vigencia");
            entity.Property(e => e.ImpuestoTotal)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("impuesto_total");
            entity.Property(e => e.Subtotal)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("subtotal");
            entity.Property(e => e.Total)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("total");

            entity.HasOne(d => d.Cliente).WithMany(p => p.Cotizacions)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_COTIZACION_CLIENTE");
        });

        modelBuilder.Entity<DetalleCotizacion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DETALLE___3213E83FCDA15464");

            entity.ToTable("DETALLE_COTIZACION", "ven");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CantidadPropuesta)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("cantidad_propuesta");
            entity.Property(e => e.CotizacionId).HasColumnName("cotizacion_id");
            entity.Property(e => e.Descuento)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("descuento");
            entity.Property(e => e.Impuesto)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("impuesto");
            entity.Property(e => e.LineaSubtotal)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("linea_subtotal");
            entity.Property(e => e.LineaTotal)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("linea_total");
            entity.Property(e => e.PrecioUnitario)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("precio_unitario");
            entity.Property(e => e.ProductoId).HasColumnName("producto_id");

            entity.HasOne(d => d.Cotizacion).WithMany(p => p.DetalleCotizacions)
                .HasForeignKey(d => d.CotizacionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DETALLE_COT_COTIZACION");

            entity.HasOne(d => d.Producto).WithMany(p => p.DetalleCotizacions)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DETALLE_COT_PRODUCTO");
        });

        modelBuilder.Entity<DetalleFactura>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DETALLE___3213E83FFCF81901");

            entity.ToTable("DETALLE_FACTURA", "ven");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CantidadVendida)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("cantidad_vendida");
            entity.Property(e => e.Descuento)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("descuento");
            entity.Property(e => e.FacturaId).HasColumnName("factura_id");
            entity.Property(e => e.Impuesto)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("impuesto");
            entity.Property(e => e.LineaSubtotal)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("linea_subtotal");
            entity.Property(e => e.LineaTotal)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("linea_total");
            entity.Property(e => e.PrecioUnitarioFinal)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("precio_unitario_final");
            entity.Property(e => e.ProductoId).HasColumnName("producto_id");

            entity.HasOne(d => d.Factura).WithMany(p => p.DetalleFacturas)
                .HasForeignKey(d => d.FacturaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DETALLE_FAC_FACTURA");

            entity.HasOne(d => d.Producto).WithMany(p => p.DetalleFacturas)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DETALLE_FAC_PRODUCTO");
        });

        modelBuilder.Entity<DireccionCliente>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DIRECCIO__3213E83F2969199A");

            entity.ToTable("DIRECCION_CLIENTE", "cli");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.CodigoPostal).HasColumnName("codigo_postal");
            entity.Property(e => e.Detalles)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("detalles");
            entity.Property(e => e.DistritoId).HasColumnName("distrito_id");
            entity.Property(e => e.EsPrincipal).HasColumnName("es_principal");
            entity.Property(e => e.TipoDireccion)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("tipo_direccion");

            entity.HasOne(d => d.Cliente).WithMany(p => p.DireccionClientes)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DIRECCION_CLIENTE_CLIENTE");

            entity.HasOne(d => d.Distrito).WithMany(p => p.DireccionClientes)
                .HasForeignKey(d => d.DistritoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DIRECCION_CLIENTE_DISTRITO");
        });

        modelBuilder.Entity<Distrito>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DISTRITO__3213E83F29F5819C");

            entity.ToTable("DISTRITO", "geo");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CantonId).HasColumnName("canton_id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");

            entity.HasOne(d => d.Canton).WithMany(p => p.Distritos)
                .HasForeignKey(d => d.CantonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DISTRITO_CANTON");
        });

        modelBuilder.Entity<Encuestum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ENCUESTA__3213E83F919BD247");

            entity.ToTable("ENCUESTA", "crm");

            entity.HasIndex(e => e.ServicioId, "UQ_ENCUESTA_SERVICIO").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Calificacion).HasColumnName("calificacion");
            entity.Property(e => e.Comentario)
                .HasColumnType("text")
                .HasColumnName("comentario");
            entity.Property(e => e.FechaEnvio)
                .HasColumnType("datetime")
                .HasColumnName("fecha_envio");
            entity.Property(e => e.ServicioId).HasColumnName("servicio_id");

            entity.HasOne(d => d.Servicio).WithOne(p => p.Encuestum)
                .HasForeignKey<Encuestum>(d => d.ServicioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ENCUESTA_SERVICIO");
        });

        modelBuilder.Entity<Equipamiento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__EQUIPAMI__3213E83FDFCF68FA");

            entity.ToTable("EQUIPAMIENTO", "act");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Factura>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__FACTURA__3213E83F610193A4");

            entity.ToTable("FACTURA", "ven");

            entity.HasIndex(e => e.NumeroConsecutivo, "UQ_FACTURA_CONSECUTIVO").IsUnique();

            entity.HasIndex(e => e.CotizacionId, "UQ_FACTURA_COTIZACION").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.ComprobanteSinpeRuta)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("comprobante_sinpe_ruta");
            entity.Property(e => e.CondicionPago)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("condicion_pago");
            entity.Property(e => e.CotizacionId).HasColumnName("cotizacion_id");
            entity.Property(e => e.CreadoPor).HasColumnName("creado_por");
            entity.Property(e => e.DescuentoTotal)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("descuento_total");
            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("estado");
            entity.Property(e => e.FechaEmision).HasColumnName("fecha_emision");
            entity.Property(e => e.FechaVencimiento).HasColumnName("fecha_vencimiento");
            entity.Property(e => e.ImpuestoTotal)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("impuesto_total");
            entity.Property(e => e.NumeroConsecutivo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("numero_consecutivo");
            entity.Property(e => e.Subtotal)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("subtotal");
            entity.Property(e => e.Total)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("total");

            entity.HasOne(d => d.Cliente).WithMany(p => p.Facturas)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FACTURA_CLIENTE");

            entity.HasOne(d => d.Cotizacion).WithOne(p => p.Factura)
                .HasForeignKey<Factura>(d => d.CotizacionId)
                .HasConstraintName("FK_FACTURA_COTIZACION");
        });

        modelBuilder.Entity<GastoOperativo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GASTO_OP__3213E83F2091F899");

            entity.ToTable("GASTO_OPERATIVO", "fin");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CategoriaId).HasColumnName("categoria_id");
            entity.Property(e => e.CitaId).HasColumnName("cita_id");
            entity.Property(e => e.ComprobanteRuta)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("comprobante_ruta");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("estado");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.Monto)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("monto");
            entity.Property(e => e.MotivoRechazo)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("motivo_rechazo");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.Categoria).WithMany(p => p.GastoOperativos)
                .HasForeignKey(d => d.CategoriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GASTO_CATEGORIA");

            entity.HasOne(d => d.Cita).WithMany(p => p.GastoOperativos)
                .HasForeignKey(d => d.CitaId)
                .HasConstraintName("FK_GASTO_CITA");

            entity.HasOne(d => d.Usuario).WithMany(p => p.GastoOperativos)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GASTO_USUARIO");
        });

        modelBuilder.Entity<Inspeccion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__INSPECCI__3213E83FC74FC454");

            entity.ToTable("INSPECCION", "ops");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AcidoCianurico).HasColumnName("acido_cianurico");
            entity.Property(e => e.Alcalinidad).HasColumnName("alcalinidad");
            entity.Property(e => e.Calcio).HasColumnName("calcio");
            entity.Property(e => e.CloroPpm).HasColumnName("cloro_ppm");
            entity.Property(e => e.FechaInspeccion).HasColumnName("fecha_inspeccion");
            entity.Property(e => e.Observaciones)
                .HasColumnType("text")
                .HasColumnName("observaciones");
            entity.Property(e => e.Ph).HasColumnName("ph");
            entity.Property(e => e.ServicioId).HasColumnName("servicio_id");

            entity.HasOne(d => d.Servicio).WithMany(p => p.Inspeccions)
                .HasForeignKey(d => d.ServicioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_INSPECCION_SERVICIO");
        });

        modelBuilder.Entity<ItemCarrito>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ITEM_CAR__3213E83FCF5D1505");

            entity.ToTable("ITEM_CARRITO", "ven");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.CarritoId).HasColumnName("carrito_id");
            entity.Property(e => e.ProductoId).HasColumnName("producto_id");

            entity.HasOne(d => d.Carrito).WithMany(p => p.ItemCarritos)
                .HasForeignKey(d => d.CarritoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ITEM_CARRITO");

            entity.HasOne(d => d.Producto).WithMany(p => p.ItemCarritos)
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ITEM_PRODUCTO");
        });

        modelBuilder.Entity<Piscina>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PISCINA__3213E83FEB3F804C");

            entity.ToTable("PISCINA", "act");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.DireccionId).HasColumnName("direccion_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("estado");
            entity.Property(e => e.Tipo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("tipo");
            entity.Property(e => e.VolumenM3).HasColumnName("volumen_m3");

            entity.HasOne(d => d.Cliente).WithMany(p => p.Piscinas)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PISCINA_CLIENTE");

            entity.HasOne(d => d.Direccion).WithMany(p => p.Piscinas)
                .HasForeignKey(d => d.DireccionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PISCINA_DIRECCION");
        });

        modelBuilder.Entity<PiscinaEquipamiento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PISCINA___3213E83F7FD15167");

            entity.ToTable("PISCINA_EQUIPAMIENTO", "act");

            entity.HasIndex(e => new { e.PiscinaId, e.EquipamientoId }, "UQ_PISCINA_EQUIPAMIENTO").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EquipamientoId).HasColumnName("equipamiento_id");
            entity.Property(e => e.PiscinaId).HasColumnName("piscina_id");

            entity.HasOne(d => d.Equipamiento).WithMany(p => p.PiscinaEquipamientos)
                .HasForeignKey(d => d.EquipamientoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PE_EQUIPAMIENTO");

            entity.HasOne(d => d.Piscina).WithMany(p => p.PiscinaEquipamientos)
                .HasForeignKey(d => d.PiscinaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PE_PISCINA");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PRODUCTO__3213E83FED7556DA");

            entity.ToTable("PRODUCTO", "inv");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.CategoriaId).HasColumnName("categoria_id");
            entity.Property(e => e.Descripcion)
                .HasColumnType("text")
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Precio)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("precio");
            entity.Property(e => e.Stock).HasColumnName("stock");

            entity.HasOne(d => d.Categoria).WithMany(p => p.Productos)
                .HasForeignKey(d => d.CategoriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PRODUCTO_CATEGORIA");
        });

        modelBuilder.Entity<Provincium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PROVINCI__3213E83F0AE981CD");

            entity.ToTable("PROVINCIA", "geo");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<ProyectoConstruccion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PROYECTO__3213E83FF1B37E32");

            entity.ToTable("PROYECTO_CONSTRUCCION", "pry");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.Descripcion)
                .HasColumnType("text")
                .HasColumnName("descripcion");
            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("estado");
            entity.Property(e => e.FechaFinEstimada).HasColumnName("fecha_fin_estimada");
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.PiscinaId).HasColumnName("piscina_id");
            entity.Property(e => e.Presupuesto)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("presupuesto");

            entity.HasOne(d => d.Cliente).WithMany(p => p.ProyectoConstruccions)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PROYECTO_CLIENTE");

            entity.HasOne(d => d.Piscina).WithMany(p => p.ProyectoConstruccions)
                .HasForeignKey(d => d.PiscinaId)
                .HasConstraintName("FK_PROYECTO_PISCINA");
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ROL__3213E83FDA44A0DB");

            entity.ToTable("ROL", "seg");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<RutaOptimizadum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RUTA_OPT__3213E83FEFBA2F6B");

            entity.ToTable("RUTA_OPTIMIZADA", "log");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DistanciaTotalKm).HasColumnName("distancia_total_km");
            entity.Property(e => e.DuracionTotalMin).HasColumnName("duracion_total_min");
            entity.Property(e => e.EnlaceGoogleMaps)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("enlace_google_maps");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.GeneradaEn)
                .HasColumnType("datetime")
                .HasColumnName("generada_en");
            entity.Property(e => e.TecnicoId).HasColumnName("tecnico_id");

            entity.HasOne(d => d.Tecnico).WithMany(p => p.RutaOptimizada)
                .HasForeignKey(d => d.TecnicoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RUTA_TECNICO");
        });

        modelBuilder.Entity<Servicio>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SERVICIO__3213E83FDA07485A");

            entity.ToTable("SERVICIO", "ops");

            entity.HasIndex(e => e.CitaId, "UQ_SERVICIO_CITA").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CitaId).HasColumnName("cita_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("estado");
            entity.Property(e => e.FechaApertura).HasColumnName("fecha_apertura");
            entity.Property(e => e.FechaCierre).HasColumnName("fecha_cierre");
            entity.Property(e => e.TrabajoRealizado)
                .HasColumnType("text")
                .HasColumnName("trabajo_realizado");

            entity.HasOne(d => d.Cita).WithOne(p => p.Servicio)
                .HasForeignKey<Servicio>(d => d.CitaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SERVICIO_CITA");
        });

        modelBuilder.Entity<TareaServicio>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TAREA_SE__3213E83F9CCE3DAC");

            entity.ToTable("TAREA_SERVICIO", "ops");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("estado");
            entity.Property(e => e.FechaAsignacion).HasColumnName("fecha_asignacion");
            entity.Property(e => e.FechaCompletacion).HasColumnName("fecha_completacion");
            entity.Property(e => e.ServicioId).HasColumnName("servicio_id");

            entity.HasOne(d => d.Servicio).WithMany(p => p.TareaServicios)
                .HasForeignKey(d => d.ServicioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TAREA_SERVICIO");
        });

        modelBuilder.Entity<TelefonosCliente>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TELEFONO__3213E83F071450C8");

            entity.ToTable("TELEFONOS_CLIENTE", "cli");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.EsPrincipal).HasColumnName("es_principal");
            entity.Property(e => e.NumeroTelefono)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("numero_telefono");
            entity.Property(e => e.TipoTelefono)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("tipo_telefono");

            entity.HasOne(d => d.Cliente).WithMany(p => p.TelefonosClientes)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TELEFONOS_CLIENTE_CLIENTE");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__USUARIO__3213E83F34D090B6");

            entity.ToTable("USUARIO", "seg");

            entity.HasIndex(e => e.Correo, "UQ_USUARIO_CORREO").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Activo).HasColumnName("activo");
            entity.Property(e => e.ApellidoMaterno)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("apellido_materno");
            entity.Property(e => e.ApellidoPaterno)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("apellido_paterno");
            entity.Property(e => e.Contrasena)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("contrasena");
            entity.Property(e => e.Correo)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("correo");
            entity.Property(e => e.FechaCreacion)
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.RolId).HasColumnName("rol_id");

            entity.HasOne(d => d.Rol).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.RolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_USUARIO_ROL");
        });

        modelBuilder.Entity<Vehiculo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__VEHICULO__3213E83FDE195E87");

            entity.ToTable("VEHICULO", "log");

            entity.HasIndex(e => e.Placa, "UQ_VEHICULO_PLACA").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("estado");
            entity.Property(e => e.Marca)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("marca");
            entity.Property(e => e.Modelo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("modelo");
            entity.Property(e => e.Placa)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("placa");
            entity.Property(e => e.TecnicoId).HasColumnName("tecnico_id");

            entity.HasOne(d => d.Tecnico).WithMany(p => p.Vehiculos)
                .HasForeignKey(d => d.TecnicoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VEHICULO_TECNICO");
        });

        modelBuilder.Entity<VisitaRutum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__VISITA_R__3213E83FF1F8B4BF");

            entity.ToTable("VISITA_RUTA", "log");

            entity.HasIndex(e => e.CitaId, "UQ_VISITA_RUTA_CITA").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CitaId).HasColumnName("cita_id");
            entity.Property(e => e.DistanciaTramoKm).HasColumnName("distancia_tramo_km");
            entity.Property(e => e.DuracionTramoMin).HasColumnName("duracion_tramo_min");
            entity.Property(e => e.OrdenVisita).HasColumnName("orden_visita");
            entity.Property(e => e.RutaId).HasColumnName("ruta_id");

            entity.HasOne(d => d.Cita).WithOne(p => p.VisitaRutum)
                .HasForeignKey<VisitaRutum>(d => d.CitaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VISITA_CITA");

            entity.HasOne(d => d.Ruta).WithMany(p => p.VisitaRuta)
                .HasForeignKey(d => d.RutaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VISITA_RUTA");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
