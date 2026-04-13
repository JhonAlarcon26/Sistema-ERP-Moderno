# 🚀 Sistema ERP Moderno - STC Services

<div align="center">
  <img src="Sistema%20ERP/wwwroot/img/preview.png" alt="Sistema ERP Dashboard" width="100%">
  
  [![Status](https://img.shields.io/badge/Status-v1.1.0--Stable-emerald?style=for-the-badge&logo=github)](https://github.com/JhonAlarcon26/Sistema-ERP-Moderno)
  [![Tech](https://img.shields.io/badge/.NET-8.0-blue?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
  [![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?style=for-the-badge&logo=docker)](https://www.docker.com/)
  [![UX/UI](https://img.shields.io/badge/Design-Premium-indigo?style=for-the-badge)](https://github.com/JhonAlarcon26/Sistema-ERP-Moderno)
</div>

---

## 🛠 Características Principales
- **Gestión de Stock**: Control total de productos y servicios con categorías dinámicas.
- **Módulo de Ventas y Cotizaciones**: Flujo completo desde la cotización hasta la venta final.
- **Agenda Técnica**: Calendario interactivo para programar visitas y servicios.
- **Finanzas**: Control de cobranzas y liquidaciones con Dashboards analíticos.
- **Seguridad**: Sistema de Roles y Permisos granular (RBAC).
- **UX Optimizada**: Inclusión de micro-animaciones y loaders de prevención de doble clic.

---


## 🐳 Despliegue Rápido con Docker

Este sistema está completamente contenerizado para que puedas probarlo o desplegarlo en un **VPS** en cuestión de segundos.

### Requisitos Previos
- Tener instalado **Docker** y **Docker Compose**.

### Instrucciones de Instalación
1.  **Clonar el Repositorio**:
    ```bash
    git clone https://github.com/JhonAlarcon26/Sistema-ERP-Moderno.git
    cd Sistema-ERP-Moderno
    ```

2.  **Configurar Credenciales**:
    Copia el archivo de ejemplo y edita tu contraseña de base de datos:
    ```bash
    cp .env.example .env
    ```
    *Edita el archivo `.env` y cambia `SQLSERVER_SA_PASSWORD` por una clave segura.*

3.  **Encender el Sistema**:
    ```bash
    docker compose up -d
    ```

El sistema estará disponible en `http://localhost:8080`.

### 🔑 Acceso al Sistema
Para ingresar al sistema por primera vez, utiliza las siguientes credenciales predefinidas:

- **Usuario**: `admin`
- **Contraseña**: `admin`

---

## 🔄 Cómo Actualizar

Si ya tienes una versión instalada y quieres actualizar el sistema con las últimas mejoras de diseño y correcciones de errores, ejecuta estos comandos en tu terminal:

```bash
git pull origin main
docker compose down
docker compose up --build -d
```
*Este proceso descargará el código nuevo y reconstruirá los contenedores para aplicar los cambios sin perder tus datos de la base de datos.*


---

## ☁️ Guía de Despliegue en VPS (Nginx + SSL)

Para una instalación profesional en un VPS con **Certificado SSL (HTTPS)**, sigue estos pasos:

### 1. Configurar Nginx como Proxy Inverso
Instala Nginx y crea un archivo de configuración para tu sitio:
```nginx
server {
    listen 80;
    server_name tu-dominio.com;

    location / {
        proxy_pass http://localhost:8080;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }
}
```

### 2. Obtener Certificado SSL Gratuito (Let's Encrypt)
Ejecuta los siguientes comandos:
```bash
sudo apt install certbot python3-certbot-nginx
sudo certbot --nginx -d tu-dominio.com
```

---

## 👨‍💻 Tecnologías Utilizadas
- **Backend**: ASP.NET Core 8.0 (MVC)
- **Base de Datos**: Microsoft SQL Server 2022
- **Frontend**: Vanilla CSS / JavaScript (Estética Premium)
- **Contenedores**: Docker & Docker Compose
- **Reportes**: QuestPDF & ClosedXML

---

## 📄 Licencia
Este proyecto se distribuye bajo la licencia MIT. Siéntete libre de usarlo, modificarlo y compartirlo.

---
*Desarrollado con ❤️ por Jhon Michael Alarcon Alaro (@JhonAlarcon26)*
*Modernización y DevOps por Antigravity AI*
