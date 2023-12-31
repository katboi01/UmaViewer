using System;

namespace DereTore.Exchange.Archive.ACB
{
    public class AcbFieldMissingException : AcbException
    {

        public AcbFieldMissingException(string fieldName)
            : base("Missing" + fieldName + "field.")
        {
        }

    }
}
