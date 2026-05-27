using System;
using System.Collections.Generic;
using Npgsql;
using GestorInventario.Config;
using GestorInventario.Models;

namespace GestorInventario.Repositories
{
    // ══════════════════════════════════════════════════════════
    //  BASE REPOSITORY
    // ══════════════════════════════════════════════════════════
    public abstract class BaseRepository
    {
        protected NpgsqlConnection GetConnection() => DatabaseConfig.GetConnection();
    }

    // ══════════════════════════════════════════════════════════
    //  USUARIO REPOSITORY
    // ══════════════════════════════════════════════════════════
    public class UsuarioRepository : BaseRepository
    {
        /// <summary>Valida credenciales y retorna el usuario si existe.</summary>
        public Usuario? GetByCredentials(string email, string passwordHash)
        {
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT u.id, u.nombre, u.email, u.password_hash, u.activo,
                         r.nombre AS rol
                  FROM tb_usuarios u
                  JOIN tb_roles r ON u.rol_id = r.id
                  WHERE u.email = @email
                    AND u.password_hash = @hash
                    AND u.activo = true", conn);

            cmd.Parameters.AddWithValue("email", email);
            cmd.Parameters.AddWithValue("hash", passwordHash);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;

            return new Usuario
            {
                Id = reader.GetInt32(0),
                Nombre = reader.GetString(1),
                Email = reader.GetString(2),
                PasswordHash = reader.GetString(3),
                Activo = reader.GetBoolean(4),
                Rol = reader.GetString(5)
            };
        }

        public List<Usuario> GetAll()
        {
            var lista = new List<Usuario>();
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT u.id, u.nombre, u.email, u.activo, r.nombre AS rol
                  FROM tb_usuarios u
                  JOIN tb_roles r ON u.rol_id = r.id
                  ORDER BY u.nombre", conn);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new Usuario
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    Email = reader.GetString(2),
                    Activo = reader.GetBoolean(3),
                    Rol = reader.GetString(4)
                });
            }
            return lista;
        }

        public bool Create(Usuario u)
        {
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                @"INSERT INTO tb_usuarios (nombre, email, password_hash, rol_id)
                  VALUES (@nombre, @email, @hash,
                          (SELECT id FROM tb_roles WHERE nombre = @rol))", conn);

            cmd.Parameters.AddWithValue("nombre", u.Nombre);
            cmd.Parameters.AddWithValue("email", u.Email);
            cmd.Parameters.AddWithValue("hash", u.PasswordHash);
            cmd.Parameters.AddWithValue("rol", u.Rol);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Update(Usuario u)
        {
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                @"UPDATE tb_usuarios
                  SET nombre = @nombre,
                      email  = @email,
                      activo = @activo,
                      rol_id = (SELECT id FROM tb_roles WHERE nombre = @rol)
                  WHERE id = @id", conn);

            cmd.Parameters.AddWithValue("nombre", u.Nombre);
            cmd.Parameters.AddWithValue("email", u.Email);
            cmd.Parameters.AddWithValue("activo", u.Activo);
            cmd.Parameters.AddWithValue("rol", u.Rol);
            cmd.Parameters.AddWithValue("id", u.Id);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Delete(int id)
        {
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                "UPDATE tb_usuarios SET activo = false WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        public List<Rol> GetRoles()
        {
            var lista = new List<Rol>();
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                "SELECT id, nombre FROM tb_roles ORDER BY nombre", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                lista.Add(new Rol { Id = reader.GetInt32(0), Nombre = reader.GetString(1) });
            return lista;
        }
    }

    // ══════════════════════════════════════════════════════════
    //  PRODUCTO REPOSITORY
    // ══════════════════════════════════════════════════════════
    public class ProductoRepository : BaseRepository
    {
        public List<Producto> GetAll()
        {
            var lista = new List<Producto>();
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT p.id, p.codigo, p.nombre, p.descripcion,
                         p.precio_compra, p.precio_venta,
                         p.stock_actual, p.stock_minimo,
                         p.categoria, p.activo,
                         COALESCE(pr.nombre,'') AS proveedor,
                         COALESCE(p.Proveedor_id, 0) AS proveedor_id
                  FROM tb_productos p
                  LEFT JOIN tb_proveedores pr ON p.Proveedor_id = pr.id
                  WHERE p.activo = true
                  ORDER BY p.nombre", conn);

            using var reader = cmd.ExecuteReader();
            while (reader.Read()) lista.Add(MapProducto(reader));
            return lista;
        }

        public Producto? GetById(int id)
        {
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT p.id, p.codigo, p.nombre, p.descripcion,
                         p.precio_compra, p.precio_venta,
                         p.stock_actual, p.stock_minimo,
                         p.categoria, p.activo,
                         COALESCE(pr.nombre,'') AS proveedor,
                         COALESCE(p.Proveedor_id, 0) AS proveedor_id
                  FROM tb_productos p
                  LEFT JOIN tb_proveedores pr ON p.Proveedor_id = pr.id
                  WHERE p.id = @id", conn);
            cmd.Parameters.AddWithValue("id", id);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapProducto(reader) : null;
        }

        public Producto? GetByCodigo(string codigo)
        {
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT p.id, p.codigo, p.nombre, p.descripcion,
                         p.precio_compra, p.precio_venta,
                         p.stock_actual, p.stock_minimo,
                         p.categoria, p.activo,
                         COALESCE(pr.nombre,'') AS proveedor,
                         COALESCE(p.Proveedor_id, 0) AS proveedor_id
                  FROM tb_productos p
                  LEFT JOIN tb_proveedores pr ON p.Proveedor_id = pr.id
                  WHERE p.codigo = @codigo", conn);
            cmd.Parameters.AddWithValue("codigo", codigo);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapProducto(reader) : null;
        }

        /// <summary>Búsqueda por nombre, código o categoría.</summary>
        public List<Producto> Search(string filtro)
        {
            var lista = new List<Producto>();
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT p.id, p.codigo, p.nombre, p.descripcion,
                         p.precio_compra, p.precio_venta,
                         p.stock_actual, p.stock_minimo,
                         p.categoria, p.activo,
                         COALESCE(pr.nombre,'') AS proveedor,
                         COALESCE(p.Proveedor_id, 0) AS proveedor_id
                  FROM tb_productos p
                  LEFT JOIN tb_proveedores pr ON p.Proveedor_id = pr.id
                  WHERE p.activo = true
                    AND (p.nombre    ILIKE @f
                      OR p.codigo   ILIKE @f
                      OR p.categoria ILIKE @f)
                  ORDER BY p.nombre", conn);
            cmd.Parameters.AddWithValue("f", $"%{filtro}%");
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) lista.Add(MapProducto(reader));
            return lista;
        }

        public bool Create(Producto p)
        {
            // Validar código duplicado
            if (GetByCodigo(p.Codigo) != null)
                throw new InvalidOperationException($"Ya existe un producto con el código '{p.Codigo}'.");

            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                @"INSERT INTO tb_productos
                    (codigo, nombre, descripcion, precio_compra, precio_venta,
                     stock_actual, stock_minimo, categoria, proveedor_id)
                  VALUES
                    (@codigo, @nombre, @desc, @pcompra, @pventa,
                     @stock, @minimo, @cat,
                     NULLIF(@provId, 0))", conn);

            cmd.Parameters.AddWithValue("codigo", p.Codigo);
            cmd.Parameters.AddWithValue("nombre", p.Nombre);
            cmd.Parameters.AddWithValue("desc", p.Descripcion ?? "");
            cmd.Parameters.AddWithValue("pcompra", p.PrecioCompra);
            cmd.Parameters.AddWithValue("pventa", p.PrecioVenta);
            cmd.Parameters.AddWithValue("stock", p.StockActual);
            cmd.Parameters.AddWithValue("minimo", p.StockMinimo);
            cmd.Parameters.AddWithValue("cat", p.Categoria ?? "");
            cmd.Parameters.AddWithValue("provId", p.ProveedorId);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Update(Producto p)
        {
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                @"UPDATE tb_productos
                  SET nombre        = @nombre,
                      descripcion   = @desc,
                      precio_compra = @pcompra,
                      precio_venta  = @pventa,
                      stock_minimo  = @minimo,
                      categoria     = @cat,
                      proveedor_id  = NULLIF(@provId, 0)
                  WHERE id = @id", conn);

            cmd.Parameters.AddWithValue("nombre", p.Nombre);
            cmd.Parameters.AddWithValue("desc", p.Descripcion ?? "");
            cmd.Parameters.AddWithValue("pcompra", p.PrecioCompra);
            cmd.Parameters.AddWithValue("pventa", p.PrecioVenta);
            cmd.Parameters.AddWithValue("minimo", p.StockMinimo);
            cmd.Parameters.AddWithValue("cat", p.Categoria ?? "");
            cmd.Parameters.AddWithValue("provId", p.ProveedorId);
            cmd.Parameters.AddWithValue("id", p.Id);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Delete(int id)
        {
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                "UPDATE tb_productos SET activo = false WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        public List<Producto> GetCriticos()
        {
            var lista = new List<Producto>();
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT p.id, p.codigo, p.nombre, p.descripcion,
                         p.precio_compra, p.precio_venta,
                         p.stock_actual, p.stock_minimo,
                         p.categoria, p.activo,
                         COALESCE(pr.nombre,'') AS proveedor,
                         COALESCE(p.Proveedor_id, 0) AS proveedor_id
                  FROM tb_productos p
                  LEFT JOIN tb_proveedores pr ON p.Proveedor_id = pr.id
                  WHERE p.activo = true
                    AND p.stock_actual <= p.stock_minimo
                  ORDER BY (p.stock_actual - p.stock_minimo)", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) lista.Add(MapProducto(reader));
            return lista;
        }

        private static Producto MapProducto(NpgsqlDataReader r) => new()
        {
            Id = r.GetInt32(0),
            Codigo = r.GetString(1),
            Nombre = r.GetString(2),
            Descripcion = r.IsDBNull(3) ? "" : r.GetString(3),
            PrecioCompra = r.GetDecimal(4),
            PrecioVenta = r.GetDecimal(5),
            StockActual = r.GetInt32(6),
            StockMinimo = r.GetInt32(7),
            Categoria = r.IsDBNull(8) ? "" : r.GetString(8),
            Activo = r.GetBoolean(9),
            Proveedor = r.GetString(10),
            ProveedorId = r.GetInt32(11)
        };
    }

    // ══════════════════════════════════════════════════════════
    //  PROVEEDOR REPOSITORY
    // ══════════════════════════════════════════════════════════
    public class ProveedorRepository : BaseRepository
    {
        public List<Proveedor> GetAll()
        {
            var lista = new List<Proveedor>();
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT id, nombre, telefono, correo, direccion, activo
                  FROM tb_proveedores
                  WHERE activo = true
                  ORDER BY nombre", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) lista.Add(MapProveedor(reader));
            return lista;
        }

        public List<Proveedor> Search(string filtro)
        {
            var lista = new List<Proveedor>();
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT id, nombre, telefono, correo, direccion, activo
                  FROM tb_proveedores
                  WHERE activo = true
                    AND (nombre  ILIKE @f
                      OR correo  ILIKE @f
                      OR telefono ILIKE @f)
                  ORDER BY nombre", conn);
            cmd.Parameters.AddWithValue("f", $"%{filtro}%");
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) lista.Add(MapProveedor(reader));
            return lista;
        }

        public bool Create(Proveedor p)
        {
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                @"INSERT INTO tb_proveedores (nombre, telefono, correo, direccion)
                  VALUES (@nombre, @tel, @correo, @dir)", conn);
            cmd.Parameters.AddWithValue("nombre", p.Nombre);
            cmd.Parameters.AddWithValue("tel", p.Telefono ?? "");
            cmd.Parameters.AddWithValue("correo", p.Correo ?? "");
            cmd.Parameters.AddWithValue("dir", p.Direccion ?? "");
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Update(Proveedor p)
        {
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                @"UPDATE tb_proveedores
                  SET nombre    = @nombre,
                      telefono  = @tel,
                      correo    = @correo,
                      direccion = @dir
                  WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("nombre", p.Nombre);
            cmd.Parameters.AddWithValue("tel", p.Telefono ?? "");
            cmd.Parameters.AddWithValue("correo", p.Correo ?? "");
            cmd.Parameters.AddWithValue("dir", p.Direccion ?? "");
            cmd.Parameters.AddWithValue("id", p.Id);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Delete(int id)
        {
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                "UPDATE tb_proveedores SET activo = false WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        private static Proveedor MapProveedor(NpgsqlDataReader r) => new()
        {
            Id = r.GetInt32(0),
            Nombre = r.GetString(1),
            Telefono = r.IsDBNull(2) ? "" : r.GetString(2),
            Correo = r.IsDBNull(3) ? "" : r.GetString(3),
            Direccion = r.IsDBNull(4) ? "" : r.GetString(4),
            Activo = r.GetBoolean(5)
        };
    }

    // ══════════════════════════════════════════════════════════
    //  MOVIMIENTO REPOSITORY
    // ══════════════════════════════════════════════════════════
    public class MovimientoRepository : BaseRepository
    {
        /// <summary>
        /// Registra un movimiento y actualiza el stock en una sola transacción.
        /// Lanza excepción si la salida dejaría stock negativo.
        /// </summary>
        public bool RegistrarMovimiento(Movimiento m)
        {
            using var conn = GetConnection();
            using var tx = conn.BeginTransaction();
            try
            {
                // 1. Obtener tipo de movimiento
                using var cmdTipo = new NpgsqlCommand(
                    "SELECT tipo FROM tb_tipo_movimiento WHERE id = @id", conn, tx);
                cmdTipo.Parameters.AddWithValue("id", m.TipoMovimientoId);
                string tipo = (string)cmdTipo.ExecuteScalar()!;

                // 2. Verificar stock si es salida
                if (tipo == "salida")
                {
                    using var cmdStock = new NpgsqlCommand(
                        "SELECT stock_actual FROM tb_productos WHERE id = @id", conn, tx);
                    cmdStock.Parameters.AddWithValue("id", m.ProductoId);
                    int stockActual = (int)cmdStock.ExecuteScalar()!;

                    if (stockActual < m.Cantidad)
                        throw new InvalidOperationException(
                            $"Stock insuficiente. Disponible: {stockActual}, solicitado: {m.Cantidad}.");
                }

                // 3. Registrar movimiento
                using var cmdMov = new NpgsqlCommand(
                    @"INSERT INTO tb_movimientos_inventario
                        (producto_id, tipo_movimiento_id, cantidad, observacion, usuario_id, proveedor_id)
                      VALUES (@prod, @tipo, @cant, @obs, @usr, NULLIF(@prov, 0))", conn, tx);
                cmdMov.Parameters.AddWithValue("prod", m.ProductoId);
                cmdMov.Parameters.AddWithValue("tipo", m.TipoMovimientoId);
                cmdMov.Parameters.AddWithValue("cant", m.Cantidad);
                cmdMov.Parameters.AddWithValue("obs", m.Observacion ?? "");
                cmdMov.Parameters.AddWithValue("usr", m.UsuarioId);
                cmdMov.Parameters.AddWithValue("prov", m.ProveedorId);
                cmdMov.ExecuteNonQuery();

                // 4. Actualizar stock (el trigger de alertas se dispara automáticamente)
                string operacion = tipo == "entrada" ? "+" : "-";
                using var cmdUpd = new NpgsqlCommand(
                    $"UPDATE tb_productos SET stock_actual = stock_actual {operacion} @cant WHERE id = @id",
                    conn, tx);
                cmdUpd.Parameters.AddWithValue("cant", m.Cantidad);
                cmdUpd.Parameters.AddWithValue("id", m.ProductoId);
                cmdUpd.ExecuteNonQuery();

                tx.Commit();
                return true;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public List<Movimiento> GetHistorial(
            int? productoId = null,
            int? tipoId = null,
            DateTime? desde = null,
            DateTime? hasta = null)
        {
            var lista = new List<Movimiento>();
            using var conn = GetConnection();

            var sql = @"SELECT m.id, m.fecha, p.nombre AS producto, p.codigo,
                               tm.nombre AS tipo_movimiento, tm.tipo,
                               m.cantidad, m.observacion,
                               COALESCE(u.nombre,'') AS usuario,
                               COALESCE(pr.nombre,'') AS proveedor,
                               m.producto_id, m.tipo_movimiento_id
                        FROM tb_movimientos_inventario m
                        JOIN tb_productos p ON m.producto_id = p.id
                        JOIN tb_tipo_movimiento tm ON m.tipo_movimiento_id = tm.id
                        LEFT JOIN tb_usuarios u ON m.usuario_id = u.id
                        LEFT JOIN tb_proveedores pr ON m.Proveedor_id = pr.id
                        WHERE 1=1";

            if (productoId.HasValue) sql += " AND m.producto_id = @prod";
            if (tipoId.HasValue) sql += " AND m.tipo_movimiento_id = @tipo";
            if (desde.HasValue) sql += " AND m.fecha >= @desde";
            if (hasta.HasValue) sql += " AND m.fecha <= @hasta";
            sql += " ORDER BY m.fecha DESC";

            using var cmd = new NpgsqlCommand(sql, conn);
            if (productoId.HasValue) cmd.Parameters.AddWithValue("prod", productoId.Value);
            if (tipoId.HasValue) cmd.Parameters.AddWithValue("tipo", tipoId.Value);
            if (desde.HasValue) cmd.Parameters.AddWithValue("desde", desde.Value);
            if (hasta.HasValue) cmd.Parameters.AddWithValue("hasta", hasta.Value);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new Movimiento
                {
                    Id = reader.GetInt32(0),
                    Fecha = reader.GetDateTime(1),
                    NombreProducto = reader.GetString(2),
                    CodigoProducto = reader.GetString(3),
                    TipoMovimiento = reader.GetString(4),
                    TipoEntradaSalida = reader.GetString(5),
                    Cantidad = reader.GetInt32(6),
                    Observacion = reader.IsDBNull(7) ? "" : reader.GetString(7),
                    NombreUsuario = reader.GetString(8),
                    Proveedor = reader.GetString(9),
                    ProductoId = reader.GetInt32(10),
                    TipoMovimientoId = reader.GetInt32(11)
                });
            }
            return lista;
        }

        public List<TipoMovimiento> GetTipos()
        {
            var lista = new List<TipoMovimiento>();
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                "SELECT id, nombre, tipo FROM tb_tipo_movimiento ORDER BY tipo, nombre", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                lista.Add(new TipoMovimiento
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    Tipo = reader.GetString(2)
                });
            return lista;
        }
    }

    // ══════════════════════════════════════════════════════════
    //  ALERTA REPOSITORY
    // ══════════════════════════════════════════════════════════
    public class AlertaRepository : BaseRepository
    {
        public List<Alerta> GetActivas()
        {
            var lista = new List<Alerta>();
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT a.id, p.nombre AS producto, p.codigo,
                         p.stock_actual, p.stock_minimo,
                         (p.stock_minimo - p.stock_actual) AS faltantes,
                         a.estado, a.creado_en
                  FROM tb_alertas a
                  JOIN tb_productos p ON a.producto_id = p.id
                  WHERE a.estado = 'Activa'
                  ORDER BY faltantes DESC", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new Alerta
                {
                    Id = reader.GetInt32(0),
                    NombreProducto = reader.GetString(1),
                    CodigoProducto = reader.GetString(2),
                    StockActual = reader.GetInt32(3),
                    StockMinimo = reader.GetInt32(4),
                    UnidadesFaltantes = reader.GetInt32(5),
                    Estado = reader.GetString(6),
                    CreadoEn = reader.GetDateTime(7)
                });
            }
            return lista;
        }

        public bool Resolver(int alertaId)
        {
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                @"UPDATE tb_alertas
                  SET estado = 'Resuelta', resuelto_en = NOW()
                  WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("id", alertaId);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Ignorar(int alertaId)
        {
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                "UPDATE tb_alertas SET estado = 'Ignorada' WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("id", alertaId);
            return cmd.ExecuteNonQuery() > 0;
        }

        public int ContarActivas()
        {
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                "SELECT COUNT(*) FROM tb_alertas WHERE estado = 'Activa'", conn);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }
}