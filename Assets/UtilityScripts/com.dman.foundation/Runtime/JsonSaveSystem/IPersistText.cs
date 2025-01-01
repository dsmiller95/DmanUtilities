using System.IO;
using JetBrains.Annotations;

namespace Dman.SaveSystem
{
    /// <summary>
    /// Generic interface for saving and loading text
    /// </summary>
    public interface IPersistText
    {
        public TextWriter WriteTo(string contextKey);
        public void OnWriteComplete(string contextKey);
        [CanBeNull] public TextReader ReadFrom(string contextKey);
        public void Delete(string contextKey);
        public void DeleteAll();
    }
}