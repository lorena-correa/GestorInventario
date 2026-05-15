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
LEFT JOIN tb_proveedores pr ON p.proveedor_id = pr.id
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
LEFT JOIN tb_proveedores pr ON m.proveedor_id = pr.id
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
