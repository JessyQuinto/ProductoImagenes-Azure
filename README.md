# ProductoImagenes

# Proyecto de API para Gestión de Productos e Imágenes

Este proyecto es una API web desarrollada en .NET 8 que permite gestionar productos y sus imágenes asociadas. La API proporciona endpoints para realizar operaciones CRUD sobre los productos y subir imágenes asociadas a cada producto.

## Funcionalidades Principales

- **Gestión de Productos**: Permite crear, leer, actualizar y eliminar productos.
- **Subida de Imágenes**: Las imágenes se suben a Azure Blob Storage y se vinculan a los productos.
- **Integración con SQL Azure**: Utiliza un servidor azure para interactuar con las tablas propuestas en azure database.
- **Documentación API**: Incluye documentación interactiva generada con Swagger para facilitar el uso y pruebas de la API.

## Tecnologías Utilizadas

- **.NET 8**: Framework de desarrollo para aplicaciones web.
- **Entity Framework Core**: ORM para la interacción con SQL Server.
- **Azure Blob Storage**: Almacenamiento de imágenes en la nube.
- **Swagger**: Documentación y pruebas interactivas de la API.

## Instalación

1. Clona el repositorio:
    ```bash
    git clone <URL_DEL_REPOSITORIO>
    ```
2. Restaura las dependencias:
    ```bash
    dotnet restore
    ```
3. Configura las cadenas de conexión en el archivo `appsettings.json`.
4. Ejecuta la aplicación:
    ```bash
    dotnet run
    ```

## Contribuciones

¡Las contribuciones son bienvenidas! Por favor, revisa [CONTRIBUTING.md](CONTRIBUTING.md) para más detalles.
