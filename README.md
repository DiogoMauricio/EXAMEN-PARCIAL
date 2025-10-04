# 🎓 Portal Académico - Deployment en Render

## 🚀 Deployment Instructions

### **Opción 1: Render Web Service**

1. **Fork/Clone** este repositorio
2. **Conectar** con tu cuenta de Render
3. **Crear Web Service** desde GitHub
4. **Configuración**:
   - **Build Command**: `dotnet publish -c Release -o out`
   - **Start Command**: `cd out && dotnet "examen parcial.dll"`
   - **Environment**: `Production`
   - **Port**: `10000` (automático)

### **Opción 2: Docker**

```bash
# Build
docker build -t portal-academico .

# Run
docker run -p 8080:80 portal-academico
```

## 🔧 Configuración de Producción

- ✅ **Base de datos**: SQLite incluida
- ✅ **Usuarios predefinidos** creados automáticamente
- ✅ **Cache en memoria** (sin Redis requerido)
- ✅ **Configuración HTTPS** deshabilitada para Render

## 👥 Credenciales de Acceso

**Coordinador:**
- Email: `admin@test.com`
- Password: `123456`

**Estudiante:**
- Email: `student@test.com` 
- Password: `123456`

## 📋 Features Incluidas

- ✅ Sistema de autenticación por roles
- ✅ Panel administrativo para coordinadores
- ✅ Gestión completa de cursos (CRUD)
- ✅ Sistema de matrículas con validaciones
- ✅ Cache de cursos optimizado
- ✅ UI responsive con Bootstrap 5

## 🔗 Estructura del Proyecto

```
Portal Académico/
├── Controllers/          # Controladores MVC
├── Models/              # Modelos de datos
├── Views/               # Vistas Razor
├── Services/            # Servicios de negocio
├── Data/               # Contexto de EF Core
└── wwwroot/            # Archivos estáticos
```

## 🎯 URLs Principales

- `/` - Página principal
- `/SimpleLogin` - Login simplificado
- `/Cursos` - Catálogo de cursos
- `/Coordinador` - Panel administrativo (requiere rol)

---
**🏆 Proyecto: Portal Académico — Gestión de Cursos y Matrículas**