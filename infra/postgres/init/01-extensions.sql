-- Extensiones usadas por TallerOS:
-- citext    -> columnas de correo sin distinguir mayúsculas/minúsculas (usuario.email)
-- pg_trgm   -> búsqueda parcial por placa, nombre de cliente y nombre de producto
CREATE EXTENSION IF NOT EXISTS citext;
CREATE EXTENSION IF NOT EXISTS pg_trgm;
