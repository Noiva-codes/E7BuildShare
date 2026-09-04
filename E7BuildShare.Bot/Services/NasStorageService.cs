using System.Runtime.InteropServices;

namespace E7BuildShare.Bot.Services;

public sealed class NasStorageService
{
    private readonly NasStorageOptions _options;

    public NasStorageService(NasStorageOptions options) => _options = options;

    public async Task<string> SaveAsync(
        ulong uploaderId,
        string unitName,
        Uri attachmentUri,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("SMB credential connections currently require Windows.");

        if (!_options.SharePath.StartsWith(@"\\", StringComparison.Ordinal))
            throw new ArgumentException(
                "NasStorage:SharePath must be a UNC SMB path such as \\\\[Server IP or Server Hostname]\\E7Builds.",
                nameof(_options.SharePath));

        var safeUnitName = string.Join("_", unitName.Split(Path.GetInvalidFileNameChars()));
        if (string.IsNullOrWhiteSpace(safeUnitName))
            throw new ArgumentException("Unit name cannot be empty.", nameof(unitName));

        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension))
            extension = ".jpg";

        using var connection = new SmbConnection(_options.SharePath, _options.Username, _options.Password);
        var targetDirectory = Path.Combine(_options.SharePath, uploaderId.ToString(), safeUnitName);
        Directory.CreateDirectory(targetDirectory);
        var targetName = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}{extension}";
        var targetPath = Path.Combine(targetDirectory, targetName);

        using var http = new HttpClient();
        await using var source = await http.GetStreamAsync(attachmentUri, cancellationToken);
        await using var destination = new FileStream(targetPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await source.CopyToAsync(destination, cancellationToken);

        return targetPath;
    }

    private sealed class SmbConnection : IDisposable
    {
        private readonly string _share;

        public SmbConnection(string share, string username, string password)
        {
            _share = share;
            var resource = new NETRESOURCE { dwType = 1, lpRemoteName = share };
            var result = WNetAddConnection2(ref resource, password, username, 0);
            if (result != 0)
                throw new IOException($"Could not connect to NAS share. Windows error: {result}.");
        }

        public void Dispose() => WNetCancelConnection2(_share, 0, true);

        [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
        private static extern int WNetAddConnection2(ref NETRESOURCE resource, string password, string username, int flags);

        [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
        private static extern int WNetCancelConnection2(string name, int flags, bool force);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct NETRESOURCE
        {
            public int dwScope, dwType, dwDisplayType, dwUsage;
            public string? lpLocalName, lpRemoteName, lpComment, lpProvider;
        }
    }
}
