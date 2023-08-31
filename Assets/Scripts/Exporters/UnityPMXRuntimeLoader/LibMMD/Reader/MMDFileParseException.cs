using System;

namespace LibMMD.Reader
{
    public class MMDFileParseException : Exception
    {
        public MMDFileParseException(string message) : base(message)
        {
        }
    }
}