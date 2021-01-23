using System;

namespace SporeCommunity.ModBrowser.ModIdentity
{
    class ModAttributeException : Exception
    {
        internal ModAttributeException(string message) : base(message)
        {
        }

        internal ModAttributeException(string message, Exception innerException) : base(message,innerException)
        {
        }
    }
}
