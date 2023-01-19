using System;
using System.Reflection;

namespace DereTore.Common {
    public static class SingletonManager<T> where T : class {

        static SingletonManager() {
            SyncObject = new T[0];
        }

        public static T Instance {
            get {
                if (_instance == null) {
                    lock (SyncObject) {
                        if (_instance == null) {
                            var t = typeof(T);
                            var constructor = t.GetConstructor(BindingFlags.NonPublic | BindingFlags.NonPublic | BindingFlags.Instance, Type.DefaultBinder, new Type[0], null);
                            if (constructor == null) {
                                throw new InvalidOperationException();
                            }
                            _instance = (T)constructor.Invoke(new object[0]);
                        }
                    }
                }
                return _instance;
            }
        }

        private static T _instance;
        private static readonly T[] SyncObject;

    }
}
