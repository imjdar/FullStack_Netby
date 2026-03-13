#!/bin/bash

# Script para inicializar las bases de datos en Docker
echo "Iniciando inicialización de bases de datos..."

# Esperar a que los contenedores estén listos
echo "Esperando a que SQL Server inicie..."
sleep 15

# Inicializar Base de Datos de Productos
echo "Configurando ProductsDB..."
docker exec -i products-db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P TuPasswordPro2026 -C -d ProductsDB -i /sql/products-init.sql

# Inicializar Base de Datos de Transacciones
echo "Configurando TransactionsDB..."
docker exec -i transactions-db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P TuPasswordPro2026 -C -d TransactionsDB -i /sql/transactions-init.sql

echo "¡Inicialización completada con éxito!"
