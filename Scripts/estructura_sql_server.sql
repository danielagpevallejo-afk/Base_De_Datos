CREATE TABLE Estados (
    Id nvarchar(8) NOT NULL PRIMARY KEY,
    Tipo nvarchar(20) NOT NULL,
    Categoria nvarchar(50) NULL
);

CREATE TABLE Simbolos (
    Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Valor nvarchar(20) NOT NULL UNIQUE
);

CREATE TABLE Transiciones (
    Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    EstadoOrigenId nvarchar(8) NOT NULL,
    SimboloId int NOT NULL,
    EstadoDestinoId nvarchar(8) NOT NULL,
    CONSTRAINT FK_Transicion_Origen FOREIGN KEY (EstadoOrigenId) REFERENCES Estados(Id),
    CONSTRAINT FK_Transicion_Simbolo FOREIGN KEY (SimboloId) REFERENCES Simbolos(Id),
    CONSTRAINT FK_Transicion_Destino FOREIGN KEY (EstadoDestinoId) REFERENCES Estados(Id),
    CONSTRAINT UQ_Transicion UNIQUE (EstadoOrigenId, SimboloId, EstadoDestinoId)
);

SELECT t.EstadoOrigenId, s.Valor AS Entrada, t.EstadoDestinoId, e.Categoria
FROM Transiciones t
JOIN Simbolos s ON s.Id = t.SimboloId
JOIN Estados e ON e.Id = t.EstadoDestinoId
ORDER BY TRY_CONVERT(int, SUBSTRING(t.EstadoOrigenId, 2, 10)), s.Valor;
