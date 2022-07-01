using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streams.Compression
{
    public class CustomCompressionStream : Stream
    {
        private bool read;
        private readonly Stream baseStream;

        public CustomCompressionStream(Stream baseStream, bool read)
        {
            this.read = read;  
            this.baseStream = baseStream;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if(read)
            {
                int bytesRead = ReadBytesFromBaseStream(out List<byte> result, count);

                result.CopyTo(buffer, offset);
                return bytesRead;
            }

            return -1;
        }

        private int ReadBytesFromBaseStream(out List<byte> result, int count)
        {
            result = new List<byte>();
            int bytesRead = 0;

            while (bytesRead < count)
            {
                int byteInt = baseStream.ReadByte();
                if (byteInt == -1)
                    break;
                byte bt = (byte)byteInt;

                int quantity = baseStream.ReadByte();
                if (quantity == -1)
                    throw new InvalidOperationException("Base stream has an odd length. Should be even");

                for (int j = 0; j < quantity; j++)
                {
                    if (bytesRead < count)
                    {
                        result.Add(bt);
                        bytesRead++;
                    }
                    else
                    {
                        baseStream.Position--;
                        baseStream.WriteByte((byte)(quantity - j));
                        baseStream.Position -= 2;

                        break;
                    }
                }
            }

            return bytesRead;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if((! read) && buffer.Length > 0)
            {
                byte current = buffer[offset];
                int currentStartIndex = offset;

                for(int i = offset + 1; i <= offset + count; i++)
                {
                    if(((i < offset + count) && (buffer[i] != current)) || (i == offset + count))
                    {
                        int quantity = i - currentStartIndex;
                        while (quantity > byte.MaxValue)
                        {
                            baseStream.WriteByte(current);
                            baseStream.WriteByte(byte.MaxValue);
                            quantity -= byte.MaxValue;
                        }
                        baseStream.WriteByte(current);
                        baseStream.WriteByte((byte)quantity);

                        if (i < offset + count)
                        {
                            current = buffer[i];
                            currentStartIndex = i;
                        }
                    }
                }
            }
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead => read;
        public override bool CanSeek => false;
        public override bool CanWrite => ! read;
        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }
    }
}
