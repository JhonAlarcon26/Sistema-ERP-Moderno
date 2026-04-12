# Changelog - Sistema ERP Moderno

Todos los cambios notables en este proyecto serán documentados en este archivo.

## [1.1.0] - 2026-04-12

### ✨ Mejoras de Interfaz (UX/UI)
- **Prevención de Doble Clic:** Implementación de íconos de carga animados (`bi-hourglass-split animate-spin`) en todos los botones de acción (Guardar, Editar, Eliminar, Exportar) para evitar envíos duplicados.
- **Estandarización de Loader:** Se reemplazó el texto estático de "Procesando..." por el nuevo estándar visual de reloj de arena.
- **Botón de Estado del Servidor:** El indicador de estado del servidor en el Dashboard ahora es color verde esmeralda y muestra el texto "En línea" para mayor claridad visual.

### 🐛 Correcciones de Errores (Bug Fixes)
- **Codificación de Caracteres:** Se corrigieron múltiples errores de codificación UTF-8 que corrompían caracteres especiales en español (acentos, tildes y la letra 'ñ') en diversas vistas y controladores.
- **Errores de Compilación (.NET):** Se resolvieron conflictos de sensibilidad a mayúsculas y minúsculas en métodos y atributos de .NET (ej. `AddAuthorization`, `AddDays`) que impedían la compilación correcta del proyecto.

### 🛠 DevOps e Infraestructura
- **Docker Compose:** Optimización del archivo de despliegue para asegurar una reconstrucción limpia con los nuevos cambios.
- **Documentación:** Inclusión de guías de actualización rápida en el `README.md`.

---
*Desarrollado con precisión técnica por el equipo de ingeniería.*
