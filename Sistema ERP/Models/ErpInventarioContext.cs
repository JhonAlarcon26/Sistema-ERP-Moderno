using Microsoft.EntityFrameworkCore;

namespace Sistema_ERP.Models;

public partial class ErpInventarioContext : DbContext
{
    public ErpInventarioContext()
    {
    }

    public ErpInventarioContext(DbContextOptions<ErpInventarioContext> options)
        : base(options)
    {
    }

    public virtual DbSet<InventarioProducto> InventarioProductos { get; set; }

    public virtual DbSet<InventarioServicio> InventarioServicios { get; set; }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<Cotizacione> Cotizaciones { get; set; }

    public virtual DbSet<CobroPendiente> CobrosPendientes { get; set; }

    public virtual DbSet<HistorialPago> HistorialPagos { get; set; }

    public virtual DbSet<DetalleCotizacionProducto> DetalleCotizacionProductos { get; set; }

    public virtual DbSet<DetalleCotizacionServicio> DetalleCotizacionServicios { get; set; }

    public virtual DbSet<InventarioCompra> InventarioCompras { get; set; }

    public virtual DbSet<Permiso> Permisos { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Proveedor> Proveedores { get; set; }

    public virtual DbSet<TipoProducto> TiposProducto { get; set; }

    public virtual DbSet<TipoServicio> TiposServicio { get; set; }

    public virtual DbSet<MetodoPago> MetodosPago { get; set; }
    public virtual DbSet<VisitaTecnica> VisitasTecnicas { get; set; }
    public virtual DbSet<ConfiguracionApi> ConfiguracionesApi { get; set; }
    public virtual DbSet<ConfiguracionSmtp> ConfiguracionesSmtp { get; set; }
    public virtual DbSet<EnlaceCompartido> EnlacesCompartidos { get; set; }
    public virtual DbSet<EnlaceProducto> EnlacesProductos { get; set; }
    public virtual DbSet<EnlaceServicio> EnlacesServicios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=erp_inventario;User Id=Sa;Password=75629487;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InventarioProducto>(entity =>
        {
            entity.HasKey(e => e.IdProducto).HasName("PK_Inventario_Productos");

            entity.ToTable("Inventario_Productos");

            entity.Property(e => e.IdProducto).HasColumnName("ID_Producto");
            entity.Property(e => e.NombreProducto)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("Nombre_Producto");
            entity.Property(e => e.Categoria)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Categoria");
            entity.Property(e => e.PrecioCompra)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("Precio_Compra");
            entity.Property(e => e.PrecioInterno)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("Precio_Interno");
            entity.Property(e => e.PrecioVentaSugerido)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("Precio_Venta_Sugerido");
            entity.Property(e => e.Stock)
                .HasColumnName("Stock");
            entity.Property(e => e.ImagenUrl)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("ImagenUrl");
            entity.Property(e => e.CodigoSN)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Codigo_SN");
            entity.Property(e => e.Descripcion)
                .IsUnicode(false)
                .HasColumnName("Descripcion");

            entity.Property(e => e.IdTipoProducto)
                .HasColumnName("ID_TipoProducto");

            entity.HasOne(d => d.TipoProductoNavigation).WithMany(p => p.InventarioProductos)
                .HasForeignKey(d => d.IdTipoProducto)
                .HasConstraintName("FK_Inventario_Productos_Tipos_Producto");
        });

        modelBuilder.Entity<InventarioServicio>(entity =>
        {
            entity.HasKey(e => e.IdServicio).HasName("PK_Inventario_Servicios");

            entity.ToTable("Inventario_Servicios");

            entity.Property(e => e.IdServicio).HasColumnName("ID_Servicio");
            entity.Property(e => e.NombreServicio)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("Nombre_Servicio");
            entity.Property(e => e.Categoria)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Categoria");
            entity.Property(e => e.PrecioVenta)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("Precio_Venta");
            entity.Property(e => e.Descripcion)
                .IsUnicode(false)
                .HasColumnName("Descripcion");
            entity.Property(e => e.ImagenUrl)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("ImagenUrl");

            entity.Property(e => e.IdTipoServicio)
                .HasColumnName("ID_TipoServicio");

            entity.HasOne(d => d.TipoServicioNavigation).WithMany(p => p.InventarioServicios)
                .HasForeignKey(d => d.IdTipoServicio)
                .HasConstraintName("FK_Inventario_Servicios_Tipos_Servicio");
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.IdCliente).HasName("PK__Clientes__E005FBFFE4AAA95F");

            entity.Property(e => e.IdCliente).HasColumnName("ID_Cliente");
            entity.Property(e => e.NitCi)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("NIT_CI");
            entity.Property(e => e.NombreRazonSocial)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("Nombre_RazonSocial");
            entity.Property(e => e.Tipo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Latitud).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Longitud).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Direccion).HasMaxLength(300).IsUnicode(false);
        });

        modelBuilder.Entity<Cotizacione>(entity =>
        {
            entity.HasKey(e => e.IdCotizacion).HasName("PK__Cotizaci__2646040F06B0C812");

            entity.Property(e => e.IdCotizacion).HasColumnName("ID_Cotizacion");
            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("Borrador");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(getDate())")
                .HasColumnType("Datetime");
            entity.Property(e => e.IdCliente).HasColumnName("ID_Cliente");
            entity.Property(e => e.IdUsuario).HasColumnName("ID_Usuario");


            entity.Property(e => e.NroCorrelativo).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.TipoOperacion).HasMaxLength(20).IsUnicode(false).HasDefaultValue("Cotizacion");
            entity.Property(e => e.PorcentajeImpuesto).HasColumnType("decimal(5, 2)").HasDefaultValue(0m);
            entity.Property(e => e.MontoImpuesto).HasColumnType("decimal(18, 2)").HasDefaultValue(0m);
            entity.Property(e => e.TotalConImpuesto).HasColumnType("decimal(18, 2)").HasDefaultValue(0m);
            entity.Property(e => e.EstadoPago).HasMaxLength(20).IsUnicode(false).HasDefaultValue("N/A");
            entity.Property(e => e.IdCotizacionOrigen).HasColumnName("IdCotizacionOrigen");
            entity.Property(e => e.IdMetodoPago).HasColumnName("ID_MetodoPago");

            entity.HasOne(d => d.IdMetodoPagoNavigation).WithMany(p => p.Cotizaciones)
                .HasForeignKey(d => d.IdMetodoPago)
                .HasConstraintName("FK_Cotizaciones_MetodoPago");

            entity.HasOne(d => d.IdCotizacionOrigenNavigation)
                .WithMany(p => p.InverseIdCotizacionOrigenNavigation)
                .HasForeignKey(d => d.IdCotizacionOrigen)
                .HasConstraintName("FK_Cotizaciones_CotizacionOrigen");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Cotizaciones)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Cotizacio__ID_Cl__412EB0B6");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Cotizaciones)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK__Cotizacio__ID_Us__6A30C649");
        });

        modelBuilder.Entity<CobroPendiente>(entity =>
        {
            entity.HasKey(e => e.IdCobro).HasName("PK__CobrosPendientes");
            entity.ToTable("CobrosPendientes");

            entity.Property(e => e.IdCobro).HasColumnName("ID_Cobro");
            entity.Property(e => e.IdOperacion).HasColumnName("ID_Operacion");
            entity.Property(e => e.MontoTotal).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MontoPagado).HasColumnType("decimal(18, 2)").HasDefaultValue(0m);
            entity.Property(e => e.MontoPendiente).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.FechaLimitePago).HasColumnType("Datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("Datetime").HasDefaultValueSql("(getDate())");
            entity.Property(e => e.EstadoCobro).HasMaxLength(20).IsUnicode(false).HasDefaultValue("Pendiente");
            entity.Property(e => e.NotasAdicionales).IsUnicode(false);

            entity.HasOne(d => d.IdOperacionNavigation).WithMany(p => p.CobrosPendientes)
                .HasForeignKey(d => d.IdOperacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cobros_Operacion");
        });

        modelBuilder.Entity<HistorialPago>(entity =>
        {
            entity.HasKey(e => e.IdPago).HasName("PK__HistorialPagos");
            entity.ToTable("HistorialPagos");

            entity.Property(e => e.IdPago).HasColumnName("ID_Pago");
            entity.Property(e => e.IdCobro).HasColumnName("ID_Cobro");
            entity.Property(e => e.MontoAbonado).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.FechaPago).HasColumnType("Datetime").HasDefaultValueSql("(getDate())");
            entity.Property(e => e.MetodoPago).HasMaxLength(50).IsUnicode(false).HasColumnName("MetodoAnterior");
            entity.Property(e => e.Comprobante).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.IdMetodoPago).HasColumnName("ID_MetodoPago");

            entity.HasOne(d => d.IdMetodoPagoNavigation).WithMany(p => p.HistorialPagos)
                .HasForeignKey(d => d.IdMetodoPago)
                .HasConstraintName("FK_Pagos_MetodoPago");

            entity.HasOne(d => d.IdCobroNavigation).WithMany(p => p.HistorialPagos)
                .HasForeignKey(d => d.IdCobro)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Pagos_Cobro");
        });

        modelBuilder.Entity<DetalleCotizacionProducto>(entity =>
        {
            entity.HasKey(e => e.IdDetalleProd).HasName("PK__Detalle___F9B46127157182E0");

            entity.ToTable("Detalle_Cotizacion_Productos");

            entity.Property(e => e.IdDetalleProd).HasColumnName("ID_Detalle_Prod");
            entity.Property(e => e.CostoReferencial)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("Costo_Referencial");
            entity.Property(e => e.IdCotizacion).HasColumnName("ID_Cotizacion");
            entity.Property(e => e.IdProducto).HasColumnName("ID_Producto");
            entity.Property(e => e.PrecioVendido)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("Precio_Vendido");

            entity.HasOne(d => d.IdCotizacionNavigation).WithMany(p => p.DetalleCotizacionProductos)
                .HasForeignKey(d => d.IdCotizacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Detalle_C__ID_Co__45F365D3");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.DetalleCotizacionProductos)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Detalle_Cotizacion_Prod_InvProd");
        });

        modelBuilder.Entity<DetalleCotizacionServicio>(entity =>
        {
            entity.HasKey(e => e.IdDetalleServ).HasName("PK__Detalle___EF9E5D6E3329A3C7");

            entity.ToTable("Detalle_Cotizacion_Servicios");

            entity.Property(e => e.IdDetalleServ).HasColumnName("ID_Detalle_Serv");
            entity.Property(e => e.Cantidad).HasDefaultValue(1);
            entity.Property(e => e.IdCotizacion).HasColumnName("ID_Cotizacion");
            entity.Property(e => e.IdServicio).HasColumnName("ID_Servicio");
            entity.Property(e => e.PrecioCobrado)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("Precio_Cobrado");

            entity.HasOne(d => d.IdCotizacionNavigation).WithMany(p => p.DetalleCotizacionServicios)
                .HasForeignKey(d => d.IdCotizacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Detalle_C__ID_Co__4AB81AF0");

            entity.HasOne(d => d.IdServicioNavigation).WithMany(p => p.DetalleCotizacionServicios)
                .HasForeignKey(d => d.IdServicio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Detalle_Cotizacion_Serv_InvServ");
        });

        modelBuilder.Entity<InventarioCompra>(entity =>
        {
            entity.HasKey(e => e.IdIngreso).HasName("PK__Inventar__2B7A7D353BE5637E");

            entity.ToTable("Inventario_Compras");

            entity.Property(e => e.IdIngreso).HasColumnName("ID_Ingreso");
            entity.Property(e => e.CantidadComprada).HasColumnName("Cantidad_Comprada");
            entity.Property(e => e.CostoCompraUnitario)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("Costo_Compra_Unitario");
            entity.Property(e => e.FechaIngreso)
                .HasDefaultValueSql("(getDate())")
                .HasColumnType("Datetime")
                .HasColumnName("Fecha_Ingreso");
            entity.Property(e => e.IdProducto).HasColumnName("ID_Producto");
            entity.Property(e => e.IdUsuario).HasColumnName("ID_Usuario");
            entity.Property(e => e.IdProveedor).HasColumnName("ID_Proveedor");
            entity.Property(e => e.IdMetodoPago).HasColumnName("ID_MetodoPago");
            entity.Property(e => e.PrecioRefHistorico).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PrecioVentaHistorico).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.InventarioCompras)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Inventario_Compras_InvProd");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.InventarioCompras)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK__Inventari__ID_Us__6B24EA82");

            entity.HasOne(d => d.IdProveedorNavigation).WithMany(p => p.InventarioCompras)
                .HasForeignKey(d => d.IdProveedor)
                .HasConstraintName("FK_Compras_Proveedor");

            entity.HasOne(d => d.IdMetodoPagoNavigation).WithMany(p => p.InventarioCompras)
                .HasForeignKey(d => d.IdMetodoPago)
                .HasConstraintName("FK_Compras_MetodoPago");
        });

        modelBuilder.Entity<Proveedor>(entity =>
        {
            entity.HasKey(e => e.IdProveedor).HasName("PK_Proveedores");

            entity.ToTable("Proveedores");

            entity.Property(e => e.IdProveedor).HasColumnName("ID_Proveedor");
            entity.Property(e => e.Nombre).HasMaxLength(200).IsUnicode(false);
            entity.Property(e => e.TipoProveedor).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.NitCi).HasMaxLength(50).IsUnicode(false).HasColumnName("NIT_CI");
            entity.Property(e => e.Telefono).HasMaxLength(30).IsUnicode(false);
            entity.Property(e => e.Direccion).HasMaxLength(300).IsUnicode(false);
            entity.Property(e => e.Latitud).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Longitud).HasMaxLength(50).IsUnicode(false);
        });

        modelBuilder.Entity<TipoProducto>(entity =>
        {
            entity.HasKey(e => e.IdTipoProducto).HasName("PK_Tipos_Producto");

            entity.ToTable("Tipos_Producto");

            entity.Property(e => e.IdTipoProducto).HasColumnName("ID_TipoProducto");
            entity.Property(e => e.Nombre).HasMaxLength(100).IsUnicode(false);
        });

        modelBuilder.Entity<TipoServicio>(entity =>
        {
            entity.HasKey(e => e.IdTipoServicio).HasName("PK_Tipos_Servicio");

            entity.ToTable("Tipos_Servicio");

            entity.Property(e => e.IdTipoServicio).HasColumnName("ID_TipoServicio");
            entity.Property(e => e.Nombre).HasMaxLength(100).IsUnicode(false);
        });

        modelBuilder.Entity<Permiso>(entity =>
        {
            entity.HasKey(e => e.IdPermiso).HasName("PK__Permisos__D5B666CCB5164112");

            entity.ToTable("Permisos");

            entity.HasIndex(e => e.NombrePermiso, "UQ__Permisos__F6479DD98817AE35").IsUnique();

            entity.Property(e => e.IdPermiso).HasColumnName("ID_Permiso");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.NombrePermiso)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Nombre_Permiso");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("PK__Roles__202AD220D4C93C82");

            entity.HasIndex(e => e.NombreRol, "UQ__Roles__320Fda7D67362AE6").IsUnique();

            entity.Property(e => e.IdRol).HasColumnName("ID_Rol");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.NombreRol)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Nombre_Rol");

            entity.HasMany(d => d.IdPermisos).WithMany(p => p.IdRols)
                .UsingEntity<Dictionary<string, object>>(
                    "RolesPermiso",
                    r => r.HasOne<Permiso>().WithMany()
                        .HasForeignKey("IdPermiso")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Roles_Per__ID_Pe__6383C8BA"),
                    l => l.HasOne<Role>().WithMany()
                        .HasForeignKey("IdRol")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Roles_Per__ID_Ro__628FA481"),
                    j =>
                    {
                        j.HasKey("IdRol", "IdPermiso").HasName("PK__Roles_Pe__FD71B44C4D71B8E0");
                        j.ToTable("Roles_Permisos");
                        j.IndexerProperty<int>("IdRol").HasColumnName("ID_Rol");
                        j.IndexerProperty<int>("IdPermiso").HasColumnName("ID_Permiso");
                    });
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__Usuarios__DE4431C52D646E89");

            entity.HasIndex(e => e.Username, "UQ__Usuarios__536C85E4039B5F70").IsUnique();

            entity.Property(e => e.IdUsuario).HasColumnName("ID_Usuario");
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getDate())")
                .HasColumnType("Datetime")
                .HasColumnName("Fecha_Creacion");
            entity.Property(e => e.IdRol).HasColumnName("ID_Rol");
            entity.Property(e => e.NombreCompleto)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("Nombre_Completo");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("Password_Hash");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Usuarios__ID_Rol__6754599E");
        });

        modelBuilder.Entity<VisitaTecnica>(entity =>
        {
            entity.HasKey(e => e.IdVisita).HasName("PK_Visitas_Tecnicas");
            entity.ToTable("Visitas_Tecnicas");

            entity.Property(e => e.IdVisita).HasColumnName("ID_Visita");
            entity.Property(e => e.IdCliente).HasColumnName("ID_Cliente");
            entity.Property(e => e.IdTecnico).HasColumnName("ID_Tecnico");
            entity.Property(e => e.IdCotizacion).HasColumnName("ID_Cotizacion");
            entity.Property(e => e.FechaVisita).HasColumnType("Datetime").HasColumnName("Fecha_Visita");
            entity.Property(e => e.FechaRegistro).HasColumnType("Datetime").HasDefaultValueSql("(getDate())").HasColumnName("Fecha_Registro");
            entity.Property(e => e.Estado).HasMaxLength(50).IsUnicode(false).HasDefaultValue("Pendiente");
            entity.Property(e => e.Empresa).HasMaxLength(200).IsUnicode(false);

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.VisitasTecnicas)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Visitas_Cliente");

            entity.HasOne(d => d.IdTecnicoNavigation).WithMany(p => p.VisitasTecnicas)
                .HasForeignKey(d => d.IdTecnico)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Visitas_Tecnico");

            entity.HasOne(d => d.IdCotizacionNavigation).WithMany(p => p.VisitasTecnicas)
                .HasForeignKey(d => d.IdCotizacion)
                .HasConstraintName("FK_Visitas_Cotizacion");
        });

        modelBuilder.Entity<ConfiguracionApi>(entity =>
        {
            entity.HasKey(e => e.Idapi);
            entity.ToTable("Configuraciones_Api");
            entity.Property(e => e.Nombre).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.ApiKey).IsUnicode(false);
            entity.Property(e => e.Proveedor).HasMaxLength(255).IsUnicode(false);

            entity.Property(e => e.IdUsuario).HasColumnName("ID_Usuario");

            entity.HasOne(d => d.UsuarioNavigation).WithMany(p => p.ConfiguracionesApi)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK_Configuraciones_Api_Usuarios");
        });

        modelBuilder.Entity<ConfiguracionSmtp>(entity =>
        {
            entity.HasKey(e => e.IdSmtp);
            entity.ToTable("Configuraciones_Smtp");
            entity.Property(e => e.NombrePerfil).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.Host).HasMaxLength(150).IsUnicode(false);
            entity.Property(e => e.Email).HasMaxLength(150).IsUnicode(false);
            entity.Property(e => e.Password).HasMaxLength(150).IsUnicode(false);
            entity.Property(e => e.SenderName).HasMaxLength(150).IsUnicode(false);

            entity.Property(e => e.IdUsuario).HasColumnName("ID_Usuario");

            entity.HasOne(d => d.UsuarioNavigation).WithMany(p => p.ConfiguracionesSmtp)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK_Configuraciones_Smtp_Usuarios");
        });



        modelBuilder.Entity<EnlaceCompartido>(entity =>
        {
            entity.HasKey(e => e.IdEnlace);
            entity.ToTable("EnlacesCompartidos");
            entity.Property(e => e.Token).HasMaxLength(100).IsRequired();
            entity.Property(e => e.FechaCreacion).HasColumnType("Datetime").HasDefaultValueSql("(getdate())").ValueGeneratedOnAdd();
            entity.Property(e => e.FechaExpiracion).HasColumnType("Datetime");
            entity.Property(e => e.EstaActivo).HasDefaultValue(true).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<EnlaceProducto>(entity =>
        {
            entity.HasKey(e => new { e.IdEnlace, e.IdProducto });
            entity.ToTable("Enlaces_Productos");
            entity.Property(e => e.IdEnlace).HasColumnName("ID_Enlace");
            entity.Property(e => e.IdProducto).HasColumnName("ID_Producto");

            entity.HasOne(d => d.Enlace)
                .WithMany(p => p.EnlacesProductos)
                .HasForeignKey(d => d.IdEnlace)
                .HasConstraintName("FK_Enlaces_Productos_Enlace");

            entity.HasOne(d => d.Producto)
                .WithMany(p => p.EnlacesProductos)
                .HasForeignKey(d => d.IdProducto)
                .HasConstraintName("FK_Enlaces_Productos_Producto");
        });

        modelBuilder.Entity<EnlaceServicio>(entity =>
        {
            entity.HasKey(e => new { e.IdEnlace, e.IdServicio });
            entity.ToTable("Enlaces_Servicios");
            entity.Property(e => e.IdEnlace).HasColumnName("ID_Enlace");
            entity.Property(e => e.IdServicio).HasColumnName("ID_Servicio");

            entity.HasOne(d => d.Enlace)
                .WithMany(p => p.EnlacesServicios)
                .HasForeignKey(d => d.IdEnlace)
                .HasConstraintName("FK_Enlaces_Servicios_Enlace");

            entity.HasOne(d => d.Servicio)
                .WithMany(p => p.EnlacesServicios)
                .HasForeignKey(d => d.IdServicio)
                .HasConstraintName("FK_Enlaces_Servicios_Servicio");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
