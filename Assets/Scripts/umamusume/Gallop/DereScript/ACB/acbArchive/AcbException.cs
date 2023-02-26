using System;

namespace DereTore.Exchange.Archive.ACB {
    public class AcbException : ApplicationException {

        public AcbException() {
        }

        public AcbException(string message)
            : base(message) {
        }

        public AcbException(string message, Exception innerException)
            : base(message, innerException) {
        }

    }
}
