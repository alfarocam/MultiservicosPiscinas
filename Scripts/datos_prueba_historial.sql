-- =============================================================================
-- DATOS DE PRUEBA: Historial de Mantenimientos de Piscinas
-- Base de datos: Piscinas_Y_Multiservicios
-- Ejecutar como un solo batch (sin GO intermedios)
-- NOTA: Los datos ya fueron insertados. Este script es para referencia o
--       para un ambiente de pruebas limpio. Verificar IDs antes de correr.
-- =============================================================================

DECLARE @usuario_id    INT,
        @cliente_id    INT,
        @direccion_id  INT,
        @piscina_id    INT,
        @tecnico_id    INT = 2,   -- Carlos Mora (Técnico existente)
        @cita1_id      INT,
        @cita2_id      INT,
        @cita3_id      INT,
        @cita4_id      INT,
        @serv1_id      INT,
        @serv2_id      INT,
        @serv3_id      INT,
        @serv4_id      INT;

-- ─────────────────────────────────────────────────────────────────────────────
-- 1. Nuevo usuario cliente
-- ─────────────────────────────────────────────────────────────────────────────
INSERT INTO seg.USUARIO
    (rol_id, nombre, apellido_paterno, apellido_materno, correo, contrasena, activo, fecha_creacion)
VALUES
    (3, 'Laura', 'Vega', 'Quiros', 'laura.vega@test.com', 'Test1234', 1, GETDATE());
SET @usuario_id = SCOPE_IDENTITY();

-- ─────────────────────────────────────────────────────────────────────────────
-- 2. Registro de cliente
-- ─────────────────────────────────────────────────────────────────────────────
INSERT INTO cli.CLIENTE (usuario_id, notas)
VALUES (@usuario_id, 'Cliente de prueba para historial de mantenimientos');
SET @cliente_id = SCOPE_IDENTITY();

-- ─────────────────────────────────────────────────────────────────────────────
-- 3. Dirección del cliente (Distrito 10101 - Carmen)
-- ─────────────────────────────────────────────────────────────────────────────
INSERT INTO cli.DIRECCION_CLIENTE
    (cliente_id, distrito_id, tipo_direccion, detalles, codigo_postal, es_principal)
VALUES
    (@cliente_id, 10101, 'Casa', 'Calle Los Robles 45, Residencial Las Palmas', 10101, 1);
SET @direccion_id = SCOPE_IDENTITY();

-- ─────────────────────────────────────────────────────────────────────────────
-- 4. Piscina del cliente
-- ─────────────────────────────────────────────────────────────────────────────
INSERT INTO act.PISCINA (cliente_id, direccion_id, tipo, volumen_m3, estado)
VALUES (@cliente_id, @direccion_id, 'Residencial', 65.00, 'Activa');
SET @piscina_id = SCOPE_IDENTITY();

-- ─────────────────────────────────────────────────────────────────────────────
-- 5. CITA 1 — Mantenimiento (hace 3 meses) — COMPLETADA
-- ─────────────────────────────────────────────────────────────────────────────
INSERT INTO ops.CITA (piscina_id, tecnico_id, fecha_hora, tipo, estado, notas)
VALUES (@piscina_id, @tecnico_id,
        DATEADD(MONTH, -3, GETDATE()),
        'Mantenimiento', 'Completada', 'Mantenimiento rutinario trimestral');
SET @cita1_id = SCOPE_IDENTITY();

INSERT INTO ops.SERVICIO (cita_id, fecha_apertura, fecha_cierre, estado, trabajo_realizado)
VALUES (@cita1_id,
        CAST(DATEADD(MONTH, -3, GETDATE()) AS DATE),
        CAST(DATEADD(MONTH, -3, GETDATE()) AS DATE),
        'Cerrado',
        'Limpieza general de la piscina, ajuste de quimicos y revision del sistema de filtracion.');
SET @serv1_id = SCOPE_IDENTITY();

INSERT INTO ops.INSPECCION
    (servicio_id, fecha_inspeccion, cloro_ppm, alcalinidad, ph, calcio, acido_cianurico, observaciones)
VALUES (@serv1_id,
        CAST(DATEADD(MONTH, -3, GETDATE()) AS DATE),
        1.5, 110, 7.4, 250, 40,
        'Niveles dentro de los rangos normales. Agua en buen estado.');

INSERT INTO ops.TAREA_SERVICIO (servicio_id, descripcion, estado, fecha_asignacion, fecha_completacion) VALUES (@serv1_id, 'Limpieza de filtros',        'Completada', CAST(DATEADD(MONTH,-3,GETDATE()) AS DATE), CAST(DATEADD(MONTH,-3,GETDATE()) AS DATE));
INSERT INTO ops.TAREA_SERVICIO (servicio_id, descripcion, estado, fecha_asignacion, fecha_completacion) VALUES (@serv1_id, 'Ajuste de cloro y pH',        'Completada', CAST(DATEADD(MONTH,-3,GETDATE()) AS DATE), CAST(DATEADD(MONTH,-3,GETDATE()) AS DATE));
INSERT INTO ops.TAREA_SERVICIO (servicio_id, descripcion, estado, fecha_asignacion, fecha_completacion) VALUES (@serv1_id, 'Limpieza de paredes y fondo', 'Completada', CAST(DATEADD(MONTH,-3,GETDATE()) AS DATE), CAST(DATEADD(MONTH,-3,GETDATE()) AS DATE));

-- ─────────────────────────────────────────────────────────────────────────────
-- 6. CITA 2 — Inspección (hace 2 meses) — COMPLETADA
-- ─────────────────────────────────────────────────────────────────────────────
INSERT INTO ops.CITA (piscina_id, tecnico_id, fecha_hora, tipo, estado, notas)
VALUES (@piscina_id, @tecnico_id,
        DATEADD(MONTH, -2, GETDATE()),
        'Inspección', 'Completada', 'Inspeccion mensual de rutina');
SET @cita2_id = SCOPE_IDENTITY();

INSERT INTO ops.SERVICIO (cita_id, fecha_apertura, fecha_cierre, estado, trabajo_realizado)
VALUES (@cita2_id,
        CAST(DATEADD(MONTH, -2, GETDATE()) AS DATE),
        CAST(DATEADD(MONTH, -2, GETDATE()) AS DATE),
        'Cerrado',
        'Inspeccion de parametros quimicos y revision visual de la estructura.');
SET @serv2_id = SCOPE_IDENTITY();

INSERT INTO ops.INSPECCION
    (servicio_id, fecha_inspeccion, cloro_ppm, alcalinidad, ph, calcio, acido_cianurico, observaciones)
VALUES (@serv2_id,
        CAST(DATEADD(MONTH, -2, GETDATE()) AS DATE),
        2.1, 105, 7.6, 245, 45,
        'pH levemente alto. Se recomienda ajustar en proxima visita.');

INSERT INTO ops.TAREA_SERVICIO (servicio_id, descripcion, estado, fecha_asignacion, fecha_completacion) VALUES (@serv2_id, 'Medicion de parametros quimicos', 'Completada', CAST(DATEADD(MONTH,-2,GETDATE()) AS DATE), CAST(DATEADD(MONTH,-2,GETDATE()) AS DATE));
INSERT INTO ops.TAREA_SERVICIO (servicio_id, descripcion, estado, fecha_asignacion, fecha_completacion) VALUES (@serv2_id, 'Revision visual de estructura',   'Completada', CAST(DATEADD(MONTH,-2,GETDATE()) AS DATE), CAST(DATEADD(MONTH,-2,GETDATE()) AS DATE));

-- ─────────────────────────────────────────────────────────────────────────────
-- 7. CITA 3 — Reparación (hace 1 mes) — COMPLETADA
-- ─────────────────────────────────────────────────────────────────────────────
INSERT INTO ops.CITA (piscina_id, tecnico_id, fecha_hora, tipo, estado, notas)
VALUES (@piscina_id, @tecnico_id,
        DATEADD(MONTH, -1, GETDATE()),
        'Reparación', 'Completada', 'Reparacion de bomba de circulacion');
SET @cita3_id = SCOPE_IDENTITY();

INSERT INTO ops.SERVICIO (cita_id, fecha_apertura, fecha_cierre, estado, trabajo_realizado)
VALUES (@cita3_id,
        CAST(DATEADD(MONTH, -1, GETDATE()) AS DATE),
        CAST(DATEADD(MONTH, -1, GETDATE()) AS DATE),
        'Cerrado',
        'Reparacion y reemplazo de sello mecanico de la bomba. Revision general del sistema hidraulico.');
SET @serv3_id = SCOPE_IDENTITY();

INSERT INTO ops.INSPECCION
    (servicio_id, fecha_inspeccion, cloro_ppm, alcalinidad, ph, calcio, acido_cianurico, observaciones)
VALUES (@serv3_id,
        CAST(DATEADD(MONTH, -1, GETDATE()) AS DATE),
        1.8, 120, 7.3, 260, 38,
        'Parametros normales post-reparacion. Sistema en buen estado.');

INSERT INTO ops.TAREA_SERVICIO (servicio_id, descripcion, estado, fecha_asignacion, fecha_completacion) VALUES (@serv3_id, 'Desmontaje de bomba de circulacion',    'Completada', CAST(DATEADD(MONTH,-1,GETDATE()) AS DATE), CAST(DATEADD(MONTH,-1,GETDATE()) AS DATE));
INSERT INTO ops.TAREA_SERVICIO (servicio_id, descripcion, estado, fecha_asignacion, fecha_completacion) VALUES (@serv3_id, 'Reemplazo de sello mecanico',           'Completada', CAST(DATEADD(MONTH,-1,GETDATE()) AS DATE), CAST(DATEADD(MONTH,-1,GETDATE()) AS DATE));
INSERT INTO ops.TAREA_SERVICIO (servicio_id, descripcion, estado, fecha_asignacion, fecha_completacion) VALUES (@serv3_id, 'Prueba de funcionamiento del sistema',   'Completada', CAST(DATEADD(MONTH,-1,GETDATE()) AS DATE), CAST(DATEADD(MONTH,-1,GETDATE()) AS DATE));

-- ─────────────────────────────────────────────────────────────────────────────
-- 8. CITA 4 — Mantenimiento (hoy) — EN PROGRESO
-- ─────────────────────────────────────────────────────────────────────────────
INSERT INTO ops.CITA (piscina_id, tecnico_id, fecha_hora, tipo, estado, notas)
VALUES (@piscina_id, @tecnico_id,
        GETDATE(),
        'Mantenimiento', 'En progreso', 'Mantenimiento preventivo trimestral');
SET @cita4_id = SCOPE_IDENTITY();

INSERT INTO ops.SERVICIO (cita_id, fecha_apertura, fecha_cierre, estado, trabajo_realizado)
VALUES (@cita4_id,
        CAST(GETDATE() AS DATE),
        NULL,
        'En progreso',
        'Mantenimiento preventivo completo en curso: limpieza, quimicos y revision de equipos.');
SET @serv4_id = SCOPE_IDENTITY();

INSERT INTO ops.TAREA_SERVICIO (servicio_id, descripcion, estado, fecha_asignacion, fecha_completacion) VALUES (@serv4_id, 'Limpieza de filtros',               'Completada', CAST(GETDATE() AS DATE), CAST(GETDATE() AS DATE));
INSERT INTO ops.TAREA_SERVICIO (servicio_id, descripcion, estado, fecha_asignacion, fecha_completacion) VALUES (@serv4_id, 'Ajuste de niveles quimicos',        'En progreso', CAST(GETDATE() AS DATE), NULL);
INSERT INTO ops.TAREA_SERVICIO (servicio_id, descripcion, estado, fecha_asignacion, fecha_completacion) VALUES (@serv4_id, 'Revision de iluminacion subacuatica', 'Pendiente',  CAST(GETDATE() AS DATE), NULL);

-- ─────────────────────────────────────────────────────────────────────────────
PRINT 'Datos de prueba insertados correctamente.';
PRINT CONCAT('usuario_id  = ', @usuario_id);
PRINT CONCAT('cliente_id  = ', @cliente_id);
PRINT CONCAT('piscina_id  = ', @piscina_id);
