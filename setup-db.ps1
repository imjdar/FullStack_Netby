# Script de PowerShell para inicializar las bases de datos en Docker
Write-Host "Iniciando inicialización de bases de datos..." -ForegroundColor Cyan

# Esperar a que los contenedores estén listos
Write-Host "Esperando a que SQL Server inicie (15s)..."
Start-Sleep -Seconds 15

# Inicializar Base de Datos de Productos
Write-Host "Configurando ProductsDB..." -ForegroundColor Yellow
docker exec -i products-db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P TuPasswordPro2026 -C -d ProductsDB -i /sql/products-init.sql

# Inicializar Base de Datos de Transacciones
Write-Host "Configurando TransactionsDB..." -ForegroundColor Yellow
docker exec -i transactions-db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P TuPasswordPro2026 -C -d TransactionsDB -i /sql/transactions-init.sql

Write-Host "¡Inicialización completada con éxito!" -ForegroundColor Green
