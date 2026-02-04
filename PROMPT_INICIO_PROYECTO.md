# Prompt para Iniciar Proyecto POS MiniMarket




## PROMPT

```
Actúa como un arquitecto de software senior y desarrollador .NET experto. Necesito que me ayudes a construir un sistema POS para un minimarket siguiendo estrictamente la arquitectura y especificaciones que te voy a proporcionar.

## CONTEXTO DEL PROYECTO

**Nombre:** POS MiniMarket
**Tipo:** Sistema de Punto de Venta para minimarket de alimentos y varios
**Framework:** .NET 8 MVC
**Arquitectura:** Hexagonal
**Base de datos:** SQL Server Express (servidor) / SQL Server LocalDB (cajas)

## ESTRUCTURA DE LA SOLUCIÓN

Crea la siguiente estructura de proyectos:

```
MiniMarket.sln
├── src/
│   ├── MiniMarket.Domain/           (Entidades, Value Objects, Puertos)
│   ├── MiniMarket.Application/      (Casos de uso, DTOs, Mappers)
│   ├── MiniMarket.Infrastructure/   (Adaptadores: BD, Hardware, Sync)
│   └── MiniMarket.Web/              (MVC: Controllers, Views, ViewModels)
├── tests/
│   ├── MiniMarket.Domain.Tests/
│   ├── MiniMarket.Application.Tests/
│   └── MiniMarket.Infrastructure.Tests/
└── docs/
```

## ARQUITECTURA HEXAGONAL

### Capa Domain (sin dependencias externas)
- Entidades y Agregados
- Value Objects
- Puertos (interfaces) de entrada y salida
- Excepciones de dominio
- Eventos de dominio

### Capa Application
- Casos de uso (implementación de puertos de entrada)
- DTOs de entrada y salida
- Mappers
- Validaciones

### Capa Infrastructure
- Implementación de repositorios (Entity Framework Core)
- Implementación de servicios externos
- Adaptadores de hardware
- Servicio de sincronización
- Servicio de idempotencia

### Capa Web (MVC)
- Controllers
- Views (Razor)
- ViewModels
- Filtros y Middleware
- Configuración de DI

## MÓDULOS DEL SISTEMA

1. **Productos/Catálogo:** Gestión de productos, variantes, categorías, precios
2. **Inventario:** Stock, kardex, lotes de vencimiento, alertas
3. **Ventas (POS):** Punto de venta, pagos múltiples, devoluciones
4. **Compras:** Proveedores, órdenes de compra, recepción
5. **Clientes:** Gestión de clientes, crédito/fiado, estados de cuenta
6. **Caja:** Turnos, arqueo, retiros, depósitos
7. **Gastos:** Categorías de gastos, registro de gastos operativos
8. **Usuarios:** Autenticación, roles (Admin/Cajero), permisos
9. **Reportes:** Dashboard, ventas, inventario, rentabilidad
10. **Sincronización:** Offline-first, cola de transacciones, push/pull
11. **Configuración:** Parámetros del sistema, consecutivos

## DISEÑO DE INTERFAZ

### Especificaciones de UI:
- **Estilo:** Limpio y profesional
- **Tema:** Solo tema claro (sin tema oscuro)
- **Idioma:** Solo español (sin multiidioma)
- **Responsive:** Sí

### Paleta de colores:
```css
:root {
    /* Colores principales */
    --color-primary: #DC3545;        /* Rojo - Acciones principales */
    --color-secondary: #FFC107;      /* Amarillo - Alertas/Precios */
    --color-success: #28A745;        /* Verde - Confirmaciones/Éxito */
    
    /* Variaciones */
    --color-primary-light: #F8D7DA;
    --color-primary-dark: #C82333;
    --color-secondary-light: #FFF3CD;
    --color-secondary-dark: #E0A800;
    --color-success-light: #D4EDDA;
    --color-success-dark: #218838;
    
    /* Neutros */
    --color-background: #F8F9FA;
    --color-surface: #FFFFFF;
    --color-border: #DEE2E6;
    --color-text-primary: #212529;
    --color-text-secondary: #6C757D;
    
    /* Sombras */
    --shadow-sm: 0 1px 2px rgba(0, 0, 0, 0.05);
    --shadow-md: 0 4px 6px rgba(0, 0, 0, 0.1);
}
```

### Uso de colores:
- **Rojo (#DC3545):** Botones primarios, acciones principales, navbar
- **Amarillo (#FFC107):** Alertas, advertencias, precios destacados
- **Verde (#28A745):** Confirmaciones, stock OK, operaciones exitosas
- **Gris claro (#F8F9FA):** Fondos
- **Blanco (#FFFFFF):** Tarjetas, superficies

## CARACTERÍSTICAS TÉCNICAS

### Sincronización:
- Cada caja tiene base de datos local independiente (LocalDB)
- Servidor central consolida toda la información
- Sincronización automática en tiempo real cuando hay conexión
- Cola de transacciones para operaciones offline
- Conflictos de inventario: permitir stock negativo y generar alerta

### Idempotencia:
- Implementar en todas las operaciones críticas
- Usar IdempotencyKey (GUID) generado en el cliente
- Almacenar resultados para evitar duplicados
- Operaciones protegidas: ventas, pagos, movimientos de inventario/caja

### Hardware soportado:
- Lector de código de barras (HID/Serial)
- Impresora térmica de tickets (ESC/POS)
- Generación de códigos de barras
- Impresión de etiquetas

## PAQUETES NUGET SUGERIDOS

```xml
<!-- Domain -->
<!-- Sin dependencias externas -->

<!-- Application -->
<PackageReference Include="FluentValidation" />
<PackageReference Include="AutoMapper" />

<!-- Infrastructure -->
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" />
<PackageReference Include="ZXing.Net" />
<PackageReference Include="EPPlus" />

<!-- Web -->
<PackageReference Include="Microsoft.AspNetCore.Authentication.Cookies" />
```

## INSTRUCCIONES

1. **Primero:** Crea la estructura completa de la solución con todos los proyectos
2. **Segundo:** Implementa la capa Domain con todas las entidades, value objects y puertos
3. **Tercero:** Implementa la capa Application con los casos de uso principales
4. **Cuarto:** Implementa la capa Infrastructure con Entity Framework Core
5. **Quinto:** Implementa la capa Web con los controladores y vistas

Comienza creando la estructura de la solución y el proyecto Domain con:
- Entidades principales (Product, Sale, Customer, CashRegister, etc.)
- Value Objects (Money, Barcode, Quantity, etc.)
- Puertos de entrada (interfaces de servicios)
- Puertos de salida (interfaces de repositorios)

¿Listo para comenzar?
```

---

## NOTAS ADICIONALES

### Para continuar el desarrollo:

Puedes usar prompts específicos como:

**Para implementar un módulo específico:**
```
Continúa con el proyecto POS MiniMarket. Implementa el módulo de [NOMBRE_MÓDULO] siguiendo la arquitectura hexagonal establecida.
```

**Para implementar una vista:**
```
Crea la vista de [NOMBRE_VISTA] para el módulo [NOMBRE_MÓDULO] usando la paleta de colores definida (rojo, amarillo, verde) con estilo limpio y profesional.
```

**Para implementar la sincronización:**
```
Implementa el servicio de sincronización offline-first con cola de transacciones siguiendo el patrón Outbox definido en la arquitectura.
```

**Para implementar idempotencia:**
```
Implementa el servicio de idempotencia para proteger las operaciones críticas (ventas, pagos, movimientos) contra duplicados.
```

### Archivos de referencia:

Si tienes el archivo `ARQUITECTURA_POS_MINIMARKET.md`, puedes adjuntarlo al prompt inicial diciendo:

```
Adjunto el documento de arquitectura completo. Sigue estas especificaciones exactamente.
```

---

## CHECKLIST DE IMPLEMENTACIÓN

### Fase 0 - Setup del Entorno
- [x] Instalar .NET 8 SDK (8.0.417)
- [x] Crear hooks.json (linter post-edit, pre-commit)
- [x] Crear skill: code-review
- [x] Crear skill: run-tests
- [x] Crear skill: stage-complete
- [x] Configurar Context7 MCP
- [x] Crear .editorconfig
- [x] Crear Directory.Build.props (analyzers)
- [x] Crear global.json

### Fase 1 - Fundamentos
- [x] Estructura de solución
- [x] Capa Domain completa
- [x] Capa Application básica
- [x] Configuración de Entity Framework
- [x] Migraciones iniciales

### Fase 2 - Módulos Core
- [x] Módulo de Productos
  - [x] ProductService + IProductService
  - [x] ProductRepository + IProductRepository
  - [x] ProductsController (CRUD completo)
  - [x] Vistas: Index, Create, Edit, Details
  - [x] Gestión de categorías (CategoryService, CategoriesController, Vistas)
- [x] Módulo de Inventario
  - [x] Integrado en Product (StockMovement, Kardex)
  - [x] Métodos AddStock/RemoveStock en entidad Product
  - [x] IInventoryService (interface)
- [x] Módulo de Ventas (POS)
  - [x] SaleService + ISaleService
  - [x] SaleRepository + ISaleRepository
  - [x] SalesController (API JSON para AJAX)
  - [x] Vista POS completa con JavaScript interactivo
  - [x] Vistas: Index, Create (POS), Details, Today
- [x] Módulo de Caja
  - [x] CashRegisterService + ICashRegisterService
  - [x] CashRegisterRepository + ICashRegisterRepository
  - [x] CashRegisterController
  - [x] Vistas: Index, Open, Current, Details

### Fase 3 - Módulos Secundarios
- [x] Módulo de Clientes
  - [x] CustomerService + ICustomerService
  - [x] CustomerRepository + ICustomerRepository
  - [x] CustomersController
  - [x] Vistas: Index, Create, Edit, Details, WithDebt
- [x] Módulo de Proveedores
  - [x] SupplierService + ISupplierService
  - [x] SupplierRepository + ISupplierRepository
  - [x] SuppliersController
  - [x] Vistas: Index, Create, Edit, Details
- [x] Módulo de Compras
  - [x] PurchaseOrderService + IPurchaseOrderService
  - [x] PurchaseOrderRepository + IPurchaseOrderRepository
  - [x] PurchaseOrdersController
  - [x] Vistas: Index, Pending, Create, Receive, Details
- [x] Módulo de Gastos
  - [x] ExpenseCategoryService + IExpenseCategoryService
  - [x] ExpenseCategoryRepository + IExpenseCategoryRepository
  - [x] ExpenseService + IExpenseService
  - [x] ExpenseRepository (ya existente)
  - [x] ExpensesController
  - [x] Vistas: Index, Today, Create, Edit, Details, Categories, CreateCategory, EditCategory

### Fase 4 - Funcionalidades Avanzadas
- [x] Sistema de sincronización
  - [x] SyncQueueItem entity + SyncEnums (SyncOperation, SyncStatus)
  - [x] ISyncQueueRepository + SyncQueueRepository
  - [x] ISyncService + SyncService (EnqueueAsync, ProcessPendingAsync, GetStatusAsync)
  - [x] SyncQueueItemConfiguration (EF Core)
- [x] Idempotencia
  - [x] IdempotencyRecord entity
  - [x] IIdempotencyRepository + IdempotencyRepository
  - [x] IIdempotencyService + IdempotencyService (ExecuteAsync, CleanupExpiredAsync)
  - [x] IdempotencyRecordConfiguration (EF Core)
- [x] Integración con hardware
  - [x] IBarcodeService + BarcodeService (GenerateBarcode, ValidateBarcode, GenerateProductBarcode)
  - [x] ITicketPrinterService + TicketPrinterService (PrintSaleTicket, PrintCashClosing, OpenCashDrawer)
- [x] Reportes y Dashboard
  - [x] IReportService + ReportService
  - [x] DTOs: DashboardDto, SalesReportDto, InventoryReportDto, ProfitabilityReportDto
  - [x] DashboardController (Index, SalesReport, InventoryReport, ProfitabilityReport)
  - [x] Vistas: Dashboard/Index, SalesReport, InventoryReport, ProfitabilityReport

### Fase 5 - Pulido y Produccion
- [x] Alertas automaticas
  - [x] Alert entity + AlertEnums (AlertType, AlertSeverity, AlertStatus)
  - [x] IAlertRepository + AlertRepository
  - [x] IAlertService + AlertService (CheckLowStock, CheckExpiring, CheckDebts, RunAllChecks)
  - [x] AlertsController
  - [x] Vista: Alerts/Index
- [x] Backup automatico
  - [x] IBackupService + BackupService (Create, Restore, Delete, Cleanup)
  - [x] BackupController
  - [x] Vista: Backup/Index
- [x] Logs de auditoria
  - [x] AuditLog entity
  - [x] IAuditLogRepository + AuditLogRepository
  - [x] IAuditService + AuditService
  - [x] AuditController
  - [x] Vistas: Audit/Index, Entity, User
- [x] Seguridad y Autenticacion
  - [x] IAuthService + AuthService (Login, CreateUser, ChangePassword, HashPassword)
  - [x] Autenticacion con Cookies configurada en Program.cs
  - [x] AccountController (Login, Logout, ChangePassword, AccessDenied)
  - [x] Vistas: Account/Login, ChangePassword, AccessDenied
  - [x] UsersController (CRUD de usuarios, solo Admin)
  - [x] Vistas: Users/Index, Create, Edit, ResetPassword
  - [x] Layout actualizado con menu de usuario y roles
- [x] Pruebas unitarias (202 tests pasando)
