# MiniMarket POS

Sistema de Punto de Venta (POS) completo para minimarket desarrollado con .NET 8 MVC y arquitectura hexagonal.

## Tecnologias

- **.NET 8** - Framework principal
- **ASP.NET Core MVC** - Capa de presentacion
- **Entity Framework Core 8** - ORM y acceso a datos
- **SQL Server** - Base de datos
- **Bootstrap 5** - UI/UX
- **FluentValidation** - Validaciones
- **xUnit** - Testing

## Arquitectura

El proyecto sigue una arquitectura hexagonal (Clean Architecture):

```
MiniMarket.sln
├── src/
│   ├── MiniMarket.Domain/        # Entidades, Value Objects, Puertos
│   ├── MiniMarket.Application/   # Casos de uso, DTOs, Servicios
│   ├── MiniMarket.Infrastructure/# Repositorios, DbContext, Servicios externos
│   └── MiniMarket.Web/           # Controllers, Views, UI
└── tests/
    ├── MiniMarket.Domain.Tests/
    ├── MiniMarket.Application.Tests/
    └── MiniMarket.Infrastructure.Tests/
```

## Requisitos Previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (una de las siguientes opciones):
  - SQL Server Express/Developer (Windows)
  - Docker con SQL Server (Linux/macOS/Windows)

## Instalacion

### 1. Clonar el repositorio

```bash
git clone https://github.com/tu-usuario/minimarket.git
cd minimarket
```

### 2. Configurar la base de datos

#### Opcion A: Docker (Recomendado para Linux/macOS)

```bash
# Iniciar SQL Server en Docker
docker run -e 'ACCEPT_EULA=Y' \
  -e 'MSSQL_SA_PASSWORD=MiniMarket123!' \
  -p 1433:1433 \
  --name minimarket-db \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

Luego, actualiza la cadena de conexion en `src/MiniMarket.Web/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=MiniMarketDb;User Id=sa;Password=MiniMarket123!;TrustServerCertificate=True"
  }
}
```

#### Opcion B: SQL Server LocalDB (Solo Windows)

La configuracion por defecto usa LocalDB. No requiere cambios adicionales.

### 3. Restaurar dependencias

```bash
dotnet restore
```

### 4. Aplicar migraciones

```bash
dotnet ef database update --project src/MiniMarket.Infrastructure --startup-project src/MiniMarket.Web
```

### 5. Ejecutar la aplicacion

```bash
dotnet run --project src/MiniMarket.Web
```

La aplicacion estara disponible en: `http://localhost:5095`

## Credenciales por Defecto

Al ejecutar por primera vez, se crea un usuario administrador:

- **Usuario:** `admin`
- **Contrasena:** `Admin123!`

## Funcionalidades

### Modulos Core
- **Punto de Venta (POS)** - Interfaz de ventas con carrito y multiples metodos de pago
- **Productos** - CRUD completo, categorias, historial de precios
- **Inventario** - Gestion de stock, kardex, alertas de stock bajo
- **Caja** - Apertura/cierre de turno, arqueo, movimientos

### Modulos Secundarios
- **Clientes** - Gestion de clientes, sistema de credito/fiado
- **Proveedores** - CRUD de proveedores
- **Compras** - Ordenes de compra, recepcion de mercancia
- **Gastos** - Registro y categorizacion de gastos operativos

### Funcionalidades Avanzadas
- **Dashboard** - KPIs y metricas en tiempo real
- **Reportes** - Ventas, inventario, rentabilidad
- **Alertas** - Stock bajo, productos por vencer, deudas
- **Auditoria** - Logs de todas las operaciones
- **Backup** - Respaldo y restauracion de base de datos
- **Sincronizacion** - Soporte offline-first con cola de sincronizacion

### Seguridad
- Autenticacion basada en cookies
- Roles: Administrador y Cajero
- Permisos por modulo

## Estructura de la Base de Datos

### Entidades Principales
- `Products` - Productos del inventario
- `Categories` - Categorias de productos
- `Sales` / `SaleItems` - Ventas y detalle
- `Customers` - Clientes
- `Suppliers` - Proveedores
- `PurchaseOrders` - Ordenes de compra
- `CashRegisters` - Turnos de caja
- `Users` - Usuarios del sistema

## Comandos Utiles

```bash
# Compilar solucion
dotnet build

# Ejecutar tests
dotnet test

# Ejecutar con hot reload
dotnet watch run --project src/MiniMarket.Web

# Crear nueva migracion
dotnet ef migrations add NombreMigracion --project src/MiniMarket.Infrastructure --startup-project src/MiniMarket.Web

# Revertir ultima migracion
dotnet ef migrations remove --project src/MiniMarket.Infrastructure --startup-project src/MiniMarket.Web
```

## Configuracion Adicional

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "..."
  },
  "Backup": {
    "Path": "./Backups"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

## Contribuir

1. Fork el repositorio
2. Crea una rama para tu feature (`git checkout -b feature/nueva-funcionalidad`)
3. Commit tus cambios (`git commit -m 'Agregar nueva funcionalidad'`)
4. Push a la rama (`git push origin feature/nueva-funcionalidad`)
5. Abre un Pull Request

## Licencia

Este proyecto esta bajo la Licencia MIT. Ver el archivo `LICENSE` para mas detalles.
