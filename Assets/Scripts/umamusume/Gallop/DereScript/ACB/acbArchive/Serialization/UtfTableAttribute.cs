using System;

namespace DereTore.Exchange.Archive.ACB.Serialization {
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class UtfTableAttribute : Attribute {

        public UtfTableAttribute(string name) {
            _Name = name;
        }

        public string Name { get { return _Name; } }
        private string _Name;        
    }
}
