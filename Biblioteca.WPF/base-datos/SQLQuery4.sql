USE BibliotecaDB;
GO

/* =========================================================
   LIMPIEZA TOTAL (orden hijos → padres)
   ========================================================= */
DELETE FROM dbo.DetallePrestamo;
DELETE FROM dbo.Prestamos;
DELETE FROM dbo.Libros;
DELETE FROM dbo.Socios;
DELETE FROM dbo.Autores;
GO

/* =========================================================
   RESEED SOLO de tablas CON IDENTITY
   (NO incluir DetallePrestamo: tiene PK compuesta, sin identity)
   ========================================================= */
DBCC CHECKIDENT ('dbo.Prestamos', RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.Libros',    RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.Socios',    RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.Autores',   RESEED, 0) WITH NO_INFOMSGS;
GO

/* =========================================================
   1) AUTORES  (8)
   ========================================================= */
INSERT INTO dbo.Autores (Nombre, Nacionalidad, Activo) VALUES
('Gabriel García Márquez', 'Colombiano', 1),
('Mario Vargas Llosa',     'Peruano',    1),
('Jorge Luis Borges',      'Argentino',  1),
('Isabel Allende',         'Chilena',    1),
('Julio Cortázar',         'Argentino',  1),
('Pablo Neruda',           'Chileno',    1),
('Miguel de Cervantes',    'Español',    1),
('Franz Kafka',            'Checo',      1);
GO

/* =========================================================
   2) LIBROS  (20)
   ========================================================= */
INSERT INTO dbo.Libros (Titulo, ISBN, AutorId, Ejemplares, Activo) VALUES
('Cien años de soledad',              '978-0307474728', 1, 5, 1),
('El amor en los tiempos del cólera', '978-0307387264', 1, 3, 1),
('Crónica de una muerte anunciada',   '978-1400034956', 1, 4, 1),
('El otoño del patriarca',            '978-0307474698', 1, 3, 1),
('La ciudad y los perros',            '978-8420471839', 2, 3, 1),
('La fiesta del chivo',               '978-8420469515', 2, 2, 1),
('Conversación en La Catedral',       '978-8420431504', 2, 3, 1),
('Ficciones',                         '978-0802130303', 3, 4, 1),
('El Aleph',                          '978-0142437889', 3, 3, 1),
('La casa de los espíritus',          '978-1501117015', 4, 4, 1),
('Eva Luna',                          '978-1501117022', 4, 3, 1),
('Rayuela',                           '978-8437604572', 5, 3, 1),
('Bestiario',                         '978-8437604589', 5, 2, 1),
('Veinte poemas de amor',             '978-0811212444', 6, 5, 1),
('Cien sonetos de amor',              '978-0811217296', 6, 3, 1),
('Don Quijote de la Mancha',          '978-8437622514', 7, 6, 1),
('Novelas ejemplares',                '978-8437623559', 7, 2, 1),
('La metamorfosis',                   '978-0805210576', 8, 5, 1),
('El proceso',                        '978-0805211238', 8, 3, 1),
('América',                           '978-0805211351', 8, 2, 1);
GO

/* =========================================================
   3) SOCIOS  (10)
   ========================================================= */
INSERT INTO dbo.Socios (DNI, Nombre, Email, Activo) VALUES
('12345678', 'Juan Pérez',      'juan.perez@mail.com',    1),
('23456789', 'María López',     'maria.lopez@mail.com',   1),
('34567890', 'Carlos Ruiz',     'carlos.ruiz@mail.com',   1),
('45678901', 'Ana Torres',      'ana.torres@mail.com',    1),
('56789012', 'Luis Gómez',      'luis.gomez@mail.com',    1),
('67890123', 'Pedro Sánchez',   'pedro.sanchez@mail.com', 1),
('78901234', 'Lucía Fernández', 'lucia.fernandez@mail.com', 1),
('89012345', 'Roberto Díaz',    'roberto.diaz@mail.com',  1),
('90123456', 'Sofía Ramírez',   'sofia.ramirez@mail.com', 1),
('01234567', 'Diego Castillo',  'diego.castillo@mail.com',1);
GO

/* =========================================================
   4) PRESTAMOS  (5)
   ========================================================= */
INSERT INTO dbo.Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado)
VALUES (1, '2026-09-01', '2026-09-08', 'Devuelto');

INSERT INTO dbo.Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado)
VALUES (2, '2026-09-10', '2026-09-17', 'Pendiente');

INSERT INTO dbo.Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado)
VALUES (3, '2026-09-15', '2026-09-22', 'Pendiente');

INSERT INTO dbo.Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado)
VALUES (4, '2026-09-18', '2026-09-25', 'Pendiente');

INSERT INTO dbo.Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado)
VALUES (5, '2026-08-20', '2026-08-27', 'Devuelto');
GO

/* =========================================================
   5) DETALLE PRESTAMO
   ========================================================= */
INSERT INTO dbo.DetallePrestamo (PrestamoId, LibroId, FechaDevolucion)
VALUES (1, 1, '2026-09-05');

INSERT INTO dbo.DetallePrestamo (PrestamoId, LibroId, FechaDevolucion)
VALUES (2, 2, NULL);

INSERT INTO dbo.DetallePrestamo (PrestamoId, LibroId, FechaDevolucion) VALUES
(3, 5,  NULL),
(3, 8,  NULL),
(3, 12, NULL);

INSERT INTO dbo.DetallePrestamo (PrestamoId, LibroId, FechaDevolucion) VALUES
(4, 3, '2026-09-22'),
(4, 9, NULL);

INSERT INTO dbo.DetallePrestamo (PrestamoId, LibroId, FechaDevolucion)
VALUES (5, 14, '2026-08-30');
GO

/* =========================================================
   6) AJUSTE DE STOCK
   ========================================================= */
UPDATE dbo.Libros SET Ejemplares = Ejemplares - 1
WHERE LibroId IN (2, 5, 8, 9, 12);
GO

PRINT 'Datos de prueba insertados correctamente.';
GO