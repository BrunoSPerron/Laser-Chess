using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    class BoardBytesSizeException : Exception
    {
        public Byte[] bytes { get; }
        public BoardBytesSizeException()
        {
        }

        public BoardBytesSizeException(Byte[] b, string message) : base(message)
        {
            bytes = b;
        }

        public BoardBytesSizeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected BoardBytesSizeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    class KingPositionException : Exception
    {
        public King king { get; }
        public KingPositionException()
        {
        }

        public KingPositionException(King k, string message) : base(message)
        {
            king = k;
        }

        public KingPositionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected KingPositionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
