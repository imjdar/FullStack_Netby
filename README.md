# FullStack Inventory Management System

Sistema integral de inventario corporativo basado en una arquitectura robusta de microservicios. Desarrollado con **.NET 9**, **React (Vite)** y **SQL Server**.

## Arquitectura del Proyecto

El sistema está separado en capas lógicas independientes, permitiendo escalabilidad y tolerancia a fallos.

```mermaid
graph TD;
    Client[Navegador (React)] -->|HTTP Request| Frontend[Frontend Container :3000];
    Frontend -->|API calls| ProductsAPI[Products API :8080];
    Frontend -->|API calls| TransactionsAPI[Transactions API :8082];
    
    ProductsAPI -->|Lectura/Escritura| ProductsDB[(Products DB :1434)];
    TransactionsAPI -->|Lectura/Escritura| TransactionsDB[(Transactions DB :1435)];
    
    TransactionsAPI -->|Valida Stock| ProductsAPI;
```

### Tecnologías Utama:
- **Backend:** C# .NET 9 ASP.NET Core, Entity Framework Core 9.
- **Frontend:** React 18, TypeScript, Vite, React Router, Tailwind-like UI via CSS modules.
- **Base de Datos:** Microsoft SQL Server (Múltiples bases de datos).
- **Contenedores:** Docker, Docker Compose.
- **Pruebas:** xUnit, Moq.

---

## Guía Rápida de Despliegue (Producción Local)

El proyecto está dockerizado para asegurar que se ejecute en cualquier lugar idénticamente. 

### 1. Requisitos Previos
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) iniciado.
- [Git](https://git-scm.com/downloads) instalado.
- Terminal compatible con `bash` o `powershell`.

### 2. Configurar Entorno
Clona el repositorio y establece las variables de entorno necesarias:

```bash
git clone https://github.com/imjdar/FullStack_Netby.git
cd FullStack_Netby

# Copia el archivo de ejemplo para crear tu archivo de configuración de producción
cp .env.example .env
```
*(Nota: El archivo `.env` está en `.gitignore` protegiendo las credenciales reales de ser subidas al repo).*

### 3. Levantar Contenedores

Levanta los contenedores en segundo plano. Esto descargará las imágenes de SQL Server, .NET y construirá el frontend.

```bash
docker-compose up -d --build
```

### 4. Inicializar Bases de Datos (Semilla de Producción)

Para garantizar consistencia y evitar conflictos de Entity Framework, la inicialización de tablas y datos semilla se realiza mediante scripts SQL externos:

Dependiendo de tu sistema operativo, ejecuta el script de inicialización:

- **Windows (PowerShell):**
  ```powershell
  # Importante: Permitir ejecución si está bloqueada u omitir con Bypass
  powershell -ExecutionPolicy Bypass -File ./setup-db.ps1
  ```

- **Linux/Mac (Bash) o WSL:**
  ```bash
  chmod +x ./setup-db.sh
  ./setup-db.sh
  ```

*Este script esperar a que SQL Server arranque y ejecutará `sqlcmd` para crear el catálogo real (Laptops, Accesorios) y su historial.*

---

## Desarrollo Local (Modo Standalone)

Si deseas modificar código y debuggear sin Docker, puedes ejecutar los servicios localmente. Solo asegúrate de tener una base de datos SQL Server levantada (por ejemplo, con Docker solo para la BD) o usar LocalDB, cambiando las cadenas de conexión en tus `appsettings.Development.json`.

```bash
# Terminal 1: Products API
cd Inventory.Products.Api
dotnet run

# Terminal 2: Transactions API
cd Inventory.Transactions.Api
dotnet run

# Terminal 3: Frontend Web
cd Inventory.Frontend.React
npm install
npm run dev
```

---

## Buenas Prácticas y Decisiones Técnicas Implementadas

1. **Microservicios Aislados:** Las bases de datos están separadas lógicamente (`ProductsDB` y `TransactionsDB`). Los servicios se comunican a través de HTTP Requests controladas (Patrón de Mensajería HTTP).
2. **Validaciones de Integridad (Backend & Frontend):** 
   - Control estricto de **Stock Negativo** a nivel API y control predictivo en el **Frontend**.
   - Validaciones de inputs no negativos (Cantidad, Precios).
   - Rollback transaccional ante fallos de red inter-microservicios.
3. **Independencia de Migraciones (EF Core):** Se eliminó la aplicación automática de migraciones (`Database.Migrate()`) en el startup del contenedor, reemplazándolo por scripts SQL puros que previenen crash-loops de contenedores y race conditions al levantar Docker Compose.
4. **Diseño Frontend Clean UI:** Interfaz construida desde cero con CSS puro inspirado en sistemas de diseño como Vercel/Linear. Sin librerías pesadas, 100% responsivo y rápido.
5. **Separación de Responsabilidades (REST API):** Uso de DTOs para evitar exponer el modelo de dominio. Capa `IProductService` y `ITransactionService` aislando controladores de lógica compleja.
6. **Seguridad Básica:** Centralización de contraseñas de BD y secretos usando archivo `.env` inyectado por Docker.

---

## Pruebas Unitarias

El proyecto incluye dos suites de pruebas para garantizar la calidad en la lógica de negocio simulando persistencia de datos con **Moq**:

```bash
# Correr todas las pruebas en la solución
dotnet test

# O individualmente
dotnet test Inventory.Products.Tests
dotnet test Inventory.Transactions.Tests
```
---
*Proyecto Fullstack Inventory System - 2026*
