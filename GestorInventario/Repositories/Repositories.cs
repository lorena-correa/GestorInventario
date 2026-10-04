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
            string sql = @"UPDATE tb_usuarios
                            SET nombre = @nombre,
                                email  = @email,
                                activo = @activo,
                                rol_id = (SELECT id FROM tb_roles WHERE nombre = @rol)";
            if (!string.IsNullOrEmpty(u.PasswordHash))
                sql += ", password_hash = @hash";
            sql += " WHERE id = @id";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("nombre", u.Nombre);
            cmd.Parameters.AddWithValue("email", u.Email);
            cmd.Parameters.AddWithValue("activo", u.Activo);
            cmd.Parameters.AddWithValue("rol", u.Rol);
            if (!string.IsNullOrEmpty(u.PasswordHash))
                cmd.Parameters.AddWithValue("hash", u.PasswordHash);
            cmd.Parameters.AddWithValue("id", u.Id);
            return cmd.ExecuteNonQuery() > 0;
        }

        public Usuario? GetByEmail(string email)
        {
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT u.id, u.nombre, u.email, u.activo, r.nombre AS rol
                  FROM tb_usuarios u
                  JOIN tb_roles r ON u.rol_id = r.id
                  WHERE u.email = @email", conn);
            cmd.Parameters.AddWithValue("email", email);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;

            return new Usuario
            {
                Id = reader.GetInt32(0),
                Nombre = reader.GetString(1),
                Email = reader.GetString(2),
                Activo = reader.GetBoolean(3),
                Rol = reader.GetString(4)
            };
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
    //  CATEGORIA REPOSITORY
    // ══════════════════════════════════════════════════════════
    public class CategoriaRepository : BaseRepository
    {
        private const string BaseSelect =
            @"SELECT c.id, c.nombre, c.descripcion, c.activo,
                     (SELECT COUNT(*) FROM tb_productos p
                      WHERE p.categoria = c.nombre AND p.activo = true) AS total_productos
              FROM tb_categorias c";

        public List<Categoria> GetAll()
        {
            var lista = new List<Categoria>();
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                BaseSelect + " WHERE c.activo = true ORDER BY c.nombre", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) lista.Add(MapCategoria(reader));
            return lista;
        }

        public List<Categoria> Search(string filtro)
        {
            var lista = new List<Categoria>();
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                BaseSelect + @" WHERE c.activo = true
                                AND (c.nombre ILIKE @f OR c.descripcion ILIKE @f)
                                ORDER BY c.nombre", conn);
            cmd.Parameters.AddWithValue("f", $"%{filtro}%");
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) lista.Add(MapCategoria(reader));
            return lista;
        }

        public bool Create(Categoria c)
        {
            if (ExisteNombre(c.Nombre, excluirId: null))
                throw new InvalidOperationException($"Ya existe una categoría con el nombre '{c.Nombre}'.");

            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                @"INSERT INTO tb_categorias (nombre, descripcion)
                  VALUES (@nombre, @desc)", conn);
            cmd.Parameters.AddWithValue("nombre", c.Nombre);
            cmd.Parameters.AddWithValue("desc", c.Descripcion ?? "");
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Update(Categoria c)
        {
            if (ExisteNombre(c.Nombre, excluirId: c.Id))
                throw new InvalidOperationException($"Ya existe otra categoría con el nombre '{c.Nombre}'.");

            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                @"UPDATE tb_categorias
                  SET nombre      = @nombre,
                      descripcion = @desc
                  WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("nombre", c.Nombre);
            cmd.Parameters.AddWithValue("desc", c.Descripcion ?? "");
            cmd.Parameters.AddWithValue("id", c.Id);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Delete(int id)
        {
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                "UPDATE tb_categorias SET activo = false WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        private bool ExisteNombre(string nombre, int? excluirId)
        {
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT COUNT(*) FROM tb_categorias
                  WHERE nombre = @nombre AND (@excluirId::int IS NULL OR id <> @excluirId)", conn);
            cmd.Parameters.AddWithValue("nombre", nombre);
            cmd.Parameters.AddWithValue("excluirId", (object?)excluirId ?? DBNull.Value);
            return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
        }

        private static Categoria MapCategoria(NpgsqlDataReader r) => new()
        {
            Id = r.GetInt32(0),
            Nombre = r.GetString(1),
            Descripcion = r.IsDBNull(2) ? "" : r.GetString(2),
            Activo = r.GetBoolean(3),
            TotalProductos = Convert.ToInt32(r.GetInt64(4))
        };
    }

    // ══════════════════════════════════════════════════════════
    //  CLIENTE REPOSITORY
    // ══════════════════════════════════════════════════════════
    public class ClienteRepository : BaseRepository
    {
        public List<Cliente> GetAll()
        {
            var lista = new List<Cliente>();
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT id, nombre, documento, telefono, correo, direccion, activo
                  FROM tb_clientes
                  WHERE activo = true
                  ORDER BY nombre", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) lista.Add(MapCliente(reader));
            return lista;
        }

        public List<Cliente> Search(string filtro)
        {
            var lista = new List<Cliente>();
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT id, nombre, documento, telefono, correo, direccion, activo
                  FROM tb_clientes
                  WHERE activo = true
                    AND (nombre    ILIKE @f
                      OR documento ILIKE @f
                      OR correo    ILIKE @f)
                  ORDER BY nombre", conn);
            cmd.Parameters.AddWithValue("f", $"%{filtro}%");
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) lista.Add(MapCliente(reader));
            return lista;
        }

        public bool Create(Cliente c)
        {
            if (ExisteDocumento(c.Documento, excluirId: null))
                throw new InvalidOperationException($"Ya existe un cliente con el documento '{c.Documento}'.");

            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                @"INSERT INTO tb_clientes (nombre, documento, telefono, correo, direccion)
                  VALUES (@nombre, @doc, @tel, @correo, @dir)", conn);
            cmd.Parameters.AddWithValue("nombre", c.Nombre);
            cmd.Parameters.AddWithValue("doc", c.Documento);
            cmd.Parameters.AddWithValue("tel", c.Telefono ?? "");
            cmd.Parameters.AddWithValue("correo", c.Email ?? "");
            cmd.Parameters.AddWithValue("dir", c.Direccion ?? "");
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Update(Cliente c)
        {
            if (ExisteDocumento(c.Documento, excluirId: c.Id))
                throw new InvalidOperationException($"Ya existe otro cliente con el documento '{c.Documento}'.");

            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                @"UPDATE tb_clientes
                  SET nombre    = @nombre,
                      documento = @doc,
                      telefono  = @tel,
                      correo    = @correo,
                      direccion = @dir
                  WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("nombre", c.Nombre);
            cmd.Parameters.AddWithValue("doc", c.Documento);
            cmd.Parameters.AddWithValue("tel", c.Telefono ?? "");
            cmd.Parameters.AddWithValue("correo", c.Email ?? "");
            cmd.Parameters.AddWithValue("dir", c.Direccion ?? "");
            cmd.Parameters.AddWithValue("id", c.Id);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Delete(int id)
        {
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                "UPDATE tb_clientes SET activo = false WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        private bool ExisteDocumento(string documento, int? excluirId)
        {
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT COUNT(*) FROM tb_clientes
                  WHERE documento = @doc AND (@excluirId::int IS NULL OR id <> @excluirId)", conn);
            cmd.Parameters.AddWithValue("doc", documento);
            cmd.Parameters.AddWithValue("excluirId", (object?)excluirId ?? DBNull.Value);
            return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
        }

        private static Cliente MapCliente(NpgsqlDataReader r) => new()
        {
            Id = r.GetInt32(0),
            Nombre = r.GetString(1),
            Documento = r.GetString(2),
            Telefono = r.IsDBNull(3) ? "" : r.GetString(3),
            Email = r.IsDBNull(4) ? "" : r.GetString(4),
            Direccion = r.IsDBNull(5) ? "" : r.GetString(5),
            Activo = r.GetBoolean(6)
        };
    }

    // ══════════════════════════════════════════════════════════
    //  EMPLEADO REPOSITORY
    // ══════════════════════════════════════════════════════════
    public class EmpleadoRepository : BaseRepository
    {
        public List<Empleado> GetAll()
        {
            var lista = new List<Empleado>();
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT id, nombre, documento, rol, telefono, correo, direccion,
                         fecha_ingreso, fecha_retiro, datos_adicionales, activo
                  FROM tb_empleados
                  WHERE activo = true
                  ORDER BY nombre", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) lista.Add(MapEmpleado(reader));
            return lista;
        }

        public List<Empleado> Search(string filtro)
        {
            var lista = new List<Empleado>();
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT id, nombre, documento, rol, telefono, correo, direccion,
                         fecha_ingreso, fecha_retiro, datos_adicionales, activo
                  FROM tb_empleados
                  WHERE activo = true
                    AND (nombre    ILIKE @f
                      OR documento ILIKE @f
                      OR rol       ILIKE @f)
                  ORDER BY nombre", conn);
            cmd.Parameters.AddWithValue("f", $"%{filtro}%");
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) lista.Add(MapEmpleado(reader));
            return lista;
        }

        public bool Create(Empleado e)
        {
            if (ExisteDocumento(e.Documento, excluirId: null))
                throw new InvalidOperationException($"Ya existe un empleado con el documento '{e.Documento}'.");

            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                @"INSERT INTO tb_empleados
                    (nombre, documento, rol, telefono, correo, direccion, fecha_ingreso, fecha_retiro, datos_adicionales)
                  VALUES
                    (@nombre, @doc, @rol, @tel, @correo, @dir, @ingreso, @retiro, @datos)", conn);
            cmd.Parameters.AddWithValue("nombre", e.Nombre);
            cmd.Parameters.AddWithValue("doc", e.Documento);
            cmd.Parameters.AddWithValue("rol", e.Rol);
            cmd.Parameters.AddWithValue("tel", e.Telefono ?? "");
            cmd.Parameters.AddWithValue("correo", e.Email ?? "");
            cmd.Parameters.AddWithValue("dir", e.Direccion ?? "");
            cmd.Parameters.AddWithValue("ingreso", e.FechaIngreso);
            cmd.Parameters.AddWithValue("retiro", (object?)e.FechaRetiro ?? DBNull.Value);
            cmd.Parameters.AddWithValue("datos", e.DatosAdicionales ?? "");
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Update(Empleado e)
        {
            if (ExisteDocumento(e.Documento, excluirId: e.Id))
                throw new InvalidOperationException($"Ya existe otro empleado con el documento '{e.Documento}'.");

            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                @"UPDATE tb_empleados
                  SET nombre            = @nombre,
                      documento         = @doc,
                      rol               = @rol,
                      telefono          = @tel,
                      correo            = @correo,
                      direccion         = @dir,
                      fecha_ingreso     = @ingreso,
                      fecha_retiro      = @retiro,
                      datos_adicionales = @datos
                  WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("nombre", e.Nombre);
            cmd.Parameters.AddWithValue("doc", e.Documento);
            cmd.Parameters.AddWithValue("rol", e.Rol);
            cmd.Parameters.AddWithValue("tel", e.Telefono ?? "");
            cmd.Parameters.AddWithValue("correo", e.Email ?? "");
            cmd.Parameters.AddWithValue("dir", e.Direccion ?? "");
            cmd.Parameters.AddWithValue("ingreso", e.FechaIngreso);
            cmd.Parameters.AddWithValue("retiro", (object?)e.FechaRetiro ?? DBNull.Value);
            cmd.Parameters.AddWithValue("datos", e.DatosAdicionales ?? "");
            cmd.Parameters.AddWithValue("id", e.Id);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Delete(int id)
        {
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                "UPDATE tb_empleados SET activo = false WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        private bool ExisteDocumento(string documento, int? excluirId)
        {
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT COUNT(*) FROM tb_empleados
                  WHERE documento = @doc AND (@excluirId::int IS NULL OR id <> @excluirId)", conn);
            cmd.Parameters.AddWithValue("doc", documento);
            cmd.Parameters.AddWithValue("excluirId", (object?)excluirId ?? DBNull.Value);
            return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
        }

        private static Empleado MapEmpleado(NpgsqlDataReader r) => new()
        {
            Id = r.GetInt32(0),
            Nombre = r.GetString(1),
            Documento = r.GetString(2),
            Rol = r.GetString(3),
            Telefono = r.IsDBNull(4) ? "" : r.GetString(4),
            Email = r.IsDBNull(5) ? "" : r.GetString(5),
            Direccion = r.IsDBNull(6) ? "" : r.GetString(6),
            FechaIngreso = r.GetDateTime(7),
            FechaRetiro = r.IsDBNull(8) ? null : r.GetDateTime(8),
            DatosAdicionales = r.IsDBNull(9) ? "" : r.GetString(9),
            Activo = r.GetBoolean(10)
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

    // ══════════════════════════════════════════════════════════
    //  FACTURA REPOSITORY (maestro tb_facturas + detalle tb_detalle_factura)
    // ══════════════════════════════════════════════════════════
    public class FacturaRepository : BaseRepository
    {
        private const string SelectFactura =
            @"SELECT f.id, f.nro_factura, f.fecha_registro,
                     f.cliente_id, c.nombre AS cliente,
                     f.empleado_id, e.nombre AS empleado,
                     f.estado, f.subtotal, f.descuento, f.total_iva, f.total_factura
              FROM tb_facturas f
              JOIN tb_clientes  c ON f.cliente_id  = c.id
              JOIN tb_empleados e ON f.empleado_id = e.id";

        public List<Factura> GetAll()
        {
            var lista = new List<Factura>();
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(SelectFactura + " ORDER BY f.fecha_registro DESC, f.id DESC", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) lista.Add(MapFactura(reader));
            return lista;
        }

        public List<Factura> Search(string filtro, string estado)
        {
            var lista = new List<Factura>();
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(SelectFactura +
                @" WHERE (@estado = 'Todos' OR f.estado = @estado)
                     AND (f.nro_factura ILIKE @f OR c.nombre ILIKE @f OR e.nombre ILIKE @f)
                   ORDER BY f.fecha_registro DESC, f.id DESC", conn);
            cmd.Parameters.AddWithValue("estado", estado);
            cmd.Parameters.AddWithValue("f", $"%{filtro}%");
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) lista.Add(MapFactura(reader));
            return lista;
        }

        /// <summary>Devuelve la factura con sus líneas de detalle.</summary>
        public Factura? GetById(int id)
        {
            using var conn = GetConnection();
            Factura? factura;
            using (var cmd = new NpgsqlCommand(SelectFactura + " WHERE f.id = @id", conn))
            {
                cmd.Parameters.AddWithValue("id", id);
                using var reader = cmd.ExecuteReader();
                if (!reader.Read()) return null;
                factura = MapFactura(reader);
            }

            using (var cmd = new NpgsqlCommand(
                @"SELECT d.id, d.factura_id, d.producto_id, p.codigo, p.nombre,
                         d.cantidad, d.precio_unitario
                  FROM tb_detalle_factura d
                  JOIN tb_productos p ON d.producto_id = p.id
                  WHERE d.factura_id = @id
                  ORDER BY d.id", conn))
            {
                cmd.Parameters.AddWithValue("id", id);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    factura.Detalles.Add(new DetalleFactura
                    {
                        Id = reader.GetInt32(0),
                        FacturaId = reader.GetInt32(1),
                        ProductoId = reader.GetInt32(2),
                        CodigoProducto = reader.GetString(3),
                        NombreProducto = reader.GetString(4),
                        Cantidad = reader.GetInt32(5),
                        PrecioUnitario = reader.GetDecimal(6)
                    });
                }
            }
            return factura;
        }

        /// <summary>Calcula el siguiente consecutivo con formato FAC-00001.</summary>
        public string GetSiguienteNumero()
        {
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand("SELECT COALESCE(MAX(id), 0) + 1 FROM tb_facturas", conn);
            int siguiente = Convert.ToInt32(cmd.ExecuteScalar());
            return $"FAC-{siguiente:D5}";
        }

        /// <summary>
        /// Inserta encabezado + detalle y descuenta el stock, todo en una
        /// transacción: si algo falla (p. ej. stock insuficiente) no se guarda nada.
        /// </summary>
        public int Create(Factura f)
        {
            using var conn = GetConnection();
            using var tx = conn.BeginTransaction();
            try
            {
                using var cmd = new NpgsqlCommand(
                    @"INSERT INTO tb_facturas
                        (nro_factura, fecha_registro, cliente_id, empleado_id, estado,
                         subtotal, descuento, total_iva, total_factura)
                      VALUES (@nro, @fecha, @cliente, @empleado, @estado,
                              @subtotal, @descuento, @iva, @total)
                      RETURNING id", conn, tx);
                AgregarParametrosEncabezado(cmd, f);
                cmd.Parameters.AddWithValue("nro", f.NroFactura);
                f.Id = Convert.ToInt32(cmd.ExecuteScalar());

                InsertarDetalles(conn, tx, f);
                tx.Commit();
                return f.Id;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Modifica la factura: devuelve al inventario el stock de las líneas
        /// anteriores, las borra e inserta las nuevas descontando de nuevo.
        /// </summary>
        public bool Update(Factura f)
        {
            using var conn = GetConnection();
            using var tx = conn.BeginTransaction();
            try
            {
                if (ObtenerEstado(conn, tx, f.Id) == "Anulada")
                    throw new InvalidOperationException("No se puede modificar una factura anulada.");

                DevolverStock(conn, tx, f.Id);
                using (var del = new NpgsqlCommand(
                    "DELETE FROM tb_detalle_factura WHERE factura_id = @id", conn, tx))
                {
                    del.Parameters.AddWithValue("id", f.Id);
                    del.ExecuteNonQuery();
                }

                using (var cmd = new NpgsqlCommand(
                    @"UPDATE tb_facturas
                      SET fecha_registro = @fecha,
                          cliente_id     = @cliente,
                          empleado_id    = @empleado,
                          estado         = @estado,
                          subtotal       = @subtotal,
                          descuento      = @descuento,
                          total_iva      = @iva,
                          total_factura  = @total
                      WHERE id = @id", conn, tx))
                {
                    AgregarParametrosEncabezado(cmd, f);
                    cmd.Parameters.AddWithValue("id", f.Id);
                    cmd.ExecuteNonQuery();
                }

                InsertarDetalles(conn, tx, f);
                tx.Commit();
                return true;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Retiro lógico: la factura queda en estado "Anulada" (no se borra,
        /// por trazabilidad contable) y el stock vendido regresa al inventario.
        /// </summary>
        public bool Anular(int id)
        {
            using var conn = GetConnection();
            using var tx = conn.BeginTransaction();
            try
            {
                if (ObtenerEstado(conn, tx, id) == "Anulada")
                    throw new InvalidOperationException("La factura ya se encuentra anulada.");

                DevolverStock(conn, tx, id);
                using var cmd = new NpgsqlCommand(
                    "UPDATE tb_facturas SET estado = 'Anulada' WHERE id = @id", conn, tx);
                cmd.Parameters.AddWithValue("id", id);
                bool ok = cmd.ExecuteNonQuery() > 0;
                tx.Commit();
                return ok;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        // ── Informes ────────────────────────────────────────────
        public List<Factura> GetPorRangoFechas(DateTime desde, DateTime hasta)
        {
            var lista = new List<Factura>();
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(SelectFactura +
                @" WHERE f.fecha_registro >= @desde AND f.fecha_registro < @hasta
                   ORDER BY f.fecha_registro", conn);
            cmd.Parameters.AddWithValue("desde", desde.Date);
            cmd.Parameters.AddWithValue("hasta", hasta.Date.AddDays(1));
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) lista.Add(MapFactura(reader));
            return lista;
        }

        public List<ProductoVendido> GetProductosMasVendidos(DateTime desde, DateTime hasta)
        {
            var lista = new List<ProductoVendido>();
            using var conn = GetConnection();
            using var cmd = new NpgsqlCommand(
                @"SELECT p.codigo, p.nombre, COALESCE(p.categoria, ''),
                         SUM(d.cantidad) AS unidades, SUM(d.total_linea) AS ingresos
                  FROM tb_detalle_factura d
                  JOIN tb_facturas  f ON d.factura_id  = f.id
                  JOIN tb_productos p ON d.producto_id = p.id
                  WHERE f.estado <> 'Anulada'
                    AND f.fecha_registro >= @desde AND f.fecha_registro < @hasta
                  GROUP BY p.codigo, p.nombre, p.categoria
                  ORDER BY unidades DESC", conn);
            cmd.Parameters.AddWithValue("desde", desde.Date);
            cmd.Parameters.AddWithValue("hasta", hasta.Date.AddDays(1));
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new ProductoVendido
                {
                    Codigo = reader.GetString(0),
                    Nombre = reader.GetString(1),
                    Categoria = reader.GetString(2),
                    UnidadesVendidas = Convert.ToInt32(reader.GetInt64(3)),
                    Ingresos = reader.GetDecimal(4)
                });
            }
            return lista;
        }

        // ── Auxiliares ──────────────────────────────────────────
        private static void AgregarParametrosEncabezado(NpgsqlCommand cmd, Factura f)
        {
            cmd.Parameters.AddWithValue("fecha", f.FechaRegistro);
            cmd.Parameters.AddWithValue("cliente", f.ClienteId);
            cmd.Parameters.AddWithValue("empleado", f.EmpleadoId);
            cmd.Parameters.AddWithValue("estado", f.Estado);
            cmd.Parameters.AddWithValue("subtotal", f.Subtotal);
            cmd.Parameters.AddWithValue("descuento", f.Descuento);
            cmd.Parameters.AddWithValue("iva", f.TotalIva);
            cmd.Parameters.AddWithValue("total", f.TotalFactura);
        }

        private static void InsertarDetalles(NpgsqlConnection conn, NpgsqlTransaction tx, Factura f)
        {
            foreach (var d in f.Detalles)
            {
                // Descuenta stock solo si alcanza; si no, aborta toda la factura
                using (var stock = new NpgsqlCommand(
                    @"UPDATE tb_productos
                      SET stock_actual = stock_actual - @cant
                      WHERE id = @prod AND stock_actual >= @cant", conn, tx))
                {
                    stock.Parameters.AddWithValue("cant", d.Cantidad);
                    stock.Parameters.AddWithValue("prod", d.ProductoId);
                    if (stock.ExecuteNonQuery() == 0)
                        throw new InvalidOperationException(
                            $"Stock insuficiente para el producto {d.CodigoProducto} - {d.NombreProducto}.");
                }

                using var cmd = new NpgsqlCommand(
                    @"INSERT INTO tb_detalle_factura
                        (factura_id, producto_id, cantidad, precio_unitario, subtotal, iva, total_linea)
                      VALUES (@factura, @prod, @cant, @precio, @subtotal, @iva, @total)", conn, tx);
                cmd.Parameters.AddWithValue("factura", f.Id);
                cmd.Parameters.AddWithValue("prod", d.ProductoId);
                cmd.Parameters.AddWithValue("cant", d.Cantidad);
                cmd.Parameters.AddWithValue("precio", d.PrecioUnitario);
                cmd.Parameters.AddWithValue("subtotal", d.Subtotal);
                cmd.Parameters.AddWithValue("iva", d.Iva);
                cmd.Parameters.AddWithValue("total", d.TotalLinea);
                cmd.ExecuteNonQuery();
            }
        }

        private static void DevolverStock(NpgsqlConnection conn, NpgsqlTransaction tx, int facturaId)
        {
            using var cmd = new NpgsqlCommand(
                @"UPDATE tb_productos p
                  SET stock_actual = p.stock_actual + d.cantidad
                  FROM tb_detalle_factura d
                  WHERE d.producto_id = p.id AND d.factura_id = @id", conn, tx);
            cmd.Parameters.AddWithValue("id", facturaId);
            cmd.ExecuteNonQuery();
        }

        private static string ObtenerEstado(NpgsqlConnection conn, NpgsqlTransaction tx, int facturaId)
        {
            using var cmd = new NpgsqlCommand(
                "SELECT estado FROM tb_facturas WHERE id = @id FOR UPDATE", conn, tx);
            cmd.Parameters.AddWithValue("id", facturaId);
            return cmd.ExecuteScalar() as string
                ?? throw new InvalidOperationException("La factura no existe.");
        }

        private static Factura MapFactura(NpgsqlDataReader r) => new()
        {
            Id = r.GetInt32(0),
            NroFactura = r.GetString(1),
            FechaRegistro = r.GetDateTime(2),
            ClienteId = r.GetInt32(3),
            Cliente = r.GetString(4),
            EmpleadoId = r.GetInt32(5),
            Empleado = r.GetString(6),
            Estado = r.GetString(7),
            Subtotal = r.GetDecimal(8),
            Descuento = r.GetDecimal(9),
            TotalIva = r.GetDecimal(10),
            TotalFactura = r.GetDecimal(11)
        };
    }
}