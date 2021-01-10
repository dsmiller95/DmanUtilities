using System;
using Unity.Jobs;

namespace Dman.Utilities
{
    /// <summary>
    /// Utility to help with locking of resources which can be read in parallel by multiple jobs,
    ///     but still need to be edited occasionally
    /// </summary>
    public class ReadWriteJobHandleProtector : IDisposable
    {
        private JobHandle readers = default;
        public bool isWritable { private set; get; } = true;
        public void RegisterJobHandleForReader(JobHandle handle)
        {
            readers = JobHandle.CombineDependencies(readers, handle);
            isWritable = false;
        }
        public void OpenForEdit()
        {
            readers.Complete();
            isWritable = true;
        }

        public void Dispose()
        {
            readers.Complete();
        }
    }
}
