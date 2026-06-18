using System;

namespace SyntaxSultan.ComputerSystem.FileSystem
{
    /// <summary>
    /// Dosya ve klasörün ortak tabanı. Doğrudan instantiate edilemez.
    /// Parent set'i internal tutulur; sadece VirtualFolder.AddChild/RemoveChild erişir.
    /// </summary>
    public abstract class VirtualFSNode
    {
        public string Name { get; private set; }
        public VirtualFolder Parent { get; internal set; }
        public DateTime CreatedAt { get; }

        protected VirtualFSNode(string name)
        {
            Name = name;
            CreatedAt = DateTime.Now;
        }

        public void Rename(string newName)
        {
            if (!string.IsNullOrWhiteSpace(newName))
                Name = newName;
        }

        /// <summary>
        /// Root'tan bu node'a kadar tam yol. Örn: /C/Documents/photo.png
        /// </summary>
        public string GetFullPath() =>
            Parent == null ? "/" + Name : Parent.GetFullPath() + "/" + Name;
    }
}