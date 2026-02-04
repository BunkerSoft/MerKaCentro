# Arquitectura del Sistema POS MiniMarket

## Información General

| Aspecto | Descripción |
|---------|-------------|
| **Nombre del Proyecto** | POS MiniMarket |
| **Tipo** | Sistema de Punto de Venta para Minimarket |
| **Arquitectura** | Hexagonal |
| **Framework** | .NET 8 MVC |
| **Base de datos servidor** | SQL Server Express |
| **Base de datos cajas** | SQL Server LocalDB |
| **Topología** | Servidor central + cajas independientes |

---

## Alcance Versión 1.0

### Núcleo del Negocio
- Gestión de Productos/Catálogo
- Inventario (con control de vencimientos)
- Punto de Venta (POS)
- Compras/Proveedores
- Kardex
- Ajustes de inventario

### Caja y Finanzas
- Turnos de caja
- Apertura/cierre/arqueo de caja
- Retiros parciales de caja
- Gastos operativos
- Cuentas por cobrar (fiado)

### Ventas
- Múltiples métodos de pago (efectivo, tarjeta, Nequi, transferencia)
- Devoluciones/cambios
- Listas de precios

### Gestión de Productos
- Variantes/presentaciones
- Generación de códigos de barras
- Impresión de etiquetas
- Historial de precios
- Importación masiva (Excel)

### Usuarios
- Módulo administrador
- Módulo cajero
- Auditoría/Logs

### Reportes e Inteligencia
- Dashboard con indicadores clave
- Alertas automáticas (stock, vencimientos)
- Rentabilidad por producto
- Reportes varios

### Técnicos
- Impresión de tickets
- Lector código de barras
- Backup automático
- Modo offline resiliente
- Idempotencia en operaciones críticas

### Clientes
- Gestión de clientes
- Crédito/fiado

---

## Estructura de la Solución

```
MiniMarket.sln
├── MiniMarket.Domain           (Entidades, Value Objects, Puertos)
├── MiniMarket.Application      (Casos de uso, DTOs)
├── MiniMarket.Infrastructure   (Adaptadores: BD, Hardware, Sync)
└── MiniMarket.Web              (MVC: Controllers, Views, ViewModels)
```

---

## Modelo de Dominio

### Agregados Identificados

| Agregado | Raíz | Entidades Internas | Responsabilidad |
|----------|------|-------------------|-----------------|
| **Product** | Product | ProductVariant, PriceHistory | Gestión del catálogo |
| **Inventory** | InventoryItem | InventoryMovement, ExpirationBatch | Stock y vencimientos |
| **Sale** | Sale | SaleItem, Payment | Transacción de venta |
| **Purchase** | Purchase | PurchaseItem | Compras a proveedores |
| **Customer** | Customer | CustomerAccount, AccountMovement | Clientes y crédito |
| **Supplier** | Supplier | - | Proveedores |
| **CashRegister** | CashRegister | CashShift, CashMovement | Turnos y movimientos de caja |
| **User** | User | - | Usuarios del sistema |

### Value Objects

| Value Object | Uso |
|--------------|-----|
| `Money` | Valores monetarios (amount + currency) |
| `Barcode` | Código de barras con validación de formato |
| `Quantity` | Cantidad con unidad de medida (unidad, kg, litro) |
| `DateRange` | Rango de fechas para reportes/consultas |
| `Address` | Dirección de cliente o proveedor |
| `PhoneNumber` | Teléfono con validación |
| `Email` | Email con validación |
| `Percentage` | Para descuentos, impuestos, márgenes |

### Detalle de Agregados

#### Product (Agregado)
```
Product (Raíz)
├── Id
├── Code (Barcode)
├── Name
├── Description
├── Category
├── BasePrice (Money)
├── Cost (Money)
├── TaxRate (Percentage)
├── UnitOfMeasure
├── MinStock
├── IsActive
├── Variants: List<ProductVariant>
└── PriceHistory: List<PriceHistory>

ProductVariant
├── Id
├── Sku
├── Name (ej: "500ml", "1L")
├── Barcode
├── Price (Money)
└── Cost (Money)

PriceHistory
├── Id
├── OldPrice (Money)
├── NewPrice (Money)
├── ChangedAt
└── ChangedBy
```

#### Sale (Agregado)
```
Sale (Raíz)
├── Id
├── Number (consecutivo)
├── Date
├── CustomerId (nullable)
├── UserId (cajero)
├── CashRegisterId
├── Subtotal (Money)
├── TaxTotal (Money)
├── DiscountTotal (Money)
├── Total (Money)
├── Status (Completed, Cancelled, Refunded)
├── Items: List<SaleItem>
└── Payments: List<Payment>

SaleItem
├── Id
├── ProductId
├── ProductVariantId (nullable)
├── ProductName (snapshot)
├── Quantity
├── UnitPrice (Money)
├── Discount (Money)
├── Tax (Money)
└── Subtotal (Money)

Payment
├── Id
├── Method (Cash, Card, Nequi, Transfer)
├── Amount (Money)
├── Reference (nullable)
└── ReceivedAt
```

#### Inventory (Agregado)
```
InventoryItem (Raíz)
├── Id
├── ProductId
├── ProductVariantId (nullable)
├── CurrentStock (Quantity)
├── ReservedStock (Quantity)
├── AvailableStock (calculado)
├── Batches: List<ExpirationBatch>
└── Movements: List<InventoryMovement>

ExpirationBatch
├── Id
├── BatchNumber
├── Quantity
├── ExpirationDate
├── ReceivedDate
└── Status (Available, Expired, Depleted)

InventoryMovement (Kardex)
├── Id
├── Type (Entry, Exit, Adjustment, Transfer, Return)
├── Quantity
├── PreviousStock
├── NewStock
├── Reason
├── ReferenceType (Sale, Purchase, Adjustment, etc)
├── ReferenceId
├── CreatedAt
└── CreatedBy
```

#### CashRegister (Agregado)
```
CashRegister (Raíz)
├── Id
├── Name
├── Location
├── IsActive
└── Shifts: List<CashShift>

CashShift (Turno)
├── Id
├── UserId
├── OpenedAt
├── ClosedAt (nullable)
├── OpeningAmount (Money)
├── ExpectedAmount (Money) (calculado)
├── ActualAmount (Money) (conteo real)
├── Difference (Money) (calculado)
├── Status (Open, Closed)
└── Movements: List<CashMovement>

CashMovement
├── Id
├── Type (Sale, Withdrawal, Deposit, Expense)
├── Amount (Money)
├── Description
├── ReferenceType
├── ReferenceId
└── CreatedAt
```

#### Customer (Agregado)
```
Customer (Raíz)
├── Id
├── DocumentNumber
├── Name
├── Phone (PhoneNumber)
├── Email (Email)
├── Address (Address)
├── CreditLimit (Money)
├── IsActive
└── Account: CustomerAccount

CustomerAccount
├── Id
├── Balance (Money) (lo que debe)
├── Movements: List<AccountMovement>

AccountMovement
├── Id
├── Type (Charge, Payment)
├── Amount (Money)
├── ReferenceType (Sale, Payment)
├── ReferenceId
├── Description
└── CreatedAt
```

---

## Puertos y Adaptadores

### Puertos de Entrada (Inbound / Driving)

#### Productos
```csharp
IProductService
├── CreateProduct(dto): Result<ProductId>
├── UpdateProduct(id, dto): Result
├── DeactivateProduct(id): Result
├── GetById(id): ProductDto
├── GetByBarcode(barcode): ProductDto
├── Search(criteria): PagedList<ProductDto>
├── AddVariant(productId, dto): Result<VariantId>
├── UpdatePrice(productId, newPrice, userId): Result
├── ImportFromExcel(stream): Result<ImportResult>
└── ExportToExcel(criteria): Stream
```

#### Inventario
```csharp
IInventoryService
├── GetStock(productId): StockDto
├── GetStockByBarcode(barcode): StockDto
├── RegisterEntry(dto): Result<MovementId>
├── RegisterAdjustment(dto): Result<MovementId>
├── GetMovements(productId, dateRange): List<MovementDto>
├── GetExpiringProducts(daysAhead): List<ExpirationAlertDto>
├── GetLowStockProducts(): List<LowStockAlertDto>
└── GetKardex(productId, dateRange): KardexReportDto
```

#### Ventas
```csharp
ISaleService
├── CreateSale(dto): Result<SaleId>
├── CancelSale(id, reason): Result
├── RegisterReturn(saleId, items, reason): Result<ReturnId>
├── GetById(id): SaleDto
├── GetByNumber(number): SaleDto
├── GetDailySales(date, cashRegisterId): List<SaleDto>
└── GetSalesReport(criteria): SalesReportDto
```

#### Compras
```csharp
IPurchaseService
├── CreatePurchase(dto): Result<PurchaseId>
├── CancelPurchase(id, reason): Result
├── GetById(id): PurchaseDto
├── GetBySupplier(supplierId, dateRange): List<PurchaseDto>
└── GetPurchaseReport(criteria): PurchaseReportDto
```

#### Clientes
```csharp
ICustomerService
├── CreateCustomer(dto): Result<CustomerId>
├── UpdateCustomer(id, dto): Result
├── DeactivateCustomer(id): Result
├── GetById(id): CustomerDto
├── Search(criteria): PagedList<CustomerDto>
├── GetAccountBalance(customerId): Money
├── RegisterPayment(customerId, amount): Result<PaymentId>
├── GetAccountStatement(customerId, dateRange): AccountStatementDto
└── GetDebtors(): List<DebtorDto>
```

#### Proveedores
```csharp
ISupplierService
├── CreateSupplier(dto): Result<SupplierId>
├── UpdateSupplier(id, dto): Result
├── DeactivateSupplier(id): Result
├── GetById(id): SupplierDto
└── Search(criteria): PagedList<SupplierDto>
```

#### Caja
```csharp
ICashRegisterService
├── OpenShift(cashRegisterId, userId, openingAmount): Result<ShiftId>
├── CloseShift(shiftId, actualAmount): Result<CashClosingDto>
├── RegisterWithdrawal(shiftId, amount, reason): Result<MovementId>
├── RegisterDeposit(shiftId, amount, reason): Result<MovementId>
├── RegisterExpense(shiftId, amount, description): Result<MovementId>
├── GetCurrentShift(cashRegisterId): ShiftDto
├── GetShiftSummary(shiftId): ShiftSummaryDto
└── GetCashReport(dateRange): CashReportDto
```

#### Usuarios
```csharp
IUserService
├── CreateUser(dto): Result<UserId>
├── UpdateUser(id, dto): Result
├── DeactivateUser(id): Result
├── Authenticate(username, password): Result<AuthToken>
├── ChangePassword(userId, oldPassword, newPassword): Result
├── GetById(id): UserDto
└── GetAll(): List<UserDto>
```

#### Reportes
```csharp
IReportService
├── GetDashboard(): DashboardDto
├── GetSalesReport(criteria): SalesReportDto
├── GetInventoryReport(criteria): InventoryReportDto
├── GetProfitabilityReport(criteria): ProfitabilityReportDto
├── GetExpirationReport(daysAhead): ExpirationReportDto
└── GetCustomerDebtReport(): DebtReportDto
```

### Puertos de Salida (Outbound / Driven)

#### Persistencia
```csharp
IProductRepository
├── Add(product): void
├── Update(product): void
├── GetById(id): Product
├── GetByBarcode(barcode): Product
├── Search(specification): PagedList<Product>
└── Exists(barcode): bool

IInventoryRepository
├── Add(item): void
├── Update(item): void
├── GetByProductId(productId): InventoryItem
├── GetExpiringBefore(date): List<InventoryItem>
└── GetBelowMinStock(): List<InventoryItem>

ISaleRepository
├── Add(sale): void
├── Update(sale): void
├── GetById(id): Sale
├── GetByNumber(number): Sale
├── GetByDateRange(range, cashRegisterId): List<Sale>
└── GetNextNumber(): string

IPurchaseRepository
├── Add(purchase): void
├── Update(purchase): void
├── GetById(id): Purchase
└── GetBySupplier(supplierId, dateRange): List<Purchase>

ICustomerRepository
├── Add(customer): void
├── Update(customer): void
├── GetById(id): Customer
├── Search(specification): PagedList<Customer>
└── GetWithPositiveBalance(): List<Customer>

ISupplierRepository
├── Add(supplier): void
├── Update(supplier): void
├── GetById(id): Supplier
└── Search(specification): PagedList<Supplier>

ICashRegisterRepository
├── Add(cashRegister): void
├── Update(cashRegister): void
├── GetById(id): CashRegister
├── GetOpenShift(cashRegisterId): CashShift
└── GetAll(): List<CashRegister>

IUserRepository
├── Add(user): void
├── Update(user): void
├── GetById(id): User
├── GetByUsername(username): User
└── GetAll(): List<User>
```

#### Unidad de Trabajo
```csharp
IUnitOfWork
├── BeginTransaction(): void
├── Commit(): void
├── Rollback(): void
└── SaveChanges(): void
```

#### Hardware
```csharp
ITicketPrinter
├── Print(ticket): Result
├── OpenCashDrawer(): Result
└── IsAvailable(): bool

IBarcodeScanner
├── OnBarcodeScanned: Event<string>
├── StartListening(): void
└── StopListening(): void

IBarcodeGenerator
├── Generate(code, format): byte[]
└── GenerateForProduct(product): byte[]

ILabelPrinter
├── PrintLabel(labelData): Result
└── PrintBatch(labels): Result
```

#### Servicios Externos
```csharp
IBackupService
├── CreateBackup(): Result<string>
├── RestoreBackup(path): Result
├── GetBackupHistory(): List<BackupInfo>
└── ScheduleAutoBackup(config): void

ISyncService
├── PushChanges(): Task<SyncResult>
├── PullChanges(): Task<SyncResult>
├── FullSync(): Task<SyncResult>
├── GetSyncStatus(): SyncStatus
└── OnConnectionChanged: Event<bool>

IAlertService
├── SendAlert(alert): void
├── GetPendingAlerts(): List<Alert>
└── DismissAlert(alertId): void

IExcelService
├── Import<T>(stream): List<T>
└── Export<T>(data): Stream

IIdempotencyService
├── ExecuteAsync<T>(idempotencyKey, operationType, operation): Task<IdempotencyResult<T>>
├── ExistsAsync(idempotencyKey): Task<bool>
├── GetStoredResultAsync<T>(idempotencyKey): Task<T>
└── CleanExpiredAsync(): Task
```

### Adaptadores (Implementaciones)

| Puerto | Adaptador |
|--------|-----------|
| `IProductRepository` | `SqlServerProductRepository` |
| `ISaleRepository` | `SqlServerSaleRepository` |
| `ITicketPrinter` | `EscPosPrinter` (impresoras térmicas) |
| `IBarcodeScanner` | `SerialBarcodeScanner` o `HidBarcodeScanner` |
| `IBarcodeGenerator` | `ZXingBarcodeGenerator` |
| `IBackupService` | `SqlServerBackupService` |
| `ISyncService` | `SignalRSyncService` o `HttpSyncService` |
| `IExcelService` | `EpPlusExcelService` |
| `IIdempotencyService` | `SqlServerIdempotencyService` |

---

## Modelo de Datos

### Productos y Catálogo

```sql
Categories
├── Id                  UNIQUEIDENTIFIER PK
├── Name                NVARCHAR(100) NOT NULL
├── Description         NVARCHAR(500)
├── ParentCategoryId    UNIQUEIDENTIFIER FK (self-reference)
├── IsActive            BIT DEFAULT 1
├── CreatedAt           DATETIME2
└── UpdatedAt           DATETIME2

Products
├── Id                  UNIQUEIDENTIFIER PK
├── Code                NVARCHAR(50) UNIQUE NOT NULL
├── Barcode             NVARCHAR(50) UNIQUE
├── Name                NVARCHAR(200) NOT NULL
├── Description         NVARCHAR(1000)
├── CategoryId          UNIQUEIDENTIFIER FK
├── BasePrice           DECIMAL(18,2) NOT NULL
├── Cost                DECIMAL(18,2) NOT NULL
├── TaxRate             DECIMAL(5,2) DEFAULT 0
├── UnitOfMeasure       NVARCHAR(20) NOT NULL (Unit, Kg, Liter)
├── MinStock            DECIMAL(18,3) DEFAULT 0
├── IsActive            BIT DEFAULT 1
├── CreatedAt           DATETIME2
├── UpdatedAt           DATETIME2
└── CreatedBy           UNIQUEIDENTIFIER FK

ProductVariants
├── Id                  UNIQUEIDENTIFIER PK
├── ProductId           UNIQUEIDENTIFIER FK NOT NULL
├── Sku                 NVARCHAR(50) UNIQUE
├── Barcode             NVARCHAR(50) UNIQUE
├── Name                NVARCHAR(100) NOT NULL
├── Price               DECIMAL(18,2) NOT NULL
├── Cost                DECIMAL(18,2) NOT NULL
├── IsActive            BIT DEFAULT 1
├── CreatedAt           DATETIME2
└── UpdatedAt           DATETIME2

PriceHistory
├── Id                  UNIQUEIDENTIFIER PK
├── ProductId           UNIQUEIDENTIFIER FK NOT NULL
├── ProductVariantId    UNIQUEIDENTIFIER FK (nullable)
├── OldPrice            DECIMAL(18,2) NOT NULL
├── NewPrice            DECIMAL(18,2) NOT NULL
├── ChangedAt           DATETIME2 NOT NULL
└── ChangedBy           UNIQUEIDENTIFIER FK

PriceLists
├── Id                  UNIQUEIDENTIFIER PK
├── Name                NVARCHAR(100) NOT NULL
├── Description         NVARCHAR(500)
├── IsDefault           BIT DEFAULT 0
├── IsActive            BIT DEFAULT 1
├── CreatedAt           DATETIME2
└── UpdatedAt           DATETIME2

PriceListItems
├── Id                  UNIQUEIDENTIFIER PK
├── PriceListId         UNIQUEIDENTIFIER FK NOT NULL
├── ProductId           UNIQUEIDENTIFIER FK NOT NULL
├── ProductVariantId    UNIQUEIDENTIFIER FK (nullable)
├── Price               DECIMAL(18,2) NOT NULL
├── CreatedAt           DATETIME2
└── UpdatedAt           DATETIME2
```

### Inventario

```sql
InventoryItems
├── Id                  UNIQUEIDENTIFIER PK
├── ProductId           UNIQUEIDENTIFIER FK NOT NULL
├── ProductVariantId    UNIQUEIDENTIFIER FK (nullable)
├── CurrentStock        DECIMAL(18,3) NOT NULL DEFAULT 0
├── ReservedStock       DECIMAL(18,3) NOT NULL DEFAULT 0
├── CreatedAt           DATETIME2
└── UpdatedAt           DATETIME2

ExpirationBatches
├── Id                  UNIQUEIDENTIFIER PK
├── InventoryItemId     UNIQUEIDENTIFIER FK NOT NULL
├── BatchNumber         NVARCHAR(50)
├── Quantity            DECIMAL(18,3) NOT NULL
├── ExpirationDate      DATE NOT NULL
├── ReceivedDate        DATE NOT NULL
├── Status              NVARCHAR(20) NOT NULL (Available, Expired, Depleted)
├── CreatedAt           DATETIME2
└── UpdatedAt           DATETIME2

InventoryMovements (Kardex)
├── Id                  UNIQUEIDENTIFIER PK
├── InventoryItemId     UNIQUEIDENTIFIER FK NOT NULL
├── Type                NVARCHAR(20) NOT NULL (Entry, Exit, Adjustment, Return)
├── Quantity            DECIMAL(18,3) NOT NULL
├── PreviousStock       DECIMAL(18,3) NOT NULL
├── NewStock            DECIMAL(18,3) NOT NULL
├── Reason              NVARCHAR(500)
├── ReferenceType       NVARCHAR(50) (Sale, Purchase, Adjustment)
├── ReferenceId         UNIQUEIDENTIFIER
├── CreatedAt           DATETIME2 NOT NULL
└── CreatedBy           UNIQUEIDENTIFIER FK
```

### Ventas

```sql
Sales
├── Id                  UNIQUEIDENTIFIER PK
├── Number              NVARCHAR(20) UNIQUE NOT NULL
├── Date                DATETIME2 NOT NULL
├── CustomerId          UNIQUEIDENTIFIER FK (nullable)
├── UserId              UNIQUEIDENTIFIER FK NOT NULL
├── CashRegisterId      UNIQUEIDENTIFIER FK NOT NULL
├── ShiftId             UNIQUEIDENTIFIER FK NOT NULL
├── Subtotal            DECIMAL(18,2) NOT NULL
├── TaxTotal            DECIMAL(18,2) NOT NULL
├── DiscountTotal       DECIMAL(18,2) NOT NULL DEFAULT 0
├── Total               DECIMAL(18,2) NOT NULL
├── Status              NVARCHAR(20) NOT NULL (Completed, Cancelled, Refunded)
├── Notes               NVARCHAR(1000)
├── CreatedAt           DATETIME2
└── UpdatedAt           DATETIME2

SaleItems
├── Id                  UNIQUEIDENTIFIER PK
├── SaleId              UNIQUEIDENTIFIER FK NOT NULL
├── ProductId           UNIQUEIDENTIFIER FK NOT NULL
├── ProductVariantId    UNIQUEIDENTIFIER FK (nullable)
├── ProductName         NVARCHAR(200) NOT NULL (snapshot)
├── Barcode             NVARCHAR(50)
├── Quantity            DECIMAL(18,3) NOT NULL
├── UnitPrice           DECIMAL(18,2) NOT NULL
├── Discount            DECIMAL(18,2) DEFAULT 0
├── TaxRate             DECIMAL(5,2) DEFAULT 0
├── TaxAmount           DECIMAL(18,2) DEFAULT 0
├── Subtotal            DECIMAL(18,2) NOT NULL
└── CreatedAt           DATETIME2

Payments
├── Id                  UNIQUEIDENTIFIER PK
├── SaleId              UNIQUEIDENTIFIER FK NOT NULL
├── Method              NVARCHAR(20) NOT NULL (Cash, Card, Nequi, Transfer)
├── Amount              DECIMAL(18,2) NOT NULL
├── Reference           NVARCHAR(100)
├── ReceivedAt          DATETIME2 NOT NULL
└── CreatedAt           DATETIME2

Returns
├── Id                  UNIQUEIDENTIFIER PK
├── Number              NVARCHAR(20) UNIQUE NOT NULL
├── OriginalSaleId      UNIQUEIDENTIFIER FK NOT NULL
├── Date                DATETIME2 NOT NULL
├── UserId              UNIQUEIDENTIFIER FK NOT NULL
├── Reason              NVARCHAR(500) NOT NULL
├── Total               DECIMAL(18,2) NOT NULL
├── Status              NVARCHAR(20) NOT NULL (Pending, Completed)
├── CreatedAt           DATETIME2
└── UpdatedAt           DATETIME2

ReturnItems
├── Id                  UNIQUEIDENTIFIER PK
├── ReturnId            UNIQUEIDENTIFIER FK NOT NULL
├── SaleItemId          UNIQUEIDENTIFIER FK NOT NULL
├── Quantity            DECIMAL(18,3) NOT NULL
├── Amount              DECIMAL(18,2) NOT NULL
└── CreatedAt           DATETIME2
```

### Compras

```sql
Suppliers
├── Id                  UNIQUEIDENTIFIER PK
├── DocumentNumber      NVARCHAR(20)
├── Name                NVARCHAR(200) NOT NULL
├── ContactName         NVARCHAR(200)
├── Phone               NVARCHAR(20)
├── Email               NVARCHAR(100)
├── Address             NVARCHAR(500)
├── IsActive            BIT DEFAULT 1
├── CreatedAt           DATETIME2
└── UpdatedAt           DATETIME2

Purchases
├── Id                  UNIQUEIDENTIFIER PK
├── Number              NVARCHAR(20) UNIQUE NOT NULL
├── SupplierId          UNIQUEIDENTIFIER FK NOT NULL
├── Date                DATETIME2 NOT NULL
├── UserId              UNIQUEIDENTIFIER FK NOT NULL
├── Subtotal            DECIMAL(18,2) NOT NULL
├── TaxTotal            DECIMAL(18,2) NOT NULL
├── Total               DECIMAL(18,2) NOT NULL
├── Status              NVARCHAR(20) NOT NULL (Pending, Received, Cancelled)
├── Notes               NVARCHAR(1000)
├── CreatedAt           DATETIME2
└── UpdatedAt           DATETIME2

PurchaseItems
├── Id                  UNIQUEIDENTIFIER PK
├── PurchaseId          UNIQUEIDENTIFIER FK NOT NULL
├── ProductId           UNIQUEIDENTIFIER FK NOT NULL
├── ProductVariantId    UNIQUEIDENTIFIER FK (nullable)
├── Quantity            DECIMAL(18,3) NOT NULL
├── UnitCost            DECIMAL(18,2) NOT NULL
├── TaxRate             DECIMAL(5,2) DEFAULT 0
├── TaxAmount           DECIMAL(18,2) DEFAULT 0
├── Subtotal            DECIMAL(18,2) NOT NULL
├── ExpirationDate      DATE (nullable)
├── BatchNumber         NVARCHAR(50)
└── CreatedAt           DATETIME2
```

### Clientes

```sql
Customers
├── Id                  UNIQUEIDENTIFIER PK
├── DocumentNumber      NVARCHAR(20)
├── Name                NVARCHAR(200) NOT NULL
├── Phone               NVARCHAR(20)
├── Email               NVARCHAR(100)
├── Address             NVARCHAR(500)
├── CreditLimit         DECIMAL(18,2) DEFAULT 0
├── IsActive            BIT DEFAULT 1
├── CreatedAt           DATETIME2
└── UpdatedAt           DATETIME2

CustomerAccounts
├── Id                  UNIQUEIDENTIFIER PK
├── CustomerId          UNIQUEIDENTIFIER FK UNIQUE NOT NULL
├── Balance             DECIMAL(18,2) NOT NULL DEFAULT 0
├── CreatedAt           DATETIME2
└── UpdatedAt           DATETIME2

AccountMovements
├── Id                  UNIQUEIDENTIFIER PK
├── CustomerAccountId   UNIQUEIDENTIFIER FK NOT NULL
├── Type                NVARCHAR(20) NOT NULL (Charge, Payment)
├── Amount              DECIMAL(18,2) NOT NULL
├── ReferenceType       NVARCHAR(50) (Sale, Payment)
├── ReferenceId         UNIQUEIDENTIFIER
├── Description         NVARCHAR(500)
├── CreatedAt           DATETIME2 NOT NULL
└── CreatedBy           UNIQUEIDENTIFIER FK
```

### Caja

```sql
CashRegisters
├── Id                  UNIQUEIDENTIFIER PK
├── Name                NVARCHAR(100) NOT NULL
├── Location            NVARCHAR(200)
├── IsActive            BIT DEFAULT 1
├── CreatedAt           DATETIME2
└── UpdatedAt           DATETIME2

CashShifts
├── Id                  UNIQUEIDENTIFIER PK
├── CashRegisterId      UNIQUEIDENTIFIER FK NOT NULL
├── UserId              UNIQUEIDENTIFIER FK NOT NULL
├── OpenedAt            DATETIME2 NOT NULL
├── ClosedAt            DATETIME2 (nullable)
├── OpeningAmount       DECIMAL(18,2) NOT NULL
├── ExpectedAmount      DECIMAL(18,2) (calculado al cerrar)
├── ActualAmount        DECIMAL(18,2) (nullable, conteo real)
├── Difference          DECIMAL(18,2) (nullable)
├── Status              NVARCHAR(20) NOT NULL (Open, Closed)
├── Notes               NVARCHAR(1000)
├── CreatedAt           DATETIME2
└── UpdatedAt           DATETIME2

CashMovements
├── Id                  UNIQUEIDENTIFIER PK
├── ShiftId             UNIQUEIDENTIFIER FK NOT NULL
├── Type                NVARCHAR(20) NOT NULL (Sale, Withdrawal, Deposit, Expense)
├── Amount              DECIMAL(18,2) NOT NULL
├── Description         NVARCHAR(500)
├── ReferenceType       NVARCHAR(50)
├── ReferenceId         UNIQUEIDENTIFIER
├── CreatedAt           DATETIME2 NOT NULL
└── CreatedBy           UNIQUEIDENTIFIER FK
```

### Gastos

```sql
ExpenseCategories
├── Id                  UNIQUEIDENTIFIER PK
├── Name                NVARCHAR(100) NOT NULL
├── Description         NVARCHAR(500)
├── IsActive            BIT DEFAULT 1
├── CreatedAt           DATETIME2
└── UpdatedAt           DATETIME2

Expenses
├── Id                  UNIQUEIDENTIFIER PK
├── CategoryId          UNIQUEIDENTIFIER FK NOT NULL
├── ShiftId             UNIQUEIDENTIFIER FK (nullable)
├── Amount              DECIMAL(18,2) NOT NULL
├── Description         NVARCHAR(500) NOT NULL
├── Date                DATETIME2 NOT NULL
├── UserId              UNIQUEIDENTIFIER FK NOT NULL
├── CreatedAt           DATETIME2
└── UpdatedAt           DATETIME2
```

### Usuarios y Seguridad

```sql
Roles
├── Id                  UNIQUEIDENTIFIER PK
├── Name                NVARCHAR(50) NOT NULL UNIQUE
├── Description         NVARCHAR(200)
├── CreatedAt           DATETIME2
└── UpdatedAt           DATETIME2

Permissions
├── Id                  UNIQUEIDENTIFIER PK
├── Code                NVARCHAR(100) NOT NULL UNIQUE
├── Name                NVARCHAR(100) NOT NULL
├── Module              NVARCHAR(50) NOT NULL
└── CreatedAt           DATETIME2

RolePermissions
├── RoleId              UNIQUEIDENTIFIER FK PK
└── PermissionId        UNIQUEIDENTIFIER FK PK

Users
├── Id                  UNIQUEIDENTIFIER PK
├── Username            NVARCHAR(50) UNIQUE NOT NULL
├── PasswordHash        NVARCHAR(500) NOT NULL
├── FullName            NVARCHAR(200) NOT NULL
├── Email               NVARCHAR(100)
├── RoleId              UNIQUEIDENTIFIER FK NOT NULL
├── IsActive            BIT DEFAULT 1
├── LastLoginAt         DATETIME2
├── CreatedAt           DATETIME2
└── UpdatedAt           DATETIME2

AuditLogs
├── Id                  UNIQUEIDENTIFIER PK
├── UserId              UNIQUEIDENTIFIER FK
├── Action              NVARCHAR(100) NOT NULL
├── EntityType          NVARCHAR(100) NOT NULL
├── EntityId            NVARCHAR(50)
├── OldValues           NVARCHAR(MAX)
├── NewValues           NVARCHAR(MAX)
├── IpAddress           NVARCHAR(50)
└── CreatedAt           DATETIME2 NOT NULL
```

### Sincronización

```sql
SyncQueue
├── Id                  UNIQUEIDENTIFIER PK
├── SyncId              UNIQUEIDENTIFIER NOT NULL (IdempotencyKey)
├── EntityType          NVARCHAR(100) NOT NULL
├── EntityId            UNIQUEIDENTIFIER NOT NULL
├── Operation           NVARCHAR(20) NOT NULL (Insert, Update, Delete)
├── Payload             NVARCHAR(MAX) NOT NULL
├── Status              NVARCHAR(20) NOT NULL (Pending, Synced, Failed)
├── Attempts            INT DEFAULT 0
├── LastAttemptAt       DATETIME2
├── ErrorMessage        NVARCHAR(1000)
├── CreatedAt           DATETIME2 NOT NULL
└── SyncedAt            DATETIME2

SyncLog
├── Id                  UNIQUEIDENTIFIER PK
├── Direction           NVARCHAR(10) NOT NULL (Push, Pull)
├── StartedAt           DATETIME2 NOT NULL
├── CompletedAt         DATETIME2
├── Status              NVARCHAR(20) NOT NULL (InProgress, Completed, Failed)
├── RecordsProcessed    INT DEFAULT 0
├── ErrorMessage        NVARCHAR(MAX)
└── CreatedAt           DATETIME2

NodeRegistry (en servidor)
├── Id                  UNIQUEIDENTIFIER PK
├── NodeCode            NVARCHAR(50) UNIQUE NOT NULL (CAJA-01, CAJA-02)
├── Name                NVARCHAR(100)
├── LastSyncAt          DATETIME2
├── LastPushAt          DATETIME2
├── LastPullAt          DATETIME2
├── IsOnline            BIT
├── IpAddress           NVARCHAR(50)
├── CreatedAt           DATETIME2
└── UpdatedAt           DATETIME2

SyncCheckpoint (en cada caja)
├── Id                  UNIQUEIDENTIFIER PK
├── EntityType          NVARCHAR(100) NOT NULL
├── LastSyncVersion     BIGINT NOT NULL DEFAULT 0
├── LastSyncAt          DATETIME2
└── UpdatedAt           DATETIME2
```

### Idempotencia

```sql
IdempotencyRecords
├── Id                  UNIQUEIDENTIFIER PK
├── IdempotencyKey      UNIQUEIDENTIFIER UNIQUE NOT NULL
├── OperationType       NVARCHAR(100) NOT NULL
├── RequestHash         NVARCHAR(64)
├── ResponseData        NVARCHAR(MAX)
├── StatusCode          INT
├── ProcessedAt         DATETIME2 NOT NULL
├── ExpiresAt           DATETIME2 NOT NULL
└── CreatedAt           DATETIME2
```

### Alertas

```sql
Alerts
├── Id                  UNIQUEIDENTIFIER PK
├── Type                NVARCHAR(50) NOT NULL (LowStock, Expiration, Debt)
├── Severity            NVARCHAR(20) NOT NULL (Info, Warning, Critical)
├── Title               NVARCHAR(200) NOT NULL
├── Message             NVARCHAR(1000) NOT NULL
├── EntityType          NVARCHAR(100)
├── EntityId            UNIQUEIDENTIFIER
├── IsRead              BIT DEFAULT 0
├── IsDismissed         BIT DEFAULT 0
├── CreatedAt           DATETIME2 NOT NULL
└── DismissedAt         DATETIME2
```

### Configuración

```sql
Settings
├── Id                  UNIQUEIDENTIFIER PK
├── Key                 NVARCHAR(100) UNIQUE NOT NULL
├── Value               NVARCHAR(MAX)
├── Description         NVARCHAR(500)
├── CreatedAt           DATETIME2
└── UpdatedAt           DATETIME2

Consecutives
├── Id                  UNIQUEIDENTIFIER PK
├── Type                NVARCHAR(50) UNIQUE NOT NULL (Sale, Purchase, Return)
├── Prefix              NVARCHAR(10)
├── CurrentNumber       INT NOT NULL DEFAULT 0
├── Format              NVARCHAR(50)
└── UpdatedAt           DATETIME2
```

### Índices Recomendados

```sql
Products: IX_Products_Barcode, IX_Products_CategoryId, IX_Products_IsActive
Sales: IX_Sales_Date, IX_Sales_CustomerId, IX_Sales_Number, IX_Sales_ShiftId
InventoryMovements: IX_InventoryMovements_InventoryItemId_CreatedAt
ExpirationBatches: IX_ExpirationBatches_ExpirationDate_Status
SyncQueue: IX_SyncQueue_Status_CreatedAt
AuditLogs: IX_AuditLogs_EntityType_EntityId, IX_AuditLogs_CreatedAt
IdempotencyRecords: IX_IdempotencyRecords_IdempotencyKey
```

---

## Mecanismo de Sincronización

### Topología

```
┌─────────────────┐         ┌─────────────────┐
│   SERVIDOR      │◄───────►│     CAJA 1      │
│   (SQL Server   │         │   (LocalDB)     │
│    Express)     │◄───────►├─────────────────┤
│                 │         │     CAJA 2      │
│   - Catálogo    │         │   (LocalDB)     │
│   - Consolidado │◄───────►├─────────────────┤
│   - Reportes    │         │     CAJA 3      │
│                 │         │   (LocalDB)     │
└─────────────────┘         └─────────────────┘
```

### Principios de la Sincronización

| Principio | Descripción |
|-----------|-------------|
| Offline-first | La caja siempre puede operar sin conexión |
| Eventual consistency | Los datos se sincronizan cuando hay conexión |
| Servidor como fuente de verdad | Para catálogo, precios, clientes |
| Cola de transacciones | Las ventas se encolan y suben cuando hay conexión |
| Detección automática | El sistema detecta conexión/desconexión |
| Reintentos automáticos | Si falla, reintenta con backoff exponencial |

### Flujo de Datos

**Servidor → Cajas (PULL):**
- Productos
- Categorías
- Variantes
- Listas de precios
- Clientes
- Proveedores
- Usuarios
- Configuración
- Permisos

**Cajas → Servidor (PUSH):**
- Ventas
- Pagos
- Devoluciones
- Movimientos de caja
- Turnos
- Gastos
- Movimientos de inventario
- Movimientos de cuenta cliente
- Logs de auditoría

### Resolución de Conflictos

| Escenario | Resolución |
|-----------|------------|
| Mismo producto editado en servidor y caja | Servidor gana (fuente de verdad para catálogo) |
| Venta duplicada | Detectar por SyncId/IdempotencyKey, ignorar duplicado |
| Stock negativo | Permitir, generar alerta en servidor |
| Cliente editado en ambos lados | Servidor gana, caja recibe actualización |

### API de Sincronización (Servidor)

```
POST /api/sync/push
Body: { 
    nodeCode: "CAJA-01",
    entities: [
        { type: "Sale", operation: "Insert", data: {...}, syncId: "..." },
        { type: "CashMovement", operation: "Insert", data: {...}, syncId: "..." }
    ]
}
Response: {
    success: true,
    processed: 15,
    failed: 0,
    errors: []
}

GET /api/sync/pull?nodeCode=CAJA-01&since=2024-01-15T10:30:00
Response: {
    products: [...],
    customers: [...],
    priceLists: [...],
    settings: [...],
    serverTimestamp: "2024-01-15T12:00:00"
}

GET /api/sync/status?nodeCode=CAJA-01
Response: {
    isOnline: true,
    pendingPush: 0,
    lastPushAt: "...",
    lastPullAt: "..."
}
```

---

## Idempotencia

### Operaciones Protegidas

- Crear venta
- Registrar pago
- Registrar devolución
- Movimiento de inventario
- Movimiento de caja
- Abono a cuenta cliente
- Registrar gasto
- Todas las operaciones de sincronización (push)

### Implementación

```csharp
public interface IIdempotencyService
{
    Task<IdempotencyResult<T>> ExecuteAsync<T>(
        Guid idempotencyKey,
        string operationType,
        Func<Task<T>> operation
    );
    
    Task<bool> ExistsAsync(Guid idempotencyKey);
    Task<T> GetStoredResultAsync<T>(Guid idempotencyKey);
    Task CleanExpiredAsync();
}
```

### Flujo

```
1. ¿Existe IdempotencyKey en registro?
   │
   ├── SÍ → Retornar respuesta almacenada (no procesar de nuevo)
   │
   └── NO → Procesar operación, guardar resultado con IdempotencyKey
```

---

## Diseño de Interfaz

### Especificaciones

| Aspecto | Valor |
|---------|-------|
| Estilo | Limpio y profesional |
| Tema | Claro (sin tema oscuro) |
| Idioma | Español (sin multiidioma) |
| Colores base | Rojo, Amarillo, Verde |

### Paleta de Colores

```css
:root {
    /* Colores principales */
    --color-primary: #DC3545;        /* Rojo */
    --color-secondary: #FFC107;      /* Amarillo */
    --color-success: #28A745;        /* Verde */
    
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
    --color-text-muted: #ADB5BD;
    
    /* Estados */
    --color-error: #DC3545;
    --color-warning: #FFC107;
    --color-info: #17A2B8;
    
    /* Sombras */
    --shadow-sm: 0 1px 2px rgba(0, 0, 0, 0.05);
    --shadow-md: 0 4px 6px rgba(0, 0, 0, 0.1);
    --shadow-lg: 0 10px 15px rgba(0, 0, 0, 0.1);
}
```

### Uso de Colores por Contexto

| Contexto | Color |
|----------|-------|
| Botones primarios / Acciones principales | Rojo (#DC3545) |
| Alertas / Advertencias / Precios | Amarillo (#FFC107) |
| Confirmaciones / Stock OK / Éxito | Verde (#28A745) |
| Fondos | Gris claro (#F8F9FA) |
| Tarjetas / Superficies | Blanco (#FFFFFF) |
| Textos principales | Gris oscuro (#212529) |
| Textos secundarios | Gris (#6C757D) |

### Roles de Usuario

| Rol | Módulos Accesibles |
|-----|-------------------|
| **Administrador** | Todos los módulos |
| **Cajero** | POS, Consulta de productos, Turno de caja |

---

## Funcionalidades Pendientes (V2)

- Apartados/Reservas
- Cotizaciones
- Traslados internos (bodega → venta)
- Unidades de conversión
- Programa de fidelización (puntos)
- Historial de compras por cliente
- Cierre de período
- Notas de crédito
- Comisiones por vendedor
- Cuentas por pagar (proveedores)
- Conciliación de pagos electrónicos

---

## Resumen Técnico

| Aspecto | Decisión |
|---------|----------|
| Arquitectura | Hexagonal |
| Framework | .NET 8 MVC |
| Base de datos servidor | SQL Server Express |
| Base de datos cajas | SQL Server LocalDB |
| Estructura solución | Múltiples proyectos por capa |
| Topología | Servidor central + cajas independientes |
| Sincronización | Tiempo real automática |
| Conflictos inventario | Permitir negativos + alerta |
| Idempotencia | Sí, en operaciones críticas y sincronización |
| Interfaz | Limpia, profesional, colores claros (rojo, amarillo, verde) |
