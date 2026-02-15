using System;

namespace FFA.Services
{
    // Simple service to hold the most recent server exception details (for development only)
    public class DeveloperExceptionService
    {
        private Exception? _last;
        public void Set(Exception ex) => _last = ex;
        public Exception? Get() => _last;
        public void Clear() => _last = null;
    }
}
