namespace CommonLogic
{
    public class Singleton<T> where T : class, new()
    {
        public bool IsDisposed => mIsDisposed;
        private bool mIsDisposed = false;
        
        public static T Inst
        {
            get
            {
                return _inst ??= new T();
            }
        }
        private static T _inst;

        public virtual void Init()
        {
            
        }

        public virtual void Dispose()
        {
            mIsDisposed = true;
        }
    }
}