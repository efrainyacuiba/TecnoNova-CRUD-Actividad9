-- Prueba A: mismo rango que el reporte diario. Consulta manual para PostgreSQL.
SELECT COUNT(id) AS operaciones, COALESCE(SUM(subtotal_bruto),0) AS bruto,
       COALESCE(SUM(monto_descuento),0) AS descuentos,
       COALESCE(SUM(total_final),0) AS total_sql
FROM ventas WHERE fecha::date BETWEEN DATE '2026-11-24' AND DATE '2026-11-24';
-- Prueba B: subtotal de cada mes; no se incluyen detalles para evitar duplicar cabeceras.
SELECT TO_CHAR(fecha,'YYYY-MM') AS mes, COUNT(id) AS operaciones,
       SUM(monto_descuento) AS descuentos, SUM(total_final) AS total_cobrado
FROM ventas WHERE fecha::date BETWEEN DATE '2026-10-01' AND DATE '2026-11-30'
GROUP BY 1 ORDER BY 1;
-- Prueba C: en una BD de demostración nueva se esperan cero registros.
SELECT COUNT(*) FROM ventas WHERE fecha::date BETWEEN DATE '2020-01-01' AND DATE '2020-12-31';
