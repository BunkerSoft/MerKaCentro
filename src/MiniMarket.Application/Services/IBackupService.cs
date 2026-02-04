using MiniMarket.Application.Common;

namespace MiniMarket.Application.Services;

public interface IBackupService
{
    Task<Result<string>> CreateBackupAsync(string? customName = null);
    Task<Result<IReadOnlyList<BackupInfo>>> GetBackupsAsync();
    Task<Result> RestoreBackupAsync(string backupPath);
    Task<Result> DeleteBackupAsync(string backupPath);
    Task<Result> CleanupOldBackupsAsync(int keepCount = 10);
}

public record BackupInfo(
    string FileName,
    string FilePath,
    long SizeBytes,
    DateTime CreatedAt);
