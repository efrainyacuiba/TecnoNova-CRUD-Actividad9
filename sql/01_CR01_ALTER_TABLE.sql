-- CR-01: migración incremental A8, compatible con A7 y reejecutable.
ALTER TABLE ventas ADD COLUMN IF NOT EXISTS vendedor VARCHAR(120) NOT NULL DEFAULT 'Sin identificar (venta anterior)';
ALTER TABLE ventas ADD COLUMN IF NOT EXISTS subtotal_bruto NUMERIC(12,2) NOT NULL DEFAULT 0;
ALTER TABLE ventas ADD COLUMN IF NOT EXISTS porcentaje_descuento NUMERIC(5,2) NOT NULL DEFAULT 0;
ALTER TABLE ventas ADD COLUMN IF NOT EXISTS monto_descuento NUMERIC(12,2) NOT NULL DEFAULT 0;
ALTER TABLE ventas ADD COLUMN IF NOT EXISTS total_final NUMERIC(12,2) NOT NULL DEFAULT 0;
-- Backfill solo de filas previas; los registros nuevos no se recalculan.
UPDATE ventas SET subtotal_bruto = total, total_final = total
WHERE subtotal_bruto = 0 AND total_final = 0 AND total > 0 AND porcentaje_descuento = 0;
DO $$
BEGIN
  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname='ck_ventas_descuento_0_20') THEN
    ALTER TABLE ventas ADD CONSTRAINT ck_ventas_descuento_0_20
      CHECK (porcentaje_descuento >= 0 AND porcentaje_descuento <= 20);
  END IF;
  IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname='ck_ventas_montos_no_negativos') THEN
    ALTER TABLE ventas ADD CONSTRAINT ck_ventas_montos_no_negativos
      CHECK (subtotal_bruto >= 0 AND monto_descuento >= 0 AND total_final >= 0);
  END IF;
END $$;
CREATE INDEX IF NOT EXISTS idx_ventas_descuento_vendedor ON ventas(vendedor) WHERE porcentaje_descuento > 0;

-- A10: índice para el filtro por rango de fechas.
CREATE INDEX IF NOT EXISTS idx_ventas_fecha ON ventas(fecha);
