namespace bFrame.Game.Base
{
    public class Singleton<T> where T : new()
    {
        private static T _mInstance;

        public static T Instance
        {
            get
            {
                if (_mInstance==null)
                {
                    _mInstance=new T();
                }

                return _mInstance;
            }
        }
    }
}
