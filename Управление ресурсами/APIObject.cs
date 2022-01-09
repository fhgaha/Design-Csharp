using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Memory.API
{
    public class APIObject : IDisposable
    {
        private readonly int id;

        public APIObject(int id)
        {
            MagicAPI.Allocate(id);
            this.id = id;
        }

        private bool isDisposed = false;

        ~APIObject()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); //финализатор не будет вызываться
        }

        protected virtual void Dispose(bool fromDisposeMethod)
        {
            if (!isDisposed)
            {
                if (MagicAPI.Contains(id)) MagicAPI.Free(id);
                isDisposed = true;
                // base.Dispose(isDisposing); // если унаследован от Disposable класса
            }
        }
    }
}
