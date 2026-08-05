-- =============================================================================
-- LIMPIEZA Y SEMILLA DE CLIENTES
-- Base de datos: Piscinas_Y_Multiservicios
-- Ejecutar como un solo batch (sin GO intermedios), idealmente desde SSMS.
--
-- Qué hace:
--   1) Borra TODOS los clientes existentes (seg.USUARIO con rol_id = 3) y todo
--      lo que depende de ellos: direcciones, teléfonos, piscinas, citas,
--      servicios, tareas, inspecciones, encuestas, cotizaciones, facturas,
--      carritos y proyectos de construcción ligados a esos clientes.
--   2) NO toca administradores ni técnicos (seg.USUARIO con rol_id 1 o 2),
--      ni vehículos, catálogo de productos, roles o la división territorial
--      (geo.PROVINCIA/CANTON/DISTRITO).
--   3) Crea 7 clientes nuevos con datos correctos y consistentes: usuario,
--      cliente, teléfono, dirección CON distrito real y coordenadas reales
--      (repartidos en San José, Alajuela, Heredia y Cartago), y una piscina
--      cada uno.
--   4) Les agenda citas a los técnicos que ya existen en el sistema: varias
--      HOY para un mismo técnico en San José (para que Ruta Optimizada tenga
--      qué mostrar de inmediato), varias MAÑANA para otro técnico, y una cita
--      ya completada con su servicio y tareas cerradas (para Servicios
--      Técnicos / historial).
--   5) Si ya existen productos en el catálogo (inv.PRODUCTO), genera además
--      una cotización Aceptada y su factura correspondiente para un cliente,
--      para poder probar Cotizaciones / Facturación de una vez. Si no hay
--      productos cargados, esa parte se omite (no se inventan productos).
--
-- Requisitos antes de correr esto:
--   - Que ya exista al menos un técnico (seg.USUARIO con rol_id = 2). Si no
--     hay ninguno, el script se detiene sin cambiar nada.
--   - Que la división territorial ya esté completa (las 7 provincias). Si
--     falta San José, Alajuela, Heredia o Cartago, el script se detiene sin
--     cambiar nada — reiniciá la app primero para que el seeder las cargue.
--
-- Es irreversible: hacé un respaldo antes si te importa conservar los
-- clientes actuales. Todo corre dentro de una sola transacción: si algo
-- falla a medio camino, se revierte solo y no queda nada a medias.
-- =============================================================================

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

BEGIN TRY

    ---------------------------------------------------------------------------
    -- 0. Validaciones previas
    ---------------------------------------------------------------------------
    IF NOT EXISTS (SELECT 1 FROM seg.USUARIO WHERE rol_id = 2 AND activo = 1)
    BEGIN
        RAISERROR('No hay ningún técnico activo (seg.USUARIO rol_id=2). Creá al menos uno antes de correr este script.', 16, 1);
    END

    DECLARE @DistritoSanJose INT = (
        SELECT TOP 1 d.id FROM geo.DISTRITO d
        JOIN geo.CANTON c ON d.canton_id = c.id
        JOIN geo.PROVINCIA p ON c.provincia_id = p.id
        WHERE p.nombre = N'San José' AND c.nombre = N'San José'
        ORDER BY d.id
    );
    DECLARE @DistritoEscazu INT = (
        SELECT TOP 1 d.id FROM geo.DISTRITO d
        JOIN geo.CANTON c ON d.canton_id = c.id
        JOIN geo.PROVINCIA p ON c.provincia_id = p.id
        WHERE p.nombre = N'San José' AND c.nombre = N'Escazú'
        ORDER BY d.id
    );
    DECLARE @DistritoDesamparados INT = (
        SELECT TOP 1 d.id FROM geo.DISTRITO d
        JOIN geo.CANTON c ON d.canton_id = c.id
        JOIN geo.PROVINCIA p ON c.provincia_id = p.id
        WHERE p.nombre = N'San José' AND c.nombre = N'Desamparados'
        ORDER BY d.id
    );
    DECLARE @DistritoGoicoechea INT = (
        SELECT TOP 1 d.id FROM geo.DISTRITO d
        JOIN geo.CANTON c ON d.canton_id = c.id
        JOIN geo.PROVINCIA p ON c.provincia_id = p.id
        WHERE p.nombre = N'San José' AND c.nombre = N'Goicoechea'
        ORDER BY d.id
    );
    DECLARE @DistritoAlajuela INT = (
        SELECT TOP 1 d.id FROM geo.DISTRITO d
        JOIN geo.CANTON c ON d.canton_id = c.id
        JOIN geo.PROVINCIA p ON c.provincia_id = p.id
        WHERE p.nombre = N'Alajuela' AND c.nombre = N'Alajuela'
        ORDER BY d.id
    );
    DECLARE @DistritoHeredia INT = (
        SELECT TOP 1 d.id FROM geo.DISTRITO d
        JOIN geo.CANTON c ON d.canton_id = c.id
        JOIN geo.PROVINCIA p ON c.provincia_id = p.id
        WHERE p.nombre = N'Heredia' AND c.nombre = N'Heredia'
        ORDER BY d.id
    );
    DECLARE @DistritoCartago INT = (
        SELECT TOP 1 d.id FROM geo.DISTRITO d
        JOIN geo.CANTON c ON d.canton_id = c.id
        JOIN geo.PROVINCIA p ON c.provincia_id = p.id
        WHERE p.nombre = N'Cartago' AND c.nombre = N'Cartago'
        ORDER BY d.id
    );

    IF @DistritoSanJose IS NULL OR @DistritoAlajuela IS NULL OR @DistritoHeredia IS NULL OR @DistritoCartago IS NULL
    BEGIN
        RAISERROR('Falta división territorial (San José/Alajuela/Heredia/Cartago). Reiniciá la app para que el seeder termine de cargar las 7 provincias antes de correr este script.', 16, 1);
    END

    -- Si algún cantón de San José no existiera con ese nombre exacto, usamos
    -- el distrito de San José central como respaldo para no romper el script.
    SET @DistritoEscazu = ISNULL(@DistritoEscazu, @DistritoSanJose);
    SET @DistritoDesamparados = ISNULL(@DistritoDesamparados, @DistritoSanJose);
    SET @DistritoGoicoechea = ISNULL(@DistritoGoicoechea, @DistritoSanJose);

    DECLARE @Tecnico1 INT = (SELECT TOP 1 id FROM seg.USUARIO WHERE rol_id = 2 AND activo = 1 ORDER BY id);
    DECLARE @Tecnico2 INT = (SELECT id FROM seg.USUARIO WHERE rol_id = 2 AND activo = 1 ORDER BY id OFFSET 1 ROWS FETCH NEXT 1 ROWS ONLY);
    SET @Tecnico2 = ISNULL(@Tecnico2, @Tecnico1);

    DECLARE @Admin INT = (SELECT TOP 1 id FROM seg.USUARIO WHERE rol_id = 1 AND activo = 1 ORDER BY id);

    ---------------------------------------------------------------------------
    -- 1. LIMPIEZA: borrar todos los clientes y lo que depende de ellos
    ---------------------------------------------------------------------------
    DECLARE @ClientesABorrar TABLE (cliente_id INT, usuario_id INT);
    INSERT INTO @ClientesABorrar (cliente_id, usuario_id)
    SELECT c.id, c.usuario_id FROM cli.CLIENTE c;

    DECLARE @CitasABorrar TABLE (cita_id INT);
    INSERT INTO @CitasABorrar (cita_id)
    SELECT ci.id
    FROM ops.CITA ci
    JOIN act.PISCINA pi ON ci.piscina_id = pi.id
    WHERE pi.cliente_id IN (SELECT cliente_id FROM @ClientesABorrar);

    DECLARE @ServiciosABorrar TABLE (servicio_id INT);
    INSERT INTO @ServiciosABorrar (servicio_id)
    SELECT s.id FROM ops.SERVICIO s WHERE s.cita_id IN (SELECT cita_id FROM @CitasABorrar);

    DELETE FROM ven.DETALLE_FACTURA WHERE factura_id IN (SELECT id FROM ven.FACTURA WHERE cliente_id IN (SELECT cliente_id FROM @ClientesABorrar));
    DELETE FROM ven.FACTURA WHERE cliente_id IN (SELECT cliente_id FROM @ClientesABorrar);

    DELETE FROM ven.DETALLE_COTIZACION WHERE cotizacion_id IN (SELECT id FROM ven.COTIZACION WHERE cliente_id IN (SELECT cliente_id FROM @ClientesABorrar));
    DELETE FROM ven.COTIZACION WHERE cliente_id IN (SELECT cliente_id FROM @ClientesABorrar);

    DELETE FROM ven.ITEM_CARRITO WHERE carrito_id IN (SELECT id FROM ven.CARRITO WHERE cliente_id IN (SELECT cliente_id FROM @ClientesABorrar));
    DELETE FROM ven.CARRITO WHERE cliente_id IN (SELECT cliente_id FROM @ClientesABorrar);

    DELETE FROM pry.PROYECTO_CONSTRUCCION WHERE cliente_id IN (SELECT cliente_id FROM @ClientesABorrar);

    DELETE FROM fin.GASTO_OPERATIVO WHERE cita_id IN (SELECT cita_id FROM @CitasABorrar);

    DELETE FROM ops.NOTIFICACION
    WHERE cita_id IN (SELECT cita_id FROM @CitasABorrar)
       OR usuario_id IN (SELECT usuario_id FROM @ClientesABorrar);

    DELETE FROM crm.ENCUESTA WHERE servicio_id IN (SELECT servicio_id FROM @ServiciosABorrar);
    DELETE FROM ops.INSPECCION WHERE servicio_id IN (SELECT servicio_id FROM @ServiciosABorrar);
    DELETE FROM ops.TAREA_SERVICIO WHERE servicio_id IN (SELECT servicio_id FROM @ServiciosABorrar);
    DELETE FROM ops.SERVICIO WHERE cita_id IN (SELECT cita_id FROM @CitasABorrar);
    DELETE FROM log.VISITA_RUTA WHERE cita_id IN (SELECT cita_id FROM @CitasABorrar);
    DELETE FROM ops.CITA WHERE id IN (SELECT cita_id FROM @CitasABorrar);

    DELETE FROM act.PISCINA_EQUIPAMIENTO WHERE piscina_id IN (SELECT id FROM act.PISCINA WHERE cliente_id IN (SELECT cliente_id FROM @ClientesABorrar));
    DELETE FROM act.PISCINA WHERE cliente_id IN (SELECT cliente_id FROM @ClientesABorrar);

    DELETE FROM cli.DIRECCION_CLIENTE WHERE cliente_id IN (SELECT cliente_id FROM @ClientesABorrar);
    DELETE FROM cli.TELEFONOS_CLIENTE WHERE cliente_id IN (SELECT cliente_id FROM @ClientesABorrar);
    DELETE FROM cli.CLIENTE WHERE id IN (SELECT cliente_id FROM @ClientesABorrar);

    -- Antes de borrar los USUARIO de los clientes: BITACORA_AUDITORIA y
    -- ANUNCIO también tienen FK hacia seg.USUARIO, y en ambas tablas la
    -- columna es NOT NULL (no se puede simplemente limpiar la referencia).
    -- Se borran las filas de bitácora/anuncios que hubiera a nombre de estos
    -- clientes (en la práctica no debería haber anuncios, los publican
    -- admin/técnicos, pero se cubre por seguridad).
    DELETE FROM aud.BITACORA_AUDITORIA WHERE usuario_id IN (SELECT usuario_id FROM @ClientesABorrar);
    DELETE FROM crm.ANUNCIO WHERE autor_id IN (SELECT usuario_id FROM @ClientesABorrar);

    DELETE FROM seg.USUARIO WHERE id IN (SELECT usuario_id FROM @ClientesABorrar);

    PRINT CONCAT('Clientes eliminados: ', (SELECT COUNT(*) FROM @ClientesABorrar));

    ---------------------------------------------------------------------------
    -- 2. SEMILLA: 7 clientes nuevos con datos completos y correctos
    ---------------------------------------------------------------------------
    DECLARE @u INT, @cl INT, @dir INT, @pis INT, @cita INT, @serv INT;

    -- Tabla de trabajo para poder referenciar cada cliente/piscina más abajo
    -- al armar las citas de Ruta Optimizada.
    DECLARE @Clientes TABLE (
        etiqueta VARCHAR(20) PRIMARY KEY,
        cliente_id INT,
        piscina_id INT
    );

    ----- Cliente 1: San José centro ------------------------------------------
    INSERT INTO seg.USUARIO (rol_id, nombre, apellido_paterno, apellido_materno, correo, contrasena, activo, fecha_creacion)
    VALUES (3, 'María', 'Rodríguez', 'Solano', 'maria.rodriguez@correo.com', 'Cliente123', 1, GETDATE());
    SET @u = SCOPE_IDENTITY();
    INSERT INTO cli.CLIENTE (usuario_id, notas) VALUES (@u, 'Cliente semilla — San José centro');
    SET @cl = SCOPE_IDENTITY();
    INSERT INTO cli.TELEFONOS_CLIENTE (cliente_id, tipo_telefono, numero_telefono, es_principal) VALUES (@cl, 'Principal', '8801-1001', 1);
    INSERT INTO cli.DIRECCION_CLIENTE (cliente_id, distrito_id, tipo_direccion, detalles, es_principal, latitud, longitud)
    VALUES (@cl, @DistritoSanJose, 'Casa', 'Avenida Central, 200 metros norte del Parque Central', 1, 9.9333, -84.0833);
    SET @dir = SCOPE_IDENTITY();
    INSERT INTO act.PISCINA (cliente_id, direccion_id, tipo, volumen_m3, estado) VALUES (@cl, @dir, 'Residencial', 45.0, 'Activa');
    SET @pis = SCOPE_IDENTITY();
    INSERT INTO @Clientes VALUES ('SJ1', @cl, @pis);

    ----- Cliente 2: Escazú ----------------------------------------------------
    INSERT INTO seg.USUARIO (rol_id, nombre, apellido_paterno, apellido_materno, correo, contrasena, activo, fecha_creacion)
    VALUES (3, 'Luis', 'Fernández', 'Jiménez', 'luis.fernandez@correo.com', 'Cliente123', 1, GETDATE());
    SET @u = SCOPE_IDENTITY();
    INSERT INTO cli.CLIENTE (usuario_id, notas) VALUES (@u, 'Cliente semilla — Escazú');
    SET @cl = SCOPE_IDENTITY();
    INSERT INTO cli.TELEFONOS_CLIENTE (cliente_id, tipo_telefono, numero_telefono, es_principal) VALUES (@cl, 'Principal', '8801-1002', 1);
    INSERT INTO cli.DIRECCION_CLIENTE (cliente_id, distrito_id, tipo_direccion, detalles, es_principal, latitud, longitud)
    VALUES (@cl, @DistritoEscazu, 'Casa', 'San Rafael de Escazú, Residencial Trejos Montealegre', 1, 9.9189, -84.1478);
    SET @dir = SCOPE_IDENTITY();
    INSERT INTO act.PISCINA (cliente_id, direccion_id, tipo, volumen_m3, estado) VALUES (@cl, @dir, 'Residencial', 60.0, 'Activa');
    SET @pis = SCOPE_IDENTITY();
    INSERT INTO @Clientes VALUES ('SJ2', @cl, @pis);

    ----- Cliente 3: Desamparados -----------------------------------------------
    INSERT INTO seg.USUARIO (rol_id, nombre, apellido_paterno, apellido_materno, correo, contrasena, activo, fecha_creacion)
    VALUES (3, 'Ana', 'Castro', 'Morales', 'ana.castro@correo.com', 'Cliente123', 1, GETDATE());
    SET @u = SCOPE_IDENTITY();
    INSERT INTO cli.CLIENTE (usuario_id, notas) VALUES (@u, 'Cliente semilla — Desamparados');
    SET @cl = SCOPE_IDENTITY();
    INSERT INTO cli.TELEFONOS_CLIENTE (cliente_id, tipo_telefono, numero_telefono, es_principal) VALUES (@cl, 'Principal', '8801-1003', 1);
    INSERT INTO cli.DIRECCION_CLIENTE (cliente_id, distrito_id, tipo_direccion, detalles, es_principal, latitud, longitud)
    VALUES (@cl, @DistritoDesamparados, 'Casa', 'San Rafael Arriba de Desamparados, 300 sur de la iglesia', 1, 9.8977, -84.0663);
    SET @dir = SCOPE_IDENTITY();
    INSERT INTO act.PISCINA (cliente_id, direccion_id, tipo, volumen_m3, estado) VALUES (@cl, @dir, 'Residencial', 38.5, 'Activa');
    SET @pis = SCOPE_IDENTITY();
    INSERT INTO @Clientes VALUES ('SJ3', @cl, @pis);

    ----- Cliente 4: Goicoechea (Guadalupe) -------------------------------------
    INSERT INTO seg.USUARIO (rol_id, nombre, apellido_paterno, apellido_materno, correo, contrasena, activo, fecha_creacion)
    VALUES (3, 'Jorge', 'Vargas', 'Chacón', 'jorge.vargas@correo.com', 'Cliente123', 1, GETDATE());
    SET @u = SCOPE_IDENTITY();
    INSERT INTO cli.CLIENTE (usuario_id, notas) VALUES (@u, 'Cliente semilla — Goicoechea');
    SET @cl = SCOPE_IDENTITY();
    INSERT INTO cli.TELEFONOS_CLIENTE (cliente_id, tipo_telefono, numero_telefono, es_principal) VALUES (@cl, 'Principal', '8801-1004', 1);
    INSERT INTO cli.DIRECCION_CLIENTE (cliente_id, distrito_id, tipo_direccion, detalles, es_principal, latitud, longitud)
    VALUES (@cl, @DistritoGoicoechea, 'Casa', 'Guadalupe centro, del Automercado 400 metros este', 1, 9.9500, -84.0333);
    SET @dir = SCOPE_IDENTITY();
    INSERT INTO act.PISCINA (cliente_id, direccion_id, tipo, volumen_m3, estado) VALUES (@cl, @dir, 'Residencial', 52.0, 'Activa');
    SET @pis = SCOPE_IDENTITY();
    INSERT INTO @Clientes VALUES ('SJ4', @cl, @pis);

    ----- Cliente 5: Alajuela ---------------------------------------------------
    INSERT INTO seg.USUARIO (rol_id, nombre, apellido_paterno, apellido_materno, correo, contrasena, activo, fecha_creacion)
    VALUES (3, 'Karla', 'Mendoza', 'Araya', 'karla.mendoza@correo.com', 'Cliente123', 1, GETDATE());
    SET @u = SCOPE_IDENTITY();
    INSERT INTO cli.CLIENTE (usuario_id, notas) VALUES (@u, 'Cliente semilla — Alajuela');
    SET @cl = SCOPE_IDENTITY();
    INSERT INTO cli.TELEFONOS_CLIENTE (cliente_id, tipo_telefono, numero_telefono, es_principal) VALUES (@cl, 'Principal', '8801-1005', 1);
    INSERT INTO cli.DIRECCION_CLIENTE (cliente_id, distrito_id, tipo_direccion, detalles, es_principal, latitud, longitud)
    VALUES (@cl, @DistritoAlajuela, 'Casa', 'Alajuela centro, costado sur del Aeropuerto Juan Santamaría', 1, 10.0162, -84.2116);
    SET @dir = SCOPE_IDENTITY();
    INSERT INTO act.PISCINA (cliente_id, direccion_id, tipo, volumen_m3, estado) VALUES (@cl, @dir, 'Residencial', 48.0, 'Activa');
    SET @pis = SCOPE_IDENTITY();
    INSERT INTO @Clientes VALUES ('ALJ', @cl, @pis);

    ----- Cliente 6: Heredia -----------------------------------------------------
    INSERT INTO seg.USUARIO (rol_id, nombre, apellido_paterno, apellido_materno, correo, contrasena, activo, fecha_creacion)
    VALUES (3, 'Diego', 'Ramírez', 'Salas', 'diego.ramirez@correo.com', 'Cliente123', 1, GETDATE());
    SET @u = SCOPE_IDENTITY();
    INSERT INTO cli.CLIENTE (usuario_id, notas) VALUES (@u, 'Cliente semilla — Heredia');
    SET @cl = SCOPE_IDENTITY();
    INSERT INTO cli.TELEFONOS_CLIENTE (cliente_id, tipo_telefono, numero_telefono, es_principal) VALUES (@cl, 'Principal', '8801-1006', 1);
    INSERT INTO cli.DIRECCION_CLIENTE (cliente_id, distrito_id, tipo_direccion, detalles, es_principal, latitud, longitud)
    VALUES (@cl, @DistritoHeredia, 'Casa', 'Heredia centro, 100 metros oeste de la Universidad Nacional', 1, 10.0024, -84.1165);
    SET @dir = SCOPE_IDENTITY();
    INSERT INTO act.PISCINA (cliente_id, direccion_id, tipo, volumen_m3, estado) VALUES (@cl, @dir, 'Residencial', 55.0, 'Activa');
    SET @pis = SCOPE_IDENTITY();
    INSERT INTO @Clientes VALUES ('HER', @cl, @pis);

    ----- Cliente 7: Cartago -----------------------------------------------------
    INSERT INTO seg.USUARIO (rol_id, nombre, apellido_paterno, apellido_materno, correo, contrasena, activo, fecha_creacion)
    VALUES (3, 'Sofía', 'Navarro', 'Quesada', 'sofia.navarro@correo.com', 'Cliente123', 1, GETDATE());
    SET @u = SCOPE_IDENTITY();
    INSERT INTO cli.CLIENTE (usuario_id, notas) VALUES (@u, 'Cliente semilla — Cartago');
    SET @cl = SCOPE_IDENTITY();
    INSERT INTO cli.TELEFONOS_CLIENTE (cliente_id, tipo_telefono, numero_telefono, es_principal) VALUES (@cl, 'Principal', '8801-1007', 1);
    INSERT INTO cli.DIRECCION_CLIENTE (cliente_id, distrito_id, tipo_direccion, detalles, es_principal, latitud, longitud)
    VALUES (@cl, @DistritoCartago, 'Casa', 'Cartago centro, frente a la Basílica de los Ángeles', 1, 9.8644, -83.9194);
    SET @dir = SCOPE_IDENTITY();
    INSERT INTO act.PISCINA (cliente_id, direccion_id, tipo, volumen_m3, estado) VALUES (@cl, @dir, 'Residencial', 42.0, 'Activa');
    SET @pis = SCOPE_IDENTITY();
    INSERT INTO @Clientes VALUES ('CTG', @cl, @pis);

    ---------------------------------------------------------------------------
    -- 3. CITAS: hoy en San José (mismo técnico, para probar Ruta Optimizada),
    --    mañana en Alajuela/Heredia/Cartago (segundo técnico), y una cita
    --    completada con servicio y tareas cerradas (para historial).
    ---------------------------------------------------------------------------

    -- Tres citas HOY para @Tecnico1, todas en San José: alcanza para que
    -- /RutaOptimizada/Index?tecnicoId=@Tecnico1&fecha=hoy tenga 2+ paradas
    -- con coordenadas.
    INSERT INTO ops.CITA (piscina_id, tecnico_id, fecha_hora, tipo, estado, notas)
    SELECT piscina_id, @Tecnico1, CAST(CAST(GETDATE() AS DATE) AS DATETIME) + '08:00', 'Mantenimiento', 'Confirmada', 'Mantenimiento rutinario'
    FROM @Clientes WHERE etiqueta = 'SJ1';

    INSERT INTO ops.CITA (piscina_id, tecnico_id, fecha_hora, tipo, estado, notas)
    SELECT piscina_id, @Tecnico1, CAST(CAST(GETDATE() AS DATE) AS DATETIME) + '10:30', 'Mantenimiento', 'Confirmada', 'Mantenimiento rutinario'
    FROM @Clientes WHERE etiqueta = 'SJ2';

    INSERT INTO ops.CITA (piscina_id, tecnico_id, fecha_hora, tipo, estado, notas)
    SELECT piscina_id, @Tecnico1, CAST(CAST(GETDATE() AS DATE) AS DATETIME) + '13:00', 'Inspección', 'Confirmada', 'Inspección de rutina'
    FROM @Clientes WHERE etiqueta = 'SJ3';

    -- Una cita MAÑANA en San José también, para tener agenda a futuro.
    INSERT INTO ops.CITA (piscina_id, tecnico_id, fecha_hora, tipo, estado, notas)
    SELECT piscina_id, @Tecnico1, CAST(DATEADD(DAY, 1, CAST(GETDATE() AS DATE)) AS DATETIME) + '09:00', 'Mantenimiento', 'Confirmada', 'Mantenimiento rutinario'
    FROM @Clientes WHERE etiqueta = 'SJ4';

    -- Tres citas MAÑANA para @Tecnico2 en Alajuela/Heredia/Cartago.
    INSERT INTO ops.CITA (piscina_id, tecnico_id, fecha_hora, tipo, estado, notas)
    SELECT piscina_id, @Tecnico2, CAST(DATEADD(DAY, 1, CAST(GETDATE() AS DATE)) AS DATETIME) + '08:30', 'Mantenimiento', 'Confirmada', 'Mantenimiento rutinario'
    FROM @Clientes WHERE etiqueta = 'ALJ';

    INSERT INTO ops.CITA (piscina_id, tecnico_id, fecha_hora, tipo, estado, notas)
    SELECT piscina_id, @Tecnico2, CAST(DATEADD(DAY, 1, CAST(GETDATE() AS DATE)) AS DATETIME) + '11:00', 'Mantenimiento', 'Confirmada', 'Mantenimiento rutinario'
    FROM @Clientes WHERE etiqueta = 'HER';

    INSERT INTO ops.CITA (piscina_id, tecnico_id, fecha_hora, tipo, estado, notas)
    SELECT piscina_id, @Tecnico2, CAST(DATEADD(DAY, 1, CAST(GETDATE() AS DATE)) AS DATETIME) + '14:00', 'Inspección', 'Confirmada', 'Inspección de rutina'
    FROM @Clientes WHERE etiqueta = 'CTG';

    -- Una cita del cliente CTG hace dos semanas, ya completada, con su
    -- servicio cerrado y tareas hechas, para Servicios Técnicos / historial /
    -- y para poder generar la cotización + factura del punto 4.
    INSERT INTO ops.CITA (piscina_id, tecnico_id, fecha_hora, tipo, estado, notas)
    SELECT piscina_id, @Tecnico2, DATEADD(DAY, -14, GETDATE()), 'Mantenimiento', 'Completada', 'Mantenimiento rutinario'
    FROM @Clientes WHERE etiqueta = 'CTG';
    SET @cita = SCOPE_IDENTITY();

    INSERT INTO ops.SERVICIO (cita_id, fecha_apertura, fecha_cierre, estado, trabajo_realizado)
    VALUES (@cita, CAST(DATEADD(DAY, -14, GETDATE()) AS DATE), CAST(DATEADD(DAY, -14, GETDATE()) AS DATE),
            'Cerrado', 'Limpieza general, ajuste de químicos y revisión del sistema de filtración.');
    SET @serv = SCOPE_IDENTITY();

    INSERT INTO ops.TAREA_SERVICIO (servicio_id, descripcion, estado, fecha_asignacion, fecha_completacion)
    VALUES (@serv, 'Limpieza de filtros', 'Completada', CAST(DATEADD(DAY, -14, GETDATE()) AS DATE), CAST(DATEADD(DAY, -14, GETDATE()) AS DATE));
    INSERT INTO ops.TAREA_SERVICIO (servicio_id, descripcion, estado, fecha_asignacion, fecha_completacion)
    VALUES (@serv, 'Ajuste de cloro y pH', 'Completada', CAST(DATEADD(DAY, -14, GETDATE()) AS DATE), CAST(DATEADD(DAY, -14, GETDATE()) AS DATE));

    ---------------------------------------------------------------------------
    -- 4. Cotización Aceptada + Factura Pagada para el cliente CTG, solo si ya
    --    hay productos en el catálogo (no se inventan productos de la nada).
    ---------------------------------------------------------------------------
    IF EXISTS (SELECT 1 FROM inv.PRODUCTO) AND @Admin IS NOT NULL
    BEGIN
        DECLARE @ClienteCTG INT = (SELECT cliente_id FROM @Clientes WHERE etiqueta = 'CTG');
        DECLARE @Prod1 INT, @Prod1Precio DECIMAL(10,2);
        SELECT TOP 1 @Prod1 = id, @Prod1Precio = precio FROM inv.PRODUCTO ORDER BY id;

        DECLARE @Cant DECIMAL(10,2) = 2;
        DECLARE @Subtotal DECIMAL(12,2) = @Prod1Precio * @Cant;
        DECLARE @Impuesto DECIMAL(12,2) = ROUND(@Subtotal * 0.13, 2);
        DECLARE @Total DECIMAL(12,2) = @Subtotal + @Impuesto;

        DECLARE @cot INT;
        INSERT INTO ven.COTIZACION (cliente_id, fecha_emision, fecha_vigencia, subtotal, descuento_total, impuesto_total, total, estado)
        VALUES (@ClienteCTG, CAST(GETDATE() AS DATE), CAST(DATEADD(DAY, 15, GETDATE()) AS DATE), @Subtotal, 0, @Impuesto, @Total, 'Aceptada');
        SET @cot = SCOPE_IDENTITY();

        INSERT INTO ven.DETALLE_COTIZACION (cotizacion_id, producto_id, cantidad_propuesta, precio_unitario, descuento, impuesto, linea_subtotal, linea_total)
        VALUES (@cot, @Prod1, @Cant, @Prod1Precio, 0, @Impuesto, @Subtotal, @Total);

        DECLARE @fac INT;
        INSERT INTO ven.FACTURA (cliente_id, cotizacion_id, creado_por, numero_consecutivo, fecha_emision, fecha_vencimiento, condicion_pago, subtotal, descuento_total, impuesto_total, total, estado)
        VALUES (@ClienteCTG, @cot, @Admin, CONCAT('TEMP-', NEWID()), CAST(GETDATE() AS DATE), CAST(DATEADD(DAY, 8, GETDATE()) AS DATE), 'Contado (SINPE)', @Subtotal, 0, @Impuesto, @Total, 'Pagada');
        SET @fac = SCOPE_IDENTITY();
        UPDATE ven.FACTURA SET numero_consecutivo = CONCAT('FAC-', RIGHT('00000' + CAST(@fac AS VARCHAR(5)), 5)) WHERE id = @fac;

        INSERT INTO ven.DETALLE_FACTURA (factura_id, producto_id, cantidad_vendida, precio_unitario_final, descuento, impuesto, linea_subtotal, linea_total)
        VALUES (@fac, @Prod1, @Cant, @Prod1Precio, 0, @Impuesto, @Subtotal, @Total);

        PRINT 'Cotización y factura de ejemplo generadas para el cliente de Cartago.';
    END
    ELSE
    BEGIN
        PRINT 'No hay productos en inv.PRODUCTO (o no hay administrador activo): se omite la cotización/factura de ejemplo.';
    END

    ---------------------------------------------------------------------------
    PRINT 'Listo: 7 clientes nuevos, con dirección y coordenadas, piscina, y citas de prueba.';
    PRINT CONCAT('Técnico usado para HOY (San José): usuario_id = ', @Tecnico1);
    PRINT CONCAT('Técnico usado para MAÑANA (Alajuela/Heredia/Cartago): usuario_id = ', @Tecnico2);
    PRINT 'Contraseña de los clientes semilla: Cliente123';

    COMMIT TRANSACTION;

END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    PRINT 'Ocurrió un error, no se aplicó ningún cambio (rollback automático):';
    PRINT ERROR_MESSAGE();
    THROW;
END CATCH
