using System;
using System.Collections.Generic;
using System.Text;

namespace Fenrir.Multiplayer.Exceptions
{
    public class TypeMapException : FenrirException
    {
        public TypeMapException()
        {
        }

        public TypeMapException(string message)
            : base(message)
        {
        }

        public TypeMapException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
