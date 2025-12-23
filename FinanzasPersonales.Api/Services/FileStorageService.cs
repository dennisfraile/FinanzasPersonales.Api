namespace FinanzasPersonales.Api.Services
{
    /// <summary>
    /// Interface para servicios de almacenamiento de archivos.
    /// Permite cambiar implementación sin modificar código (Local → Azure Blob Storage)
    /// </summary>
    public interface IFileStorageService
    {
        /// <summary>
        /// Guarda un archivo y retorna la ruta relativa
        /// </summary>
        Task<string> SaveFileAsync(IFormFile file, string userId);

        /// <summary>
        /// Obtiene el contenido de un archivo
        /// </summary>
        Task<byte[]> GetFileAsync(string filePath);

        /// <summary>
        /// Elimina un archivo del almacenamiento
        /// </summary>
        Task DeleteFileAsync(string filePath);

        /// <summary>
        /// Verifica si un archivo existe
        /// </summary>
        Task<bool> FileExistsAsync(string filePath);
    }

    /// <summary>
    /// Implementación de almacenamiento en filesystem local del servidor
    /// </summary>
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _basePath;
        private readonly ILogger<LocalFileStorageService> _logger;

        public LocalFileStorageService(ILogger<LocalFileStorageService> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _basePath = Path.Combine(env.ContentRootPath, "uploads");

            // Crear directorio base si no existe
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
                _logger.LogInformation("Created uploads directory at {Path}", _basePath);
            }
        }

        public async Task<string> SaveFileAsync(IFormFile file, string userId)
        {
            try
            {
                // Crear carpeta por usuario
                var userFolder = Path.Combine(_basePath, userId);
                if (!Directory.Exists(userFolder))
                {
                    Directory.CreateDirectory(userFolder);
                }

                // Generar nombre único
                var fileExtension = Path.GetExtension(file.FileName);
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var relativePath = Path.Combine(userId, uniqueFileName);
                var fullPath = Path.Combine(_basePath, relativePath);

                // Guardar archivo
                using var stream = new FileStream(fullPath, FileMode.Create);
                await file.CopyToAsync(stream);

                _logger.LogInformation("File saved: {Path}", relativePath);
                return relativePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving file for user {UserId}", userId);
                throw;
            }
        }

        public async Task<byte[]> GetFileAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_basePath, filePath);

                if (!File.Exists(fullPath))
                {
                    throw new FileNotFoundException($"File not found: {filePath}");
                }

                return await File.ReadAllBytesAsync(fullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading file {Path}", filePath);
                throw;
            }
        }

        public Task DeleteFileAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_basePath, filePath);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation("File deleted: {Path}", filePath);
                }

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file {Path}", filePath);
                throw;
            }
        }

        public Task<bool> FileExistsAsync(string filePath)
        {
            var fullPath = Path.Combine(_basePath, filePath);
            return Task.FromResult(File.Exists(fullPath));
        }
    }
}
