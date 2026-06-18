namespace SyntaxSultan.ComputerSystem.FileSystem
{
    public interface IVirtualDrive
    {
        string DriveName        { get; }
        string DriveLabel       { get; }
        VirtualFolder Root      { get; }
        long TotalBytes         { get; }
        long UsedBytes          { get; }
        bool IsRemovable        { get; }
        bool HasEnoughSpace(long requiredBytes);
    }
}