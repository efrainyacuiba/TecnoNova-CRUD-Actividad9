-- Datos ficticios para reproducir las pruebas A10 (octubre y noviembre 2026).
-- Se ejecuta una sola vez por BD, en una transacción. No toca ventas existentes.
BEGIN;
SELECT pg_advisory_xact_lock(102026);
CREATE TABLE IF NOT EXISTS a10_demo_control (clave text PRIMARY KEY);
DO $$
DECLARE
    fila record;
    producto integer;
    venta integer;
    bruto numeric(12,2);
    descuento numeric(12,2);
BEGIN
    IF EXISTS (SELECT 1 FROM a10_demo_control WHERE clave = 'ventas-2026-v1') THEN
        RETURN;
    END IF;
    INSERT INTO productos(codigo,nombre,precio,stock)
    VALUES ('A10-DEMO','Accesorio de prueba A10',100.00,100)
    RETURNING id INTO producto;
    FOR fila IN SELECT * FROM (VALUES
        (TIMESTAMP '2026-10-10 09:00', 'Demo A10 - Cliente 1', 'Ana (demo)', 1, 10),
        (TIMESTAMP '2026-10-23 14:00', 'Demo A10 - Cliente 2', 'Luis (demo)', 2, 0),
        (TIMESTAMP '2026-11-24 09:15', 'Demo A10 - Cliente 3', 'Ana (demo)', 3, 10),
        (TIMESTAMP '2026-11-24 11:30', 'Demo A10 - Cliente 4', 'Luis (demo)', 4, 10),
        (TIMESTAMP '2026-11-24 15:45', 'Demo A10 - Cliente 5', 'Ana (demo)', 5, 0)
    ) AS d(fecha,cliente,vendedor,cantidad,porcentaje)
    LOOP
        bruto := fila.cantidad * 100.00;
        descuento := round(bruto * fila.porcentaje / 100.00, 2);
        INSERT INTO ventas(fecha,cliente,total,vendedor,subtotal_bruto,porcentaje_descuento,monto_descuento,total_final)
        VALUES(fila.fecha,fila.cliente,bruto-descuento,fila.vendedor,bruto,fila.porcentaje,descuento,bruto-descuento)
        RETURNING id INTO venta;
        INSERT INTO detalle_ventas(id_venta,id_producto,nombre_producto,cantidad,precio_unitario,subtotal)
        VALUES(venta,producto,'Accesorio de prueba A10',fila.cantidad,100.00,bruto);
        UPDATE productos SET stock=stock-fila.cantidad WHERE id=producto;
    END LOOP;
    INSERT INTO a10_demo_control VALUES ('ventas-2026-v1');
END $$;
COMMIT;
