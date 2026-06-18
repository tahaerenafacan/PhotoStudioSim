namespace SyntaxSultan.ComputerSystem.FileSystem
{
    /// <summary>
    /// Oyun içi disk sürücüsü veri modeli (MonoBehaviour değil, saf C#).
    /// InternalDrive için bir, her USB/SD kart için ayrı instance oluştur.
    /// </summary>
    public class VirtualDrive : IVirtualDrive
    {
        public string DriveName  { get; }
        public string DriveLabel { get; }
        public VirtualFolder Root       { get; }
        public long TotalBytes  { get; }
        public long UsedBytes   { get; private set; }
        public bool IsRemovable { get; }

        public VirtualDrive(string driveName, string driveLabel, long totalBytes, bool isRemovable = false)
        {
            DriveName   = driveName;
            DriveLabel  = driveLabel;
            TotalBytes  = totalBytes;
            IsRemovable = isRemovable;
            Root        = new VirtualFolder(driveName);
        }

        public bool HasEnoughSpace(long requiredBytes) =>
            (UsedBytes + requiredBytes) <= TotalBytes;

        public void AddUsedBytes     (long bytes) => UsedBytes += bytes;
        public void SubtractUsedBytes(long bytes) => UsedBytes = System.Math.Max(0, UsedBytes - bytes);
    }
}