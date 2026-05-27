-- ============================================================
-- GestorInventario — Esquema PostgreSQL
-- Proyecto universitario MVP de Control de Inventario
-- ============================================================

-- Crear base de datos (ejecutar por separado)
-- CREATE DATABASE gestor_inventario;

-- Conectarse a la base de datos antes de ejecutar el resto

-- ── Roles ──────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS tb_roles (
    id        SERIAL PRIMARY KEY,
    nombre    VARCHAR(50) NOT NULL UNIQUE,
    descripcion TEXT,
    creado_en TIMESTAMP DEFAULT NOW()
);

INSERT INTO tb_roles (nombre, descripcion) VALUES
    ('Administrador', 'Acceso total al sistema'),
    ('Operador', 'Puede registrar entradas y salidas'),
    ('Consultor', 'Solo lectura')
ON CONFLICT DO NOTHING;

-- ── Usuarios ────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS tb_usuarios (
    id            SERIAL PRIMARY KEY,
    nombre        VARCHAR(100) NOT NULL,
    email         VARCHAR(150) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    rol_id        INT REFERENCES tb_roles(id) ON DELETE SET NULL,
    activo        BOOLEAN DEFAULT TRUE,
    creado_en     TIMESTAMP DEFAULT NOW()
);

-- Password 'admin' hashed (SHA256 or bcrypt in real implementation)
INSERT INTO tb_usuarios (nombre, email, password_hash, rol_id) VALUES
    ('Administrador', 'admin@empresa.com', 'admin', 1)
ON CONFLICT DO NOTHING;

-- ── Proveedores ─────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS tb_proveedores (
    id         SERIAL PRIMARY KEY,
    nombre     VARCHAR(150) NOT NULL,
    telefono   VARCHAR(20),
    correo     VARCHAR(150),
    direccion  TEXT,
    activo     BOOLEAN DEFAULT TRUE,
    creado_en  TIMESTAMP DEFAULT NOW()
);

INSERT INTO tb_proveedores (nombre, telefono, correo, direccion) VALUES
    ('TechSupply S.A.', '555-0101', 'ventas@techsupply.com', 'Calle 10 #25-40'),
    ('Distribuidora Norte', '555-0202', 'info@distnorte.com', 'Av. Principal #100'),
    ('GlobalParts Ltda.', '555-0303', 'pedidos@globalparts.com', 'Zona Industrial Bloque 5')
ON CONFLICT DO NOTHING;

-- ── Productos ───────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS tb_productos (
    id             SERIAL PRIMARY KEY,
    codigo         VARCHAR(50) NOT NULL UNIQUE,
    nombre         VARCHAR(200) NOT NULL,
    descripcion    TEXT,
    precio_compra  DECIMAL(12, 2) DEFAULT 0,
    precio_venta   DECIMAL(12, 2) DEFAULT 0,
    stock_actual   INT DEFAULT 0 CHECK (stock_actual >= 0),
    stock_minimo   INT DEFAULT 0,
    categoria      VARCHAR(100),
    proveedor_id   INT REFERENCES tb_proveedores(id) ON DELETE SET NULL,
    activo         BOOLEAN DEFAULT TRUE,
    creado_en      TIMESTAMP DEFAULT NOW()
);

INSERT INTO tb_productos (codigo, nombre, descripcion, precio_compra, precio_venta, stock_actual, stock_minimo, categoria, proveedor_id) VALUES
    ('PRD-001', 'Monitor Samsung 24"', 'Monitor Full HD 1080p', 350000, 450000, 12, 5, 'Electrónica', 1),
    ('PRD-002', 'Teclado Mecánico', 'Teclado RGB switches blue', 85000, 130000, 3, 5, 'Periféricos', 1),
    ('PRD-003', 'Cable HDMI 2m', 'Cable HDMI 4K 2 metros', 12000, 22000, 45, 10, 'Cables', 2),
    ('PRD-004', 'Mouse Inalámbrico', 'Mouse óptico 2.4GHz', 35000, 58000, 2, 8, 'Periféricos', 1),
    ('PRD-005', 'Hub USB 4 puertos', 'Hub USB 3.0 con alimentación', 28000, 48000, 18, 6, 'Accesorios', 3),
    ('PRD-006', 'Disco SSD 500GB', 'SSD SATA 2.5" 500GB', 180000, 260000, 7, 3, 'Almacenamiento', 2)
ON CONFLICT DO NOTHING;

-- ── Tipos de movimiento ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS tb_tipo_movimiento (
    id       SERIAL PRIMARY KEY,
    nombre   VARCHAR(50) NOT NULL UNIQUE,
    tipo     VARCHAR(10) CHECK (tipo IN ('entrada', 'salida'))
);

INSERT INTO tb_tipo_movimiento (nombre, tipo) VALUES
    ('Compra a proveedor', 'entrada'),
    ('Venta', 'salida'),
    ('Uso interno', 'salida'),
    ('Devolución a proveedor', 'salida'),
    ('Ajuste de inventario', 'entrada'),
    ('Daño/pérdida', 'salida')
ON CONFLICT DO NOTHING;

-- ── Movimientos de inventario ───────────────────────────────────
CREATE TABLE IF NOT EXISTS tb_movimientos_inventario (
    id                 SERIAL PRIMARY KEY,
    producto_id        INT NOT NULL REFERENCES tb_productos(id) ON DELETE CASCADE,
    tipo_movimiento_id INT NOT NULL REFERENCES tb_tipo_movimiento(id),
    cantidad           INT NOT NULL CHECK (cantidad > 0),
    observacion        TEXT,
    usuario_id         INT REFERENCES tb_usuarios(id) ON DELETE SET NULL,
    proveedor_id       INT REFERENCES tb_proveedores(id) ON DELETE SET NULL,
    fecha              TIMESTAMP DEFAULT NOW()
);

-- ── Alertas ────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS tb_alertas (
    id           SERIAL PRIMARY KEY,
    producto_id  INT NOT NULL REFERENCES tb_productos(id) ON DELETE CASCADE,
    estado       VARCHAR(20) DEFAULT 'Activa' CHECK (estado IN ('Activa', 'Resuelta', 'Ignorada')),
    creado_en    TIMESTAMP DEFAULT NOW(),
    resuelto_en  TIMESTAMP
);

-- ── Vistas útiles ──────────────────────────────────────────────

-- Vista: Estado actual del inventario
CREATE OR REPLACE VIEW vw_inventario_actual AS
SELECT
    p.id,
    p.codigo,
    p.nombre,
    p.categoria,
    pr.nombre AS proveedor,
    p.stock_actual,
    p.stock_minimo,
    p.precio_venta,
    CASE WHEN p.stock_actual <= p.stock_minimo THEN 'Crítico' ELSE 'Normal' END AS estado_stock,
    p.activo
FROM tb_productos p
LEFT JOIN tb_proveedores pr ON p.Proveedor_id = pr.id
ORDER BY estado_stock DESC, p.nombre;

-- Vista: Historial de movimientos completo
CREATE OR REPLACE VIEW vw_historial_movimientos AS
SELECT
    m.id,
    m.fecha,
    p.nombre AS producto,
    p.codigo,
    tm.nombre AS tipo_movimiento,
    tm.tipo,
    m.cantidad,
    m.observacion,
    u.nombre AS usuario,
    pr.nombre AS proveedor
FROM tb_movimientos_inventario m
JOIN tb_productos p ON m.producto_id = p.id
JOIN tb_tipo_movimiento tm ON m.tipo_movimiento_id = tm.id
LEFT JOIN tb_usuarios u ON m.usuario_id = u.id
LEFT JOIN tb_proveedores pr ON m.Proveedor_id = pr.id
ORDER BY m.fecha DESC;

-- Vista: Alertas activas
CREATE OR REPLACE VIEW vw_alertas_activas AS
SELECT
    a.id,
    p.nombre AS producto,
    p.codigo,
    p.stock_actual,
    p.stock_minimo,
    (p.stock_minimo - p.stock_actual) AS unidades_faltantes,
    a.estado,
    a.creado_en
FROM tb_alertas a
JOIN tb_productos p ON a.producto_id = p.id
WHERE a.estado = 'Activa'
ORDER BY (p.stock_minimo - p.stock_actual) DESC;

-- ── Trigger: Auto-generar alerta cuando stock cae bajo mínimo ──
CREATE OR REPLACE FUNCTION fn_verificar_stock_minimo()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.stock_actual <= NEW.stock_minimo THEN
        INSERT INTO tb_alertas (producto_id, estado)
        VALUES (NEW.id, 'Activa')
        ON CONFLICT DO NOTHING;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE TRIGGER trg_stock_minimo
AFTER UPDATE OF stock_actual ON tb_productos
FOR EACH ROW
EXECUTE FUNCTION fn_verificar_stock_minimo();


-- ============================================================
-- PROCEDIMIENTOS ALMACENADOS
-- GestorInventario — Sprint 2
-- ============================================================

-- ── SP: Autenticación ───────────────────────────────────────
CREATE OR REPLACE FUNCTION sp_login(
    p_email        VARCHAR,
    p_password     VARCHAR
)
RETURNS TABLE (
    id            INT,
    nombre        VARCHAR,
    email         VARCHAR,
    rol           VARCHAR,
    activo        BOOLEAN
) AS $$
BEGIN
    RETURN QUERY
    SELECT u.id, u.nombre, u.email, r.nombre AS rol, u.activo
    FROM tb_usuarios u
    JOIN tb_roles r ON u.rol_id = r.id
    WHERE u.email         = p_email
      AND u.password_hash = p_password
      AND u.activo        = true;
END;
$$ LANGUAGE plpgsql;

-- ── SP: CRUD Productos ──────────────────────────────────────
CREATE OR REPLACE FUNCTION sp_crear_producto(
    p_codigo       VARCHAR,
    p_nombre       VARCHAR,
    p_descripcion  TEXT,
    p_precio_compra DECIMAL,
    p_precio_venta  DECIMAL,
    p_stock_actual  INT,
    p_stock_minimo  INT,
    p_categoria    VARCHAR,
    p_proveedor_id INT
)
RETURNS INT AS $$
DECLARE
    v_id INT;
BEGIN
    -- Validar código duplicado
    IF EXISTS (SELECT 1 FROM tb_productos WHERE codigo = p_codigo) THEN
        RAISE EXCEPTION 'Ya existe un producto con el código %', p_codigo;
    END IF;
    -- Validar stock no negativo
    IF p_stock_actual < 0 THEN
        RAISE EXCEPTION 'El stock no puede ser negativo';
    END IF;
    -- Validar precios
    IF p_precio_venta <= 0 OR p_precio_compra < 0 THEN
        RAISE EXCEPTION 'Los precios deben ser mayores a cero';
    END IF;

    INSERT INTO tb_productos
        (codigo, nombre, descripcion, precio_compra, precio_venta,
         stock_actual, stock_minimo, categoria, proveedor_id)
    VALUES
        (p_codigo, p_nombre, p_descripcion, p_precio_compra, p_precio_venta,
         p_stock_actual, p_stock_minimo, p_categoria, NULLIF(p_proveedor_id, 0))
    RETURNING id INTO v_id;

    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_actualizar_producto(
    p_id           INT,
    p_nombre       VARCHAR,
    p_descripcion  TEXT,
    p_precio_compra DECIMAL,
    p_precio_venta  DECIMAL,
    p_stock_minimo  INT,
    p_categoria    VARCHAR,
    p_proveedor_id INT
)
RETURNS BOOLEAN AS $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM tb_productos WHERE id = p_id AND activo = true) THEN
        RAISE EXCEPTION 'Producto no encontrado';
    END IF;
    IF p_precio_venta <= 0 THEN
        RAISE EXCEPTION 'El precio de venta debe ser mayor a cero';
    END IF;

    UPDATE tb_productos
    SET nombre        = p_nombre,
        descripcion   = p_descripcion,
        precio_compra = p_precio_compra,
        precio_venta  = p_precio_venta,
        stock_minimo  = p_stock_minimo,
        categoria     = p_categoria,
        proveedor_id  = NULLIF(p_proveedor_id, 0)
    WHERE id = p_id;

    RETURN TRUE;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_eliminar_producto(p_id INT)
RETURNS BOOLEAN AS $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM tb_productos WHERE id = p_id AND activo = true) THEN
        RAISE EXCEPTION 'Producto no encontrado';
    END IF;
    UPDATE tb_productos SET activo = false WHERE id = p_id;
    RETURN TRUE;
END;
$$ LANGUAGE plpgsql;

-- ── SP: CRUD Proveedores ────────────────────────────────────
CREATE OR REPLACE FUNCTION sp_crear_proveedor(
    p_nombre    VARCHAR,
    p_telefono  VARCHAR,
    p_correo    VARCHAR,
    p_direccion TEXT
)
RETURNS INT AS $$
DECLARE
    v_id INT;
BEGIN
    IF p_nombre IS NULL OR TRIM(p_nombre) = '' THEN
        RAISE EXCEPTION 'El nombre del proveedor es obligatorio';
    END IF;

    INSERT INTO tb_proveedores (nombre, telefono, correo, direccion)
    VALUES (p_nombre, p_telefono, p_correo, p_direccion)
    RETURNING id INTO v_id;

    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_actualizar_proveedor(
    p_id        INT,
    p_nombre    VARCHAR,
    p_telefono  VARCHAR,
    p_correo    VARCHAR,
    p_direccion TEXT
)
RETURNS BOOLEAN AS $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM tb_proveedores WHERE id = p_id AND activo = true) THEN
        RAISE EXCEPTION 'Proveedor no encontrado';
    END IF;

    UPDATE tb_proveedores
    SET nombre    = p_nombre,
        telefono  = p_telefono,
        correo    = p_correo,
        direccion = p_direccion
    WHERE id = p_id;

    RETURN TRUE;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_eliminar_proveedor(p_id INT)
RETURNS BOOLEAN AS $$
BEGIN
    UPDATE tb_proveedores SET activo = false WHERE id = p_id;
    RETURN TRUE;
END;
$$ LANGUAGE plpgsql;

-- ── SP: CRUD Usuarios ───────────────────────────────────────
CREATE OR REPLACE FUNCTION sp_crear_usuario(
    p_nombre   VARCHAR,
    p_email    VARCHAR,
    p_password VARCHAR,
    p_rol      VARCHAR
)
RETURNS INT AS $$
DECLARE
    v_id     INT;
    v_rol_id INT;
BEGIN
    IF EXISTS (SELECT 1 FROM tb_usuarios WHERE email = p_email) THEN
        RAISE EXCEPTION 'Ya existe un usuario con el correo %', p_email;
    END IF;

    SELECT id INTO v_rol_id FROM tb_roles WHERE nombre = p_rol;
    IF v_rol_id IS NULL THEN
        RAISE EXCEPTION 'Rol no encontrado: %', p_rol;
    END IF;

    INSERT INTO tb_usuarios (nombre, email, password_hash, rol_id)
    VALUES (p_nombre, p_email, p_password, v_rol_id)
    RETURNING id INTO v_id;

    RETURN v_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_actualizar_usuario(
    p_id     INT,
    p_nombre VARCHAR,
    p_email  VARCHAR,
    p_rol    VARCHAR,
    p_activo BOOLEAN
)
RETURNS BOOLEAN AS $$
DECLARE
    v_rol_id INT;
BEGIN
    SELECT id INTO v_rol_id FROM tb_roles WHERE nombre = p_rol;
    IF v_rol_id IS NULL THEN
        RAISE EXCEPTION 'Rol no encontrado: %', p_rol;
    END IF;

    UPDATE tb_usuarios
    SET nombre = p_nombre,
        email  = p_email,
        rol_id = v_rol_id,
        activo = p_activo
    WHERE id = p_id;

    RETURN TRUE;
END;
$$ LANGUAGE plpgsql;

-- ── SP: Registrar movimiento ────────────────────────────────
CREATE OR REPLACE FUNCTION sp_registrar_movimiento(
    p_producto_id        INT,
    p_tipo_movimiento_id INT,
    p_cantidad           INT,
    p_observacion        TEXT,
    p_usuario_id         INT,
    p_proveedor_id       INT
)
RETURNS INT AS $$
DECLARE
    v_tipo        VARCHAR;
    v_stock       INT;
    v_mov_id      INT;
BEGIN
    -- Validar cantidad
    IF p_cantidad <= 0 THEN
        RAISE EXCEPTION 'La cantidad debe ser mayor a cero';
    END IF;

    -- Obtener tipo de movimiento
    SELECT tipo INTO v_tipo
    FROM tb_tipo_movimiento
    WHERE id = p_tipo_movimiento_id;

    IF v_tipo IS NULL THEN
        RAISE EXCEPTION 'Tipo de movimiento no válido';
    END IF;

    -- Verificar stock si es salida
    IF v_tipo = 'salida' THEN
        SELECT stock_actual INTO v_stock
        FROM tb_productos WHERE id = p_producto_id;

        IF v_stock < p_cantidad THEN
            RAISE EXCEPTION 'Stock insuficiente. Disponible: %, solicitado: %',
                v_stock, p_cantidad;
        END IF;
    END IF;

    -- Insertar movimiento
    INSERT INTO tb_movimientos_inventario
        (producto_id, tipo_movimiento_id, cantidad, observacion, usuario_id, proveedor_id)
    VALUES
        (p_producto_id, p_tipo_movimiento_id, p_cantidad, p_observacion,
         p_usuario_id, NULLIF(p_proveedor_id, 0))
    RETURNING id INTO v_mov_id;

    -- Actualizar stock (el trigger de alertas se dispara automáticamente)
    IF v_tipo = 'entrada' THEN
        UPDATE tb_productos
        SET stock_actual = stock_actual + p_cantidad
        WHERE id = p_producto_id;
    ELSE
        UPDATE tb_productos
        SET stock_actual = stock_actual - p_cantidad
        WHERE id = p_producto_id;
    END IF;

    RETURN v_mov_id;
END;
$$ LANGUAGE plpgsql;

-- ── SP: Reportes ────────────────────────────────────────────
CREATE OR REPLACE FUNCTION sp_reporte_stock_actual()
RETURNS TABLE (
    codigo        VARCHAR,
    nombre        VARCHAR,
    categoria     VARCHAR,
    proveedor     VARCHAR,
    stock_actual  INT,
    stock_minimo  INT,
    precio_venta  DECIMAL,
    valor_total   DECIMAL,
    estado        VARCHAR
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        p.codigo,
        p.nombre,
        p.categoria,
        COALESCE(pr.nombre, 'Sin proveedor')::VARCHAR,
        p.stock_actual,
        p.stock_minimo,
        p.precio_venta,
        (p.stock_actual * p.precio_venta)::DECIMAL AS valor_total,
        CASE WHEN p.stock_actual <= p.stock_minimo THEN 'Crítico' ELSE 'Normal' END::VARCHAR
    FROM tb_productos p
    LEFT JOIN tb_proveedores pr ON p.proveedor_id = pr.id
    WHERE p.activo = true
    ORDER BY estado DESC, p.nombre;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_reporte_movimientos(
    p_desde DATE,
    p_hasta DATE
)
RETURNS TABLE (
    fecha             TIMESTAMP,
    producto          VARCHAR,
    codigo            VARCHAR,
    tipo_movimiento   VARCHAR,
    tipo              VARCHAR,
    cantidad          INT,
    observacion       TEXT,
    usuario           VARCHAR
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        m.fecha,
        p.nombre::VARCHAR,
        p.codigo::VARCHAR,
        tm.nombre::VARCHAR,
        tm.tipo::VARCHAR,
        m.cantidad,
        m.observacion,
        COALESCE(u.nombre, 'Sistema')::VARCHAR
    FROM tb_movimientos_inventario m
    JOIN tb_productos p ON m.producto_id = p.id
    JOIN tb_tipo_movimiento tm ON m.tipo_movimiento_id = tm.id
    LEFT JOIN tb_usuarios u ON m.usuario_id = u.id
    WHERE m.fecha::DATE BETWEEN p_desde AND p_hasta
    ORDER BY m.fecha DESC;
END;
$$ LANGUAGE plpgsql;

-- ── SP: Dashboard stats ─────────────────────────────────────
CREATE OR REPLACE FUNCTION sp_dashboard_stats()
RETURNS TABLE (
    total_productos    BIGINT,
    productos_criticos BIGINT,
    movimientos_hoy    BIGINT,
    alertas_activas    BIGINT,
    total_proveedores  BIGINT,
    valor_inventario   DECIMAL
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        (SELECT COUNT(*) FROM tb_productos WHERE activo = true),
        (SELECT COUNT(*) FROM tb_productos WHERE activo = true AND stock_actual <= stock_minimo),
        (SELECT COUNT(*) FROM tb_movimientos_inventario WHERE fecha::DATE = CURRENT_DATE),
        (SELECT COUNT(*) FROM tb_alertas WHERE estado = 'Activa'),
        (SELECT COUNT(*) FROM tb_proveedores WHERE activo = true),
        (SELECT COALESCE(SUM(stock_actual * precio_venta), 0) FROM tb_productos WHERE activo = true);
END;
$$ LANGUAGE plpgsql;