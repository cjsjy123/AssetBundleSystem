//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using System.Text;
//using NUnit.Framework;
//using UnityEngine;
//using Random = System.Random;

//namespace CommonUtils.Editor.Test
//{

//    public class TestStream
//    {

//        static bool BytesEqual(byte[] left, Stream right,int offset=0)
//        {
//            if (offset == 0)
//            {
//                if (left.Length != right.Length)
//                {
//                    return false;
//                }
//            }

//            var pos = right.Position;
//            right.Position = offset;

//            for (int i = 0; i < left.Length; i++)
//            {
//                var bval = right.ReadByte();
//                if (left[i] != bval)
//                {
//                    right.Position = pos;
//                    return false;
//                }
//            }
//            right.Position = pos;
//            return true;
//        }


//        static bool BytesEqual(byte[] left, byte[] right, int offset = 0)
//        {
//            if (offset == 0)
//            {
//                if (left.Length != right.Length)
//                {
//                    return false;
//                }
//            }

//            for (int i = offset; i < offset + left.Length; i++)
//            {
//                if (left[i] != right[i])
//                {
//                    return false;
//                }
//            }
//            return true;
//        }

//        [Test]
//        public void TestBytesInt()
//        {
//            Random rd = new Random();
//            MemoryStream ms = new MemoryStream();
//            for (int i = 0; i < 100; i++)
//            {
//                int value = rd.Next(-10000,10000);
//                var systembytes = BitConverter.GetBytes(value);

//                ms.WriteStreamInt(0,value, BitConverter.IsLittleEndian);
//                if (!BytesEqual(systembytes, ms))
//                {
//                    Debug.LogError("not equal");
//                }
//                int temp = ms.ReadStreamInt(0, BitConverter.IsLittleEndian);
//                if (temp != value)
//                {
//                    Debug.LogError("not equal");
//                }

//                Array.Reverse(systembytes);

//                //
//                ms.WriteStreamInt(0, value, !BitConverter.IsLittleEndian);
//                if (!BytesEqual(systembytes, ms))
//                {
//                    Debug.LogError("not equal");
//                }
//                temp = ms.ReadStreamInt(0, !BitConverter.IsLittleEndian);
//                if (temp != value)
//                {
//                    Debug.LogError("not equal");
//                }
//            }
//        }

//        [Test]
//        public void TestBytesUInt()
//        {
//            Random rd = new Random();
//            MemoryStream ms = new MemoryStream();
//            for (int i = 0; i < 100; i++)
//            {
//                var value = (uint)rd.Next(0, 10000);
//                var systembytes = BitConverter.GetBytes(value);

//                ms.WriteStreamUInt(0, value,BitConverter.IsLittleEndian);
//                if (!BytesEqual(systembytes, ms))
//                {
//                    Debug.LogError("not equal");
//                }
//                var temp = ms.ReadStreamUInt(0,BitConverter.IsLittleEndian);
//                if (temp != value)
//                {
//                    Debug.LogError("not equal");
//                }
//                Array.Reverse(systembytes);
//                //
//                ms.WriteStreamUInt(0, value, !BitConverter.IsLittleEndian);
//                if (!BytesEqual(systembytes, ms))
//                {
//                    Debug.LogError("not equal");
//                }
//                temp = ms.ReadStreamUInt(0, !BitConverter.IsLittleEndian);
//                if (temp != value)
//                {
//                    Debug.LogError("not equal");
//                }
//            }
//        }

//        [Test]
//        public void TestBytesShort()
//        {
//            Random rd = new Random();
//            MemoryStream ms = new MemoryStream();
//            for (int i = 0; i < 100; i++)
//            {
//                var value = (short)rd.Next(0, 10000);
//                var systembytes = BitConverter.GetBytes(value);

//                ms.WriteStreamShort(0, value,BitConverter.IsLittleEndian);
//                if (!BytesEqual(systembytes, ms))
//                {
//                    Debug.LogError("not equal");
//                }
//                var temp = ms.ReadStreamShort(0,BitConverter.IsLittleEndian);
//                if (temp != value)
//                {
//                    Debug.LogError("not equal");
//                }
//                Array.Reverse(systembytes);
//                //
//                ms.WriteStreamShort(0, value, !BitConverter.IsLittleEndian);
//                if (!BytesEqual(systembytes, ms))
//                {
//                    Debug.LogError("not equal");
//                }
//                temp = ms.ReadStreamShort(0, !BitConverter.IsLittleEndian);
//                if (temp != value)
//                {
//                    Debug.LogError("not equal");
//                }
//            }
//        }

//        [Test]
//        public void TestBytesUShort()
//        {
//            Random rd = new Random();
//            MemoryStream ms = new MemoryStream();
//            for (int i = 0; i < 100; i++)
//            {
//                var value = (ushort)rd.Next(0, 10000);
//                var systembytes = BitConverter.GetBytes(value);

//                ms.WriteStreamUShort(0, value,BitConverter.IsLittleEndian);
//                if (!BytesEqual(systembytes, ms))
//                {
//                    Debug.LogError("not equal");
//                }
//                var temp = ms.ReadStreamUShort(0,BitConverter.IsLittleEndian);
//                if (temp != value)
//                {
//                    Debug.LogError("not equal");
//                }
//                Array.Reverse(systembytes);
//                //
//                ms.WriteStreamUShort(0, value, !BitConverter.IsLittleEndian);
//                if (!BytesEqual(systembytes, ms))
//                {
//                    Debug.LogError("not equal");
//                }
//                temp = ms.ReadStreamUShort(0, !BitConverter.IsLittleEndian);
//                if (temp != value)
//                {
//                    Debug.LogError("not equal");
//                }
//            }
//        }

//        [Test]
//        public void TestBytesLong()
//        {
//            Random rd = new Random();
//            MemoryStream ms = new MemoryStream();
//            for (int i = 0; i < 100; i++)
//            {
//                var value = (long)rd.Next(0, 10000);
//                var systembytes = BitConverter.GetBytes(value);

//                ms.WriteStreamLong(0, value,BitConverter.IsLittleEndian);
//                if (!BytesEqual(systembytes, ms))
//                {
//                    Debug.LogError("not equal");
//                }
//                var temp = ms.ReadStreamLong(0,BitConverter.IsLittleEndian);
//                if (temp != value)
//                {
//                    Debug.LogError("not equal");
//                }
//                Array.Reverse(systembytes);
//                //
//                ms.WriteStreamLong(0, value, !BitConverter.IsLittleEndian);
//                if (!BytesEqual(systembytes, ms))
//                {
//                    Debug.LogError("not equal");
//                }
//                temp = ms.ReadStreamLong(0, !BitConverter.IsLittleEndian);
//                if (temp != value)
//                {
//                    Debug.LogError("not equal");
//                }
//            }
//        }

//        [Test]
//        public void TestBytesULong()
//        {
//            Random rd = new Random();
//            MemoryStream ms = new MemoryStream();
//            for (int i = 0; i < 100; i++)
//            {
//                var value = (ulong)rd.Next(0, 10000);
//                var systembytes = BitConverter.GetBytes(value);

//                ms.WriteStreamULong(0, value,BitConverter.IsLittleEndian);
//                if (!BytesEqual(systembytes, ms))
//                {
//                    Debug.LogError("not equal");
//                }
//                var temp = ms.ReadStreamULong(0,BitConverter.IsLittleEndian);
//                if (temp != value)
//                {
//                    Debug.LogError("not equal");
//                }
//                Array.Reverse(systembytes);
//                //
//                ms.WriteStreamULong(0, value, !BitConverter.IsLittleEndian);
//                if (!BytesEqual(systembytes, ms))
//                {
//                    Debug.LogError("not equal");
//                }
//                temp = ms.ReadStreamULong(0, !BitConverter.IsLittleEndian);
//                if (temp != value)
//                {
//                    Debug.LogError("not equal");
//                }
//            }
//        }


//        [Test]
//        public void TestBytesFloat()
//        {
//            Random rd = new Random();
//            MemoryStream ms = new MemoryStream();
//            for (int i = 0; i < 100; i++)
//            {
//                var value = (float)rd.Next(0, 10000) * 0.001f;
//                var systembytes = BitConverter.GetBytes(value);

//                ms.WriteStreamFloat(0, value,BitConverter.IsLittleEndian);
//                if (!BytesEqual(systembytes, ms))
//                {
//                    Debug.LogError("not equal");
//                }
//                var temp = ms.ReadStreamFloat(0,BitConverter.IsLittleEndian);
//                if (temp.SimpleEqual(value) == false)
//                {
//                    Debug.LogError("not equal");
//                }
//                Array.Reverse(systembytes);
//                //
//                ms.WriteStreamFloat(0, value, !BitConverter.IsLittleEndian);
//                if (!BytesEqual(systembytes, ms))
//                {
//                    Debug.LogError("not equal");
//                }
//                temp = ms.ReadStreamFloat(0, !BitConverter.IsLittleEndian);
//                if (temp.SimpleEqual(value) == false)
//                {
//                    Debug.LogError("not equal");
//                }
//            }
//        }

//        [Test]
//        public void TestBytesDouble()
//        {
//            Random rd = new Random();
//            MemoryStream ms = new MemoryStream();
//            for (int i = 0; i < 100; i++)
//            {
//                var value = (double)rd.Next(0, 10000) * 0.001;
//                var systembytes = BitConverter.GetBytes(value);

//                ms.WriteStreamDouble(0, value,BitConverter.IsLittleEndian);
//                if (!BytesEqual(systembytes, ms))
//                {
//                    Debug.LogError("not equal");
//                }
//                var temp = ms.ReadStreamDouble(0,BitConverter.IsLittleEndian);
//                if (temp.SimpleEqual(value) == false)
//                {
//                    Debug.LogError("not equal");
//                }
//                Array.Reverse(systembytes);
//                //
//                ms.WriteStreamDouble(0, value, !BitConverter.IsLittleEndian);
//                if (!BytesEqual(systembytes, ms))
//                {
//                    Debug.LogError("not equal");
//                }
//                temp = ms.ReadStreamDouble(0, !BitConverter.IsLittleEndian);
//                if (temp.SimpleEqual(value) == false)
//                {
//                    Debug.LogError("not equal");
//                }
//            }
//        }

//        [Test]
//        public void TestBytesChar()
//        {
//            Random rd = new Random();
//            MemoryStream ms = new MemoryStream();
//            for (int i = 0; i < 100; i++)
//            {
//                var value = (char)rd.Next(0, 100) ;
//                var systembytes = BitConverter.GetBytes(value);

//                ms.WriteStreamChar(0, value,BitConverter.IsLittleEndian);
//                if (!BytesEqual(systembytes, ms))
//                {
//                    Debug.LogError("not equal");
//                }
//                var temp = ms.ReadStreamChar(0,BitConverter.IsLittleEndian);
//                if (temp.SimpleEqual(value) == false)
//                {
//                    Debug.LogError("not equal");
//                }
//                Array.Reverse(systembytes);
//                //
//                ms.WriteStreamChar(0, value, !BitConverter.IsLittleEndian);
//                if (!BytesEqual(systembytes, ms))
//                {
//                    Debug.LogError("not equal");
//                }
//                temp = ms.ReadStreamChar(0, !BitConverter.IsLittleEndian);
//                if (temp.SimpleEqual(value) == false)
//                {
//                    Debug.LogError("not equal");
//                }
//            }
//        }

//        [Test]
//        public void TestBytesBool()
//        {
//            MemoryStream ms = new MemoryStream();
//            for (int i = 0; i < 100; i++)
//            {
//                var value = (bool)(i % 2 ==0);
//                var systembytes = BitConverter.GetBytes(value);

//                ms.WriteStreamBool(0, value);
//                if (!BytesEqual(systembytes, ms))
//                {
//                    Debug.LogError("not equal");
//                }
//                //ms.Seek(0, SeekOrigin.Begin);
//                var temp = ms.ReadStreamBool(0);
//                if (temp.SimpleEqual(value) == false)
//                {
//                    Debug.LogError("not equal");
//                }

//            }
//        }

//        [Test]
//        public void TestBytesByte()
//        {
//            Random rd = new Random();
//            MemoryStream ms = new MemoryStream();
//            for (int i = 0; i < 100; i++)
//            {
//                var value = (byte)rd.Next(0, 100);
//                var systembytes = new byte[1]{value};

//                ms.WriteStreamByte(0, value);
//                if (!BytesEqual(systembytes, ms))
//                {
//                    Debug.LogError("not equal");
//                }
//                var temp = ms.ReadStreamByte(0);
//                if (temp.SimpleEqual(value) == false)
//                {
//                    Debug.LogError("not equal");
//                }
//            }
//        }


//        [Test]
//        public void TestBytesSByte()
//        {
//            Random rd = new Random();
//            MemoryStream ms = new MemoryStream();
//            for (int i = 0; i < 100; i++)
//            {
//                var value = (sbyte)rd.Next(0, 100);
 
//                ms.WriteStreamSByte(0, value);

//                var temp = ms.ReadStreamSByte(0);
//                if (temp.SimpleEqual(value) == false)
//                {
//                    Debug.LogError("not equal");
//                }
//            }
//        }

//        [Test]
//        public void TestBytesString()
//        {
//            Random rd = new Random();

//            MemoryStream ssss = new MemoryStream();
//            string ins1 = "你好吗啊啊啊11&as902123";
//            string ins2 = "i818190()/?A<>asB";
//            ssss.WriteStreamString(0, ins1, false);
//            ssss.WriteStreamString(4 + ins1.Length*2, ins2, !BitConverter.IsLittleEndian);

//            ssss.Position = 0;
//            var s1 = ssss.ReadStreamString(0, false);
//            var s2 = ssss.ReadStreamString(4 + ins1.Length * 2, !BitConverter.IsLittleEndian);

//            if (s1 != ins1 || s2 != ins2)
//            {
//                Debug.LogError("not equal");
//            }

//            for (int i = 0; i < 100; i++)
//            {
//                var value = rd.Next(0, 100).ToString()+"你好么打算打打噶女阿萨>S1s "+ rd.Next(-298,2135);
//                var systembytes = Encoding.UTF8.GetBytes(value);

//                MemoryStream ms = new MemoryStream();

//                ms.WriteStreamString(0, value, BitConverter.IsLittleEndian);

//                var temp = ms.ReadStreamString(0, BitConverter.IsLittleEndian);
//                if (temp.SimpleEqual(value) == false)
//                {
//                    Debug.LogError("not equal");
//                }
//                ms.Position = 0;
//                //
//                ms.WriteStreamString(0, value, !BitConverter.IsLittleEndian);

//                temp = ms.ReadStreamString(0, !BitConverter.IsLittleEndian);
//                if (temp.SimpleEqual(value) == false)
//                {
//                    Debug.LogError("not equal");
//                }
//            }
//        }
//    }


//}
