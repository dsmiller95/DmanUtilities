using System.IO;
using JetBrains.Annotations;

namespace Dman.SaveSystem
{
    public interface IPersistText
    {
        public TextWriter WriteTo(string contextKey);
        public void OnWriteComplete(string contextKey);
        [CanBeNull] public TextReader ReadFrom(string contextKey);
        public void Delete(string contextKey);
    }
}