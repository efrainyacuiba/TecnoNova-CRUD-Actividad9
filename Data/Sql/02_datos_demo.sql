-- Datos ficticios de catálogo para capturas de A8/A9; no sobrescribe datos reales.
INSERT INTO productos (codigo,nombre,precio,stock) VALUES
('TEC-001','Laptop Lenovo IdeaPad',3500.00,12),
('TEC-002','Mouse inalámbrico',85.00,80),
('TEC-003','Teclado mecánico RGB',280.00,30),
('TEC-004','Memoria USB 64GB',65.00,50),
('TEC-005','Monitor 24 pulgadas',1250.00,15),
('TEC-006','Audífonos Bluetooth',190.00,24)
ON CONFLICT (codigo) DO NOTHING;
