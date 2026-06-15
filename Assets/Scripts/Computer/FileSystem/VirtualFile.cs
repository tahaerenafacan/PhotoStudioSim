namespace SyntaxSultan.ComputerSystem.FileSystem
{
    public enum VirtualFileType { Unknown, Image, Document, AppPackage, Data }
    public enum VirtualFileExtension { TXT, PNG }

    public class VirtualFile : VirtualFSNode
    {
        public VirtualFileExtension Extension { get; }
        public VirtualFileType FileType { get; }
        public long SizeBytes { get; }
        private readonly object content;

        public VirtualFile(string name, VirtualFileExtension extension, VirtualFileType fileType, object content, long sizeBytes = 1024) : base(name)
        {
            Extension = extension;
            FileType = fileType;
            this.content = content;
            SizeBytes = sizeBytes;
        }

        /// <summary>
        /// İçeriği güvenli cast ile döner. Yanlış tipte null döner, exception fırlatmaz.
        /// </summary>
        public T GetContent<T>() where T : class => content as T;
    }
}