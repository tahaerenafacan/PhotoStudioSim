using System.Collections.Generic;
using System.Linq;

namespace SyntaxSultan.ComputerSystem.FileSystem
{
    public class VirtualFolder : VirtualFSNode
    {
        private readonly List<VirtualFSNode> children = new();
        public IReadOnlyList<VirtualFSNode> Children => children;

        public VirtualFolder(string name) : base(name) { }

        public bool AddChild(VirtualFSNode node)
        {
            // Aynı isim çakışması önle (case-insensitive)
            if (children.Any(c => string.Equals(c.Name, node.Name,
                    System.StringComparison.OrdinalIgnoreCase))) return false;
            node.Parent = this;
            children.Add(node);
            return true;
        }

        public bool RemoveChild(VirtualFSNode node)
        {
            if (!children.Remove(node)) return false;
            node.Parent = null;
            return true;
        }

        public VirtualFSNode FindChild(string name) =>
            children.FirstOrDefault(c => string.Equals(c.Name, name,
                System.StringComparison.OrdinalIgnoreCase));

        public IEnumerable<VirtualFile>   GetFiles()      => children.OfType<VirtualFile>();
        public IEnumerable<VirtualFolder> GetSubFolders() => children.OfType<VirtualFolder>();
    }
}