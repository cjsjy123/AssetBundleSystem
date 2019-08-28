#define SAFE_CAST
//#define PROFILER
using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;
#if PROFILER
using UnityEngine.Profiling;
#endif

namespace CommonUtils
{
    public static class ValueFieldExtension
    {

#if SAFE_CAST
        static unsafe void Swap(byte* bs,int offset)
        {
            byte right = *(bs + offset);
            byte left = *bs;

            *(bs + offset) = left;
            *bs = right;
        }

        public static unsafe void Reverse2(char* value)
        {
            byte* bs = (byte*)value;
            Swap(bs, 1);
        }

        public static unsafe void Reverse2(ushort* value)
        {
            byte* bs = (byte*)value;
            Swap(bs, 1);
        }

        public static unsafe void Reverse2(short* value)
        {
            byte* bs = (byte*)value;
            Swap(bs, 1);
        }

        public static unsafe void Reverse4(int* value)
        {
            byte* bs = (byte*)value;
            Swap(bs, 3);
            Swap(bs+1, 1);
        }

        public static unsafe void Reverse4(uint* value)
        {
            byte* bs = (byte*)value;
            Swap(bs, 3);
            Swap(bs + 1, 1);
        }

        public static unsafe void Reverse4(float* value)
        {
            byte* bs = (byte*)value;
            Swap(bs, 3);
            Swap(bs + 1, 1);
        }

        public static unsafe void Reverse8(long* value)
        {
            byte* bs = (byte*)value;
            Swap(bs, 7);
            Swap(bs + 1, 5);
            Swap(bs + 2, 3);
            Swap(bs + 3, 1);
        }

        public static unsafe void Reverse8(ulong* value)
        {
            byte* bs = (byte*)value;
            Swap(bs, 7);
            Swap(bs + 1, 5);
            Swap(bs + 2, 3);
            Swap(bs + 3, 1);
        }

        public static unsafe void Reverse8(double* value)
        {
            byte* bs = (byte*)value;
            Swap(bs, 7);
            Swap(bs + 1, 5);
            Swap(bs + 2, 3);
            Swap(bs + 3, 1);
        }
#endif
        public static void Reverse<T>(T[] array, int offset, int len)
        {
#if PROFILER
            Profiler.BeginSample("Reverse");
#endif
            int start = offset;
            int end = offset + len - 1;
            int ml = len / 2;
            for (int i = 0; i < ml; i++)
            {
                var tmp = array[start+ i];
                array[start + i] = array[end - i];
                array[end - i] = tmp;
            }
#if PROFILER
            Profiler.EndSample();
#endif
        }

        #region streamwrite
        public static
#if SAFE_CAST
            unsafe 
#endif
            void WriteStreamInt(this Stream stream, int offset, int value, bool little)
        {
#if SAFE_CAST
            byte[] intBytes = new byte[4];

            fixed (byte* bs = intBytes)
            {
                int* c = (int*)bs;

                *c = value;
                if (BitConverter.IsLittleEndian != little)
                {
                    Reverse4(c);
                }
            }


            stream.Position = offset;
            stream.Write(intBytes,0,intBytes.Length);
#else

            if (BitConverter.IsLittleEndian == little)
            {
                Int32Field fs = new Int32Field(value);
                fs.Fill(stream, offset);
            }
            else
            {
                RInt32Field fs = new RInt32Field(value);
                fs.Fill(stream, offset);
            }
#endif
        }

        public static
#if SAFE_CAST
            unsafe 
#endif
            void WriteStreamUInt(this Stream stream, int offset, uint value, bool little)
        {
#if SAFE_CAST
            byte[] intBytes = new byte[4];

            fixed (byte* bs = intBytes)
            {
                uint* c = (uint*)bs;

                *c = value;
                if (BitConverter.IsLittleEndian != little)
                {
                    Reverse4(c);
                }
            }

            stream.Position = offset;
            stream.Write(intBytes, 0, intBytes.Length);
#else

            if (BitConverter.IsLittleEndian == little)
            {
                Uint32Field fs = new Uint32Field(value);
                fs.Fill(stream, offset);
            }
            else
            {
                RUint32Field fs = new RUint32Field(value);
                fs.Fill(stream, offset);
            }
#endif
        }

        public static
#if SAFE_CAST
            unsafe 
#endif
            void WriteStreamShort(this Stream stream, int offset, short value, bool little)
        {

#if SAFE_CAST
            byte[] intBytes = new byte[2];

            fixed (byte* bs = intBytes)
            {
                short* c = (short*)bs;

                *c = value;
                if (BitConverter.IsLittleEndian != little)
                {
                    Reverse2(c);
                }
            }

            stream.Position = offset;
            stream.Write(intBytes, 0, intBytes.Length);
#else
            if (BitConverter.IsLittleEndian == little)
            {
                ShortField fs = new ShortField(value);
                fs.Fill(stream, offset);
            }
            else
            {
                RShortField fs = new RShortField(value);
                fs.Fill(stream, offset);
            }
#endif
        }

        public static void WriteStreamBytes(this Stream stream, int offset, byte[] value, bool little)
        {
            stream.WriteStreamLong(offset, value.Length, little);
            if (BitConverter.IsLittleEndian != little)
            {
                Reverse(value, 0, value.Length);
            }
            stream.Position = offset+8;
            stream.Write(value, 0, value.Length);
        }


        public static
#if SAFE_CAST
            unsafe 
#endif
            void WriteStreamUShort(this Stream stream, int offset, ushort value, bool little)
        {
#if SAFE_CAST
            byte[] intBytes = new byte[2];

            fixed (byte* bs = intBytes)
            {
                ushort* c = (ushort*)bs;

                *c = value;
                if (BitConverter.IsLittleEndian != little)
                {
                    Reverse2(c);
                }
            }

            stream.Position = offset;
            stream.Write(intBytes, 0, intBytes.Length);
#else
            if (BitConverter.IsLittleEndian == little)
            {
                UShortField fs = new UShortField(value);
                fs.Fill(stream, offset);
            }
            else
            {
                RUShortField fs = new RUShortField(value);
                fs.Fill(stream, offset);
            }
#endif
        }

        public static
#if SAFE_CAST
            unsafe 
#endif
            void WriteStreamLong(this Stream stream, int offset, long value, bool little)
        {
#if SAFE_CAST
            byte[] intBytes = new byte[8];

            fixed (byte* bs = intBytes)
            {
                long* c = (long*)bs;
                *c = value;
                if (BitConverter.IsLittleEndian != little)
                {
                    Reverse8(c);
                }
            }

            stream.Position = offset;
            stream.Write(intBytes, 0, intBytes.Length);
#else
            if (BitConverter.IsLittleEndian == little)
            {
                LongField fs = new LongField(value);
                fs.Fill(stream, offset);
            }
            else
            {
                RLongField fs = new RLongField(value);
                fs.Fill(stream, offset);
            }
#endif
        }

        public static
#if SAFE_CAST
            unsafe 
#endif
            void WriteStreamULong(this Stream stream, int offset, ulong value, bool little)
        {

#if SAFE_CAST
            byte[] intBytes = new byte[8];

            fixed (byte* bs = intBytes)
            {
                ulong* c = (ulong*)bs;
                *c = value;
                if (BitConverter.IsLittleEndian != little)
                {
                    Reverse8(c);
                }
            }

            stream.Position = offset;
            stream.Write(intBytes, 0, intBytes.Length);
#else
            if (BitConverter.IsLittleEndian == little)
            {
                ULongField fs = new ULongField(value);
                fs.Fill(stream, offset);
            }
            else
            {
                RULongField fs = new RULongField(value);
                fs.Fill(stream, offset);
            }
#endif
        }

        public static
#if SAFE_CAST
            unsafe 
#endif
            void WriteStreamFloat(this Stream stream, int offset, float value, bool little)
        {

#if SAFE_CAST
            byte[] intBytes = new byte[4];

            fixed (byte* bs = intBytes)
            {
                float* c = (float*)bs;
                *c = value;
                if (BitConverter.IsLittleEndian != little)
                {
                    Reverse4(c);
                }
            }

            stream.Position = offset;
            stream.Write(intBytes, 0, intBytes.Length);
#else
            if (BitConverter.IsLittleEndian == little)
            {
                FloatField fs = new FloatField(value);
                fs.Fill(stream, offset);
            }
            else
            {
                RFloatField fs = new RFloatField(value);
                fs.Fill(stream, offset);
            }
#endif
        }

        public static
#if SAFE_CAST
            unsafe 
#endif
            void WriteStreamDouble(this Stream stream, int offset, double value, bool little)
        {

#if SAFE_CAST
            byte[] intBytes = new byte[8];

            fixed (byte* bs = intBytes)
            {
                double* c = (double*)bs;
                *c = value;
                if (BitConverter.IsLittleEndian != little)
                {
                    Reverse8(c);
                }
            }

            stream.Position = offset;
            stream.Write(intBytes, 0, intBytes.Length);
#else
            if (BitConverter.IsLittleEndian == little)
            {
                DoubleField fs = new DoubleField(value);
                fs.Fill(stream, offset);
            }
            else
            {
                RDoubleField fs = new RDoubleField(value);
                fs.Fill(stream, offset);
            }
#endif
        }

        public static
#if SAFE_CAST
            unsafe 
#endif
            void WriteStreamChar(this Stream stream, int offset, char value, bool little)
        {

#if SAFE_CAST
            byte[] intBytes = new byte[2];

            fixed (byte* bs = intBytes)
            {
                char* c = (char*)bs;
                *c = value;
                if (BitConverter.IsLittleEndian != little)
                {
                    Reverse2(c);
                }
            }
            stream.Position = offset;
            stream.Write(intBytes, 0, intBytes.Length);
#else
            if (BitConverter.IsLittleEndian == little)
            {
                CharField fs = new CharField(value);
                fs.Fill(stream, offset);
            }
            else
            {
                RCharField fs = new RCharField(value);
                fs.Fill(stream, offset);
            }
#endif
        }

        public static
#if SAFE_CAST
            unsafe 
#endif
            void WriteStreamString(this Stream stream, int offset, string value, bool little)
        {

#if SAFE_CAST
            stream.WriteStreamInt(offset,value.Length,little);
            byte[] bs = new byte[value.Length *2];
            fixed (byte* tempbytes = bs)
            {
                char* start = (char*) tempbytes;
                fixed (char* c = value)
                {
                    for (int i = 0; i < value.Length; i++)
                    {
                        char* r = (c + i);
                        if (BitConverter.IsLittleEndian != little)
                        {
                            Reverse2(r);
                        }

                        *(start+i) = *r;
                    }
                }
            }



            stream.Write(bs, 0, bs.Length);
#else
            var bs = Encoding.UTF8.GetBytes(value);
            stream.WriteStreamInt(offset, bs.Length, little);

            stream.Write(bs,0,bs.Length);
#endif
        }


        public static void WriteStreamByte(this Stream stream, int offset, byte value)
        {
            stream.Position = offset;
            stream.WriteByte(value);
        }

        public static void WriteStreamSByte(this Stream stream, int offset, sbyte value)
        {
            stream.Position = offset;

#if SAFE_CAST
            int val = value + 128;
            stream.WriteByte((byte)val);
#else
            stream.WriteByte((byte)value);
#endif

        }

        public static void WriteStreamBool(this Stream stream, int offset, bool value)
        {
            stream.Position = offset;
            stream.WriteByte( value ? (byte)1 : (byte)0);
        }

        public static void WriteStreamBytes(this Stream stream, int offset, byte[] bytes)
        {
            if (bytes == null)
                return;

            stream.Position = offset;
            stream.Write(bytes, 0, bytes.Length);
        }

        #endregion

        #region streamread

        public static byte[] ReadAllBytes(this Stream stream)
        {
            var pos = stream.Position;
            stream.Seek(0, SeekOrigin.Begin);

            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);

            stream.Position = pos;
            return bytes;
        }

        public static byte[] ReadBytes(this Stream stream,int offset,int len)
        {
            var pos = stream.Position;
            stream.Seek(offset, SeekOrigin.Begin);

            var bytes = new byte[len];
            stream.Read(bytes, 0, bytes.Length);

            stream.Position = pos;
            return bytes;
        }

        public static byte[] ReadStreamBytes(this Stream stream, int offset, bool little)
        {
            long len = stream.ReadStreamLong(offset, little);
            byte[] bs = new byte[len];

            stream.Position = offset +8;
            stream.Read(bs, 0, bs.Length);

            if (BitConverter.IsLittleEndian != little)
            {
                Reverse(bs, 0, bs.Length);
            }
            return bs;
        }

        public static byte ReadStreamByte(this Stream stream, int offset)
        {
            stream.Position = offset;
            return (byte)stream.ReadByte();
 
        }

        public static sbyte ReadStreamSByte(this Stream stream, int offset)
        {
            stream.Position = offset;
#if SAFE_CAST
            int val = stream.ReadByte() - 128;
            return (sbyte)val;
#else

            return (sbyte)stream.ReadByte();
#endif

        }

        public static bool ReadStreamBool(this Stream stream, int offset)
        {
            stream.Position = offset;
            return stream.ReadByte() == 1;
        }

        public static
#if SAFE_CAST
            unsafe 
#endif
            int ReadStreamInt(this Stream stream, int offset, bool little)
        {

#if SAFE_CAST
            byte[] intBytes = new byte[4];
            stream.Position = offset;
            stream.Read(intBytes, 0, intBytes.Length);

            if (BitConverter.IsLittleEndian != little)
            {
                Reverse(intBytes, 0, intBytes.Length);
            }

            fixed (byte* bs = intBytes)
            {
                int* c = (int*)bs;
                return *c;
            }


#else
            if (BitConverter.IsLittleEndian == little)
            {
                Int32Field fs = new Int32Field(stream, offset);
                return fs.IntVal;
            }
            else
            {
                RInt32Field fs = new RInt32Field(stream, offset);
                return fs.IntVal;

            }
#endif
        }

        public static
#if SAFE_CAST
            unsafe 
#endif
            uint ReadStreamUInt(this Stream stream, int offset, bool little)
        {

#if SAFE_CAST
            byte[] intBytes = new byte[4];
            stream.Position = offset;
            stream.Read(intBytes, 0, intBytes.Length);

            if (BitConverter.IsLittleEndian != little)
            {
                Reverse(intBytes, 0, intBytes.Length);
            }

            fixed (byte* bs = intBytes)
            {
                uint* c = (uint*)bs;
                return *c;
            }
#else
            if (BitConverter.IsLittleEndian == little)
            {
                Uint32Field fs = new Uint32Field(stream, offset);
                return fs.UintVal;
            }
            else
            {
                RUint32Field fs = new RUint32Field(stream, offset);
                return fs.UintVal;

            }
#endif
        }

        public static
#if SAFE_CAST
            unsafe 
#endif
            short ReadStreamShort(this Stream stream, int offset, bool little)
        {

#if SAFE_CAST
            byte[] intBytes = new byte[2];
            stream.Position = offset;
            stream.Read(intBytes, 0, intBytes.Length);

            if (BitConverter.IsLittleEndian != little)
            {
                Reverse(intBytes, 0, intBytes.Length);
            }

            fixed (byte* bs = intBytes)
            {
                short* c = (short*)bs;
                return *c;
            }
#else
            if (BitConverter.IsLittleEndian == little)
            {
                ShortField fs = new ShortField(stream, offset);
                return fs.ShortVal;
            }
            else
            {
                RShortField fs = new RShortField(stream, offset);
                return fs.ShortVal;

            }
#endif
        }

        public static
#if SAFE_CAST
            unsafe 
#endif
            ushort ReadStreamUShort(this Stream stream, int offset, bool little)
        {

#if SAFE_CAST
            byte[] intBytes = new byte[2];
            stream.Position = offset;
            stream.Read(intBytes, 0, intBytes.Length);

            if (BitConverter.IsLittleEndian != little)
            {
                Reverse(intBytes, 0, intBytes.Length);
            }

            fixed (byte* bs = intBytes)
            {
                ushort* c = (ushort*)bs;
                return *c;
            }
#else
            if (BitConverter.IsLittleEndian == little)
            {
                UShortField fs = new UShortField(stream, offset);
                return fs.UShortVal;
            }
            else
            {
                RUShortField fs = new RUShortField(stream, offset);
                return fs.UShortVal;
            }
#endif
        }

        public static
#if SAFE_CAST
            unsafe 
#endif
            long ReadStreamLong(this Stream stream, int offset, bool little)
        {

#if SAFE_CAST
            byte[] intBytes = new byte[8];
            stream.Position = offset;
            stream.Read(intBytes, 0, intBytes.Length);

            if (BitConverter.IsLittleEndian != little)
            {
                Reverse(intBytes, 0, intBytes.Length);
            }

            fixed (byte* bs = intBytes)
            {
                long* c = (long*)bs;
                return *c;
            }
#else
            if (BitConverter.IsLittleEndian == little)
            {
                LongField fs = new LongField(stream, offset);
                return fs.LongVal;
            }
            else
            {
                RLongField fs = new RLongField(stream, offset);
                return fs.LongVal;
            }
#endif
        }

        public static
#if SAFE_CAST
            unsafe 
#endif
            ulong ReadStreamULong(this Stream stream, int offset, bool little)
        {
#if SAFE_CAST
            byte[] intBytes = new byte[8];
            stream.Position = offset;
            stream.Read(intBytes, 0, intBytes.Length);

            if (BitConverter.IsLittleEndian != little)
            {
                Reverse(intBytes, 0, intBytes.Length);
            }

            fixed (byte* bs = intBytes)
            {
                uint* c = (uint*)bs;
                return *c;
            }
#else
            if (BitConverter.IsLittleEndian == little)
            {
                ULongField fs = new ULongField(stream, offset);
                return fs.ULongVal;
            }
            else
            {
                RULongField fs = new RULongField(stream, offset);
                return fs.ULongVal;
            }
#endif
        }

        public static
#if SAFE_CAST
            unsafe 
#endif
            char ReadStreamChar(this Stream stream, int offset, bool little)
        {

#if SAFE_CAST
            byte[] intBytes = new byte[2];
            stream.Position = offset;
            stream.Read(intBytes, 0, intBytes.Length);

            if (BitConverter.IsLittleEndian != little)
            {
                Reverse(intBytes, 0, intBytes.Length);
            }

            fixed (byte* bs = intBytes)
            {
                char* c = (char*)bs;
                return *c;
            }
#else
            if (BitConverter.IsLittleEndian == little)
            {
                CharField fs = new CharField(stream, offset);
                return fs.CharVal;
            }
            else
            {
                RCharField fs = new RCharField(stream, offset);
                return fs.CharVal;
            }
#endif
        }

        public static
#if SAFE_CAST
            unsafe 
#endif
            float ReadStreamFloat(this Stream stream, int offset, bool little)
        {
#if SAFE_CAST
            byte[] intBytes = new byte[4];
            stream.Position = offset;
            stream.Read(intBytes, 0, intBytes.Length);
            if (BitConverter.IsLittleEndian != little)
            {
                Reverse(intBytes, 0, intBytes.Length);
            }

            fixed (byte* bs = intBytes)
            {
                float* c = (float*)bs;
                return *c;
            }
#else
            if (BitConverter.IsLittleEndian == little)
            {
                FloatField fs = new FloatField(stream, offset);
                return fs.FloatVal;
            }
            else
            {
                RFloatField fs = new RFloatField(stream, offset);
                return fs.FloatVal;
            }
#endif
        }

        public static
#if SAFE_CAST
            unsafe 
#endif
            double ReadStreamDouble(this Stream stream, int offset, bool little)
        {

#if SAFE_CAST
            byte[] intBytes = new byte[8];
            stream.Position = offset;
            stream.Read(intBytes, 0, intBytes.Length);

            if (BitConverter.IsLittleEndian != little)
            {
                Reverse(intBytes, 0, intBytes.Length);
            }

            fixed (byte* bs = intBytes)
            {
                double* c = (double*)bs;
                return *c;
            }
#else
            if (BitConverter.IsLittleEndian == little)
            {
                DoubleField fs = new DoubleField(stream, offset);
                return fs.DoubleVal;
            }
            else
            {
                RDoubleField fs = new RDoubleField(stream, offset);
                return fs.DoubleVal;
            }
#endif
        }

        public static
#if SAFE_CAST
            unsafe
#endif
            string ReadStreamString(this Stream stream, int offset, bool little)
        {
            int len = stream.ReadStreamInt(offset, little);
#if SAFE_CAST
            byte[] intBytes = new byte[len*2];
            stream.Read(intBytes, 0, intBytes.Length);

            string s;
            if (BitConverter.IsLittleEndian != little)
            {
                var chars = new char[len];

                fixed (byte* strbytes = intBytes)
                {
                    char* cptr = (char*)(strbytes );
                    for (int i = 0; i < chars.Length; i++)
                    {
                        chars[i] = *(cptr + i);
                    }
                }

                fixed (char* strbytes = chars)
                {
                    s = new string(strbytes, 0, len);
                }
            }
            else
            {
                fixed (byte* strbytes = intBytes)
                {
                    char* cptr = (char*)(strbytes + offset );
                    s = new string(cptr, 0, len);
                }
            }
            return s;

#else
            byte[] bytes = new byte[len];
            stream.Read(bytes, 0, bytes.Length);

            if (BitConverter.IsLittleEndian != little)
            {
                for (int i = 0; i < bytes.Length / 2; i++)
                {
                    var tmp = bytes[i];
                    bytes[i] = bytes[bytes.Length - i - 1];
                    bytes[bytes.Length - i - 1] = tmp;
                }
            }
            return Encoding.UTF8.GetString(bytes,0, len);
#endif

        }

#endregion

#region write
        public static
#if SAFE_CAST
            unsafe
#endif
            void WriteInt(this byte[] arr,int offset,int value,bool little)
        {

#if SAFE_CAST
            fixed (byte* bs = arr)
            {
                int* target = (int*)(bs + offset);
                (*target) = value;
            }

            if (BitConverter.IsLittleEndian != little)
            {
                Reverse(arr, offset, 4);
            }

#else
            if(BitConverter.IsLittleEndian == little)
            {
                Int32Field fs = new Int32Field(value);
                fs.Fill(arr, offset);
            }
            else
            {
                RInt32Field fs = new RInt32Field(value);
                fs.Fill(arr, offset);
            }
#endif

        }

        public static
#if SAFE_CAST
            unsafe
#endif
            void WriteUInt(this byte[] arr, int offset, uint value, bool little)
        {
#if SAFE_CAST
            fixed (byte* bs = arr)
            {
                uint* target = (uint*)(bs + offset);
                (*target) = value;
            }

            if (BitConverter.IsLittleEndian != little)
            {
                Reverse(arr, offset, 4);
            }
#else
            if (BitConverter.IsLittleEndian == little)
            {
                Uint32Field fs = new Uint32Field(value);
                fs.Fill(arr, offset);
            }
            else
            {
                RUint32Field fs = new RUint32Field(value);
                fs.Fill(arr, offset);
            }
#endif
        }

        public static
#if SAFE_CAST
            unsafe
#endif
            void WriteShort(this byte[] arr, int offset, short value, bool little)
        {
#if SAFE_CAST
            fixed (byte* bs = arr)
            {
                short* target = (short*)(bs + offset);
                (*target) = value;
            }

            if (BitConverter.IsLittleEndian != little)
            {
                Reverse(arr, offset,2);
            }
#else
            if (BitConverter.IsLittleEndian == little)
            {
                ShortField fs = new ShortField(value);
                fs.Fill(arr, offset);
            }
            else
            {
                RShortField fs = new RShortField(value);
                fs.Fill(arr, offset);
            }
#endif
        }

        public static
#if SAFE_CAST
            unsafe
#endif
            void WriteUShort(this byte[] arr, int offset, ushort value, bool little)
        {
#if SAFE_CAST
            fixed (byte* bs = arr)
            {
                ushort* target = (ushort*)(bs + offset);
                (*target) = value;
            }

            if (BitConverter.IsLittleEndian != little)
            {
                Reverse(arr, offset, 2);
            }
#else
            if (BitConverter.IsLittleEndian == little)
            {
                UShortField fs = new UShortField(value);
                fs.Fill(arr, offset);
            }
            else
            {
                RUShortField fs = new RUShortField(value);
                fs.Fill(arr, offset);
            }
#endif
        }

        public static
#if SAFE_CAST
            unsafe
#endif
            void WriteLong(this byte[] arr, int offset, long value, bool little)
        {
#if SAFE_CAST
            fixed (byte* bs = arr)
            {
                long* target = (long*)(bs + offset);
                (*target) = value;
            }

            if (BitConverter.IsLittleEndian != little)
            {
                Reverse(arr, offset, 8);
            }
#else
            if (BitConverter.IsLittleEndian == little)
            {
                LongField fs = new LongField(value);
                fs.Fill(arr, offset);
            }
            else
            {
                RLongField fs = new RLongField(value);
                fs.Fill(arr, offset);
            }
#endif
        }

        public static
#if SAFE_CAST
            unsafe
#endif
            void WriteULong(this byte[] arr, int offset, ulong value, bool little)
        {
#if SAFE_CAST
            fixed (byte* bs = arr)
            {
                ulong* target = (ulong*)(bs + offset);
                (*target) = value;
            }

            if (BitConverter.IsLittleEndian != little)
            {
                Reverse(arr, offset, 8);
            }
#else
            if (BitConverter.IsLittleEndian == little)
            {
                ULongField fs = new ULongField(value);
                fs.Fill(arr, offset);
            }
            else
            {
                RULongField fs = new RULongField(value);
                fs.Fill(arr, offset);
            }
#endif
        }

        public static
#if SAFE_CAST
            unsafe
#endif
            void WriteFloat(this byte[] arr, int offset, float value, bool little)
        {
#if SAFE_CAST
            fixed (byte* bs = arr)
            {
                float* target = (float*)(bs + offset);
                (*target) = value;
            }

            if (BitConverter.IsLittleEndian != little)
            {
                Reverse(arr, offset, 4);
            }
#else
            if (BitConverter.IsLittleEndian == little)
            {
                FloatField fs = new FloatField(value);
                fs.Fill(arr, offset);
            }
            else
            {
                RFloatField fs = new RFloatField(value);
                fs.Fill(arr, offset);
            }
#endif
        }

        public static
#if SAFE_CAST
            unsafe
#endif
            void WriteDouble(this byte[] arr, int offset, double value, bool little)
        {
#if SAFE_CAST
            fixed (byte* bs = arr)
            {
                double* target = (double*)(bs + offset);
                (*target) = value;
            }

            if (BitConverter.IsLittleEndian != little)
            {
                Reverse(arr, offset, 8);
            }
#else
            if (BitConverter.IsLittleEndian == little)
            {
                DoubleField fs = new DoubleField(value);
                fs.Fill(arr, offset);
            }
            else
            {
                RDoubleField fs = new RDoubleField(value);
                fs.Fill(arr, offset);
            }
#endif
        }

        public static
#if SAFE_CAST
            unsafe
#endif
            void WriteChar(this byte[] arr, int offset, char value, bool little)
        {
#if SAFE_CAST
            fixed (byte* bs = arr)
            {
                char* target = (char*)(bs + offset);
                (*target) = value;
            }

            if (BitConverter.IsLittleEndian != little)
            {
                Reverse(arr, offset, 2);
            }
#else
            if (BitConverter.IsLittleEndian == little)
            {
                CharField fs = new CharField(value);
                fs.Fill(arr, offset);
            }
            else
            {
                RCharField fs = new RCharField(value);
                fs.Fill(arr, offset);
            }
#endif
        }

        public static
#if SAFE_CAST
            unsafe
#endif
            void WriteString(this byte[] arr, int offset, string value, bool little)
        {

#if SAFE_CAST
            int len = value.Length ;
            arr.WriteInt(offset,len,little);
 
            fixed (byte* bs = arr)
            {
                char* target = (char*)(bs + offset+4);
                fixed (char* c = value)
                {
                    char* start = c;
                    for (int i = 0; i < len; i++)
                    {
                        char* r = (start + i);
                        if (BitConverter.IsLittleEndian != little)
                        {
                            Reverse2(r);
                        }
                        *(target+i) = *(start + i);
                    }
                }
            }

#else

            var bs = Encoding.UTF8.GetBytes(value);
            arr.WriteInt(offset, bs.Length, little);

            Array.Copy(bs, 0, arr, offset + 4, bs.Length);
#endif
        }


        public static void WriteByte(this byte[] arr, int offset, byte value)
        {
            arr[offset] = value;
        }

        public static void WriteSByte(this byte[] arr, int offset, sbyte value)
        {
            int val = value + 128;
            arr[offset] = (byte)val;
        }


        public static void WriteBool(this byte[] arr, int offset, bool value)
        {
            arr[offset] = value?(byte)1:(byte)0;
        }

#endregion
#region read
        public static byte ReadByte(this byte[] arr, int offset)
        {
#if PROFILER
            Profiler.BeginSample("ReadByte");
#endif
            byte b = arr[offset];
#if PROFILER
            Profiler.EndSample();
#endif
            return b;
        }

        public static sbyte ReadSByte(this byte[] arr, int offset)
        {
#if PROFILER
            Profiler.BeginSample("ReadSByte");
#endif
            int val = arr[offset] - 128;
#if PROFILER
            Profiler.EndSample();
#endif
            return (sbyte)val;
        }

        public static bool ReadBool(this byte[] arr, int offset)
        {
#if PROFILER
            Profiler.BeginSample("ReadBool");
#endif
            bool b = arr[offset] == 1;
#if PROFILER
            Profiler.EndSample();
#endif
            return b;
        }

        public static
#if SAFE_CAST
            unsafe 
#endif
            string ReadString(this byte[] arr, int offset, bool little)
        {
            int len = arr.ReadInt(offset, little);
#if SAFE_CAST

#if PROFILER
            Profiler.BeginSample("ReadString");
#endif

            string s ;
            if (BitConverter.IsLittleEndian != little)
            {
                var chars = new char[len];

                fixed (byte* strbytes = arr)
                {
                    char* cptr = (char*)(strbytes + offset + 4);
                    for (int i = 0; i < chars.Length; i++)
                    {
                        chars[i] = *(cptr + i);
                    }
                }

                fixed (char* strbytes = chars)
                {
                    s = new string(strbytes, 0, len);
                }
            }
            else
            {
                fixed (byte* strbytes = arr)
                {
                    char* cptr = (char*)(strbytes + offset + 4);
                    s = new string(cptr, 0, len);
                }
            }

#if PROFILER
            Profiler.EndSample();
#endif
            return s;
#else
            return Encoding.UTF8.GetString(arr, offset + 4, len);
#endif
        }

        public static
#if SAFE_CAST
            unsafe
#endif
            int ReadInt(this byte[] arr, int offset,bool little)
        {
#if SAFE_CAST
#if PROFILER
            Profiler.BeginSample("ReadInt");
#endif

            int result;
            fixed (byte* bytes = arr)
            {
                byte* intbytes = bytes + offset;
                int* intptr = (int*) intbytes;
                result = *intptr;
            }

            if (BitConverter.IsLittleEndian != little)
            {
                Reverse4(&result);
            }

#if PROFILER
            Profiler.EndSample();
#endif

            return result;
#else
            if (BitConverter.IsLittleEndian == little)
            {
                Int32Field fs = new Int32Field(arr, offset);
                return fs.IntVal;
            }
            else
            {
                RInt32Field fs = new RInt32Field(arr, offset);
                return fs.IntVal;
            }
#endif

        }

        public static
#if SAFE_CAST
            unsafe
#endif
            uint ReadUInt(this byte[] arr, int offset, bool little)
        {
#if SAFE_CAST

#if PROFILER
            Profiler.BeginSample("ReadUInt");
#endif
            uint result ;
            fixed (byte* bytes = arr)
            {
                byte* uintbytes = bytes + offset;
                result= * ((uint*) uintbytes);
            }

            if (BitConverter.IsLittleEndian != little)
            {
                Reverse4(&result);
            }
#if PROFILER
            Profiler.EndSample();
#endif
            return result;

#else
            if (BitConverter.IsLittleEndian == little)
            {
                Uint32Field fs = new Uint32Field(arr, offset);
                return fs.UintVal;
            }
            else
            {
                RUint32Field fs = new RUint32Field(arr, offset);
                return fs.UintVal;
            }
#endif
        }

        public static
#if SAFE_CAST
            unsafe
#endif
            short ReadShort(this byte[] arr, int offset, bool little)
        {
#if SAFE_CAST
#if PROFILER
            Profiler.BeginSample("ReadShort");
#endif

            short val ;
            fixed (byte* bytes = arr)
            {
                byte* databytes = bytes + offset;
                val = * ((short*)databytes);
            }

            if (BitConverter.IsLittleEndian != little)
            {
                Reverse2(&val);
            }
#if PROFILER
            Profiler.EndSample();
#endif
            return val;
#else
            if (BitConverter.IsLittleEndian == little)
            {
                ShortField fs = new ShortField(arr, offset);
                return fs.ShortVal;
            }
            else
            {
                RShortField fs = new RShortField(arr, offset);
                return fs.ShortVal;

            }
#endif
        }

        public static
#if SAFE_CAST
            unsafe
#endif
            ushort ReadUShort(this byte[] arr, int offset, bool little)
        {
#if SAFE_CAST
#if PROFILER
            Profiler.BeginSample("ReadUShort");
#endif

            ushort val ;
            fixed (byte* bytes = arr)
            {
                byte* databytes = bytes + offset;
                val = * ((ushort*)databytes);
            }

            if (BitConverter.IsLittleEndian != little)
            {
                Reverse2(&val);
            }
#if PROFILER
            Profiler.EndSample();
#endif
            return val;
#else
            if (BitConverter.IsLittleEndian == little)
            {
                UShortField fs = new UShortField(arr, offset);
                return fs.UShortVal;
            }
            else
            {
                RUShortField fs = new RUShortField(arr, offset);
                return fs.UShortVal;
            }
#endif

        }

        public static
#if SAFE_CAST
            unsafe
#endif
            long ReadLong(this byte[] arr, int offset, bool little)
        {
#if SAFE_CAST
#if PROFILER
            Profiler.BeginSample("ReadLong");
#endif

            long val ;
            fixed (byte* bytes = arr)
            {
                byte* databytes = bytes + offset;
                val = * ((long*)databytes);
            }

            if (BitConverter.IsLittleEndian != little)
            {
                Reverse8(&val);
            }
#if PROFILER
            Profiler.EndSample();
#endif
            return val;
#else
            if (BitConverter.IsLittleEndian == little)
            {
                LongField fs = new LongField(arr, offset);
                return fs.LongVal;
            }
            else
            {
                RLongField fs = new RLongField(arr, offset);
                return fs.LongVal;
            }
#endif
        }

        public static
#if SAFE_CAST
            unsafe
#endif
            ulong ReadULong(this byte[] arr, int offset, bool little)
        {
#if SAFE_CAST
#if PROFILER
            Profiler.BeginSample("ReadULong");
#endif

            ulong val ;
            fixed (byte* bytes = arr)
            {
                byte* databytes = bytes + offset;
                val = * ((ulong*)databytes);
            }
            if (BitConverter.IsLittleEndian != little)
            {
                Reverse8(&val);
            }
#if PROFILER
            Profiler.EndSample();
#endif
            return val;
#else
            if (BitConverter.IsLittleEndian == little)
            {
                ULongField fs = new ULongField(arr, offset);
                return fs.ULongVal;
            }
            else
            {
                RULongField fs = new RULongField(arr, offset);
                return fs.ULongVal;
            }
#endif
        }

        public static
#if SAFE_CAST
            unsafe
#endif
            char ReadChar(this byte[] arr, int offset, bool little)
        {
#if SAFE_CAST

#if PROFILER
            Profiler.BeginSample("ReadChar");
#endif

            char s ;
            fixed (byte* bytes = arr)
            {
                char* databytes =(char*)(bytes + offset);
                s = *databytes;
            }
            if (BitConverter.IsLittleEndian != little)
            {
                Reverse2(&s);
            }

#if PROFILER
            Profiler.EndSample();
#endif
            return s;
#else
            if (BitConverter.IsLittleEndian == little)
            {
                CharField fs = new CharField(arr, offset);
                return fs.CharVal;
            }
            else
            {
                RCharField fs = new RCharField(arr, offset);
                return fs.CharVal;
            }
#endif
        }

        public static
#if SAFE_CAST
            unsafe
#endif
            float ReadFloat(this byte[] arr, int offset, bool little)
        {
#if PROFILER
            Profiler.BeginSample("ReadFloat");
#endif
#if SAFE_CAST

            float f;
            fixed (byte* bytes = arr)
            {
                byte* databytes = bytes + offset;
                f= *((float*)databytes);
            }

            if (BitConverter.IsLittleEndian != little)
            {
                Reverse4(&f);
            }

#if PROFILER
            Profiler.EndSample();
#endif
            return f;
#else
            if (BitConverter.IsLittleEndian == little)
            {
                FloatField fs = new FloatField(arr, offset);
                return fs.FloatVal;
            }
            else
            {
                RFloatField fs = new RFloatField(arr, offset);
                return fs.FloatVal;
            }
#endif
        }

        public static
#if SAFE_CAST
            unsafe
#endif
            double ReadDouble(this byte[] arr, int offset, bool little)
        {

#if SAFE_CAST
#if PROFILER
            Profiler.BeginSample("ReadDouble");
#endif

            double d;
            fixed (byte* bytes = arr)
            {
                byte* databytes = bytes + offset;
                d = *((double*)databytes);
            }

            if (BitConverter.IsLittleEndian != little)
            {
                Reverse8(&d);
            }

#if PROFILER
            Profiler.EndSample();
#endif
            return d;
#else
            if (BitConverter.IsLittleEndian == little)
            {
                DoubleField fs = new DoubleField(arr, offset);
                return fs.DoubleVal;
            }
            else
            {
                RDoubleField fs = new RDoubleField(arr, offset);
                return fs.DoubleVal;
            }
#endif
        }

#endregion
    }
}
