using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MiniMarket.Application.Common;

namespace MiniMarket.Application.Services;

public class BackupService : IBackupService
{
    private readonly DbContext _dbContext;
    private readonly string _backupPath;
    private readonly string _databaseName;

    public BackupService(DbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _backupPath = configuration["Backup:Path"] ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");
        _databaseName = _dbContext.Database.GetDbConnection().Database;

        if (!Directory.Exists(_backupPath))
        {
            Directory.CreateDirectory(_backupPath);
        }
    }

    public async Task<Result<string>> CreateBackupAsync(string? customName = null)
    {
        try
        {
            var fileName = customName ?? $"backup_{_databaseName}_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
            var fullPath = Path.Combine(_backupPath, fileName);

            var sql = $@"
                BACKUP DATABASE [{_databaseName}]
                TO DISK = '{fullPath}'
                WITH FORMAT, INIT,
                NAME = 'MiniMarket Backup',
                SKIP, NOREWIND, NOUNLOAD, STATS = 10";

            await _dbContext.Database.ExecuteSqlRawAsync(sql);

            return Result<string>.Success(fullPath);
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"Error al crear backup: {ex.Message}");
        }
    }

    public Task<Result<IReadOnlyList<BackupInfo>>> GetBackupsAsync()
    {
        try
        {
            if (!Directory.Exists(_backupPath))
            {
                return Task.FromResult(Result<IReadOnlyList<BackupInfo>>.Success(
                    Array.Empty<BackupInfo>()));
            }

            var files = Directory.GetFiles(_backupPath, "*.bak")
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.CreationTime)
                .Select(f => new BackupInfo(
                    f.Name,
                    f.FullName,
                    f.Length,
                    f.CreationTime))
                .ToList();

            return Task.FromResult(Result<IReadOnlyList<BackupInfo>>.Success(
                (IReadOnlyList<BackupInfo>)files));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<IReadOnlyList<BackupInfo>>.Failure(
                $"Error al listar backups: {ex.Message}"));
        }
    }

    public async Task<Result> RestoreBackupAsync(string backupPath)
    {
        try
        {
            if (!File.Exists(backupPath))
            {
                return Result.Failure("Archivo de backup no encontrado");
            }

            // Set database to single user mode, restore, then back to multi user
            var sql = $@"
                USE master;
                ALTER DATABASE [{_databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                RESTORE DATABASE [{_databaseName}]
                FROM DISK = '{backupPath}'
                WITH REPLACE;
                ALTER DATABASE [{_databaseName}] SET MULTI_USER;";

            await _dbContext.Database.ExecuteSqlRawAsync(sql);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error al restaurar backup: {ex.Message}");
        }
    }

    public Task<Result> DeleteBackupAsync(string backupPath)
    {
        try
        {
            if (!File.Exists(backupPath))
            {
                return Task.FromResult(Result.Failure("Archivo de backup no encontrado"));
            }

            // Ensure we're only deleting from our backup directory
            var fullPath = Path.GetFullPath(backupPath);
            var backupDir = Path.GetFullPath(_backupPath);

            if (!fullPath.StartsWith(backupDir))
            {
                return Task.FromResult(Result.Failure("Operacion no permitida"));
            }

            File.Delete(backupPath);
            return Task.FromResult(Result.Success());
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result.Failure($"Error al eliminar backup: {ex.Message}"));
        }
    }

    public async Task<Result> CleanupOldBackupsAsync(int keepCount = 10)
    {
        try
        {
            var backupsResult = await GetBackupsAsync();
            if (!backupsResult.IsSuccess)
            {
                return Result.Failure(backupsResult.Error!);
            }

            var backupsToDelete = backupsResult.Value!
                .Skip(keepCount)
                .ToList();

            foreach (var backup in backupsToDelete)
            {
                await DeleteBackupAsync(backup.FilePath);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error al limpiar backups antiguos: {ex.Message}");
        }
    }
}
