using System;

namespace DereTore.Exchange.Archive.ACB.Serialization {
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class UtfFieldAttribute : Attribute {

        public UtfFieldAttribute(int order, ColumnStorage storage = ColumnStorage.PerRow, string fieldName = null) {
            _Order = order;
            _Storage = storage;
            _FieldName = fieldName;
        }

        public int Order { get { return _Order; } }
        private int _Order;

        public string FieldName { get { return _FieldName; } }
        private string _FieldName;

        public ColumnStorage Storage { get { return _Storage; } }
        private ColumnStorage _Storage;

    }
}
