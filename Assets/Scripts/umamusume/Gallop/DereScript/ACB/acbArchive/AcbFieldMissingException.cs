using System;

namespace DereTore.Exchange.Archive.ACB {
    public class AcbFieldMissingException : AcbException {

        public AcbFieldMissingException(string fieldName)
            : base("Missing" + fieldName + "field.") {
        }

        public AcbFieldMissingException(string fieldName, Exception innerException)
            : base("Missing" + fieldName + "field.", innerException) {
        }

    }
}
