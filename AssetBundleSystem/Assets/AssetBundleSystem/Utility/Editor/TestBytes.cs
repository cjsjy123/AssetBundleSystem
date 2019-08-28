//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Text;
//using NUnit.Framework;
//using UnityEngine;
//using Random = System.Random;

//namespace CommonUtils.Editor.Test
//{

//    public class TestBytes
//    {

//        static bool BytesEqual(byte[] left, byte[] right,int offset=0)
//        {
//            if (offset == 0)
//            {
//                if (left.Length != right.Length)
//                {
//                    return false;
//                }
//            }

//            for (int i = offset; i < offset+left.Length; i++)
//            {
//                if (left[i] != right[i])
//                {
//                    return false;
//                }
//            }
//            return true;
//        }

//        static bool BytesEqual(sbyte[] left, byte[] right, int offset = 0)
//        {
//            if (offset == 0)
//            {
//                if (left.Length != right.Length)
//                {
//                    return false;
//                }
//            }

//            for (int i = offset; i < offset+left.Length; i++)
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
//            byte[] bs = new byte[4];
//            for (int i = 0; i < 100; i++)
//            {
//                int value = rd.Next(-10000,10000);
//                var systembytes = BitConverter.GetBytes(value);

//                bs.WriteInt(0,value, BitConverter.IsLittleEndian);
//                if (!BytesEqual(systembytes, bs))
//                {
//                    Debug.LogError("not equal");
//                }
//                int temp = bs.ReadInt(0, BitConverter.IsLittleEndian);
//                if (temp != value)
//                {
//                    Debug.LogError("not equal");
//                }

//                Array.Reverse(systembytes);

//                //
//                bs.WriteInt(0, value, !BitConverter.IsLittleEndian);
//                if (!BytesEqual(systembytes, bs))
//                {
//                    Debug.LogError("not equal");
//                }
//                temp = bs.ReadInt(0, !BitConverter.IsLittleEndian);
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
//            byte[] bs = new byte[4];
//            for (int i = 0; i < 100; i++)
//            {
//                var value = (uint)rd.Next(0, 10000);
//                var systembytes = BitConverter.GetBytes(value);

//                bs.WriteUInt(0, value,BitConverter.IsLittleEndian);
//                if (!BytesEqual(systembytes, bs))
//                {
//                    Debug.LogError("not equal");
//                }
//                var temp = bs.ReadUInt(0,BitConverter.IsLittleEndian);
//                if (temp != value)
//                {
//                    Debug.LogError("not equal");
//                }
//                Array.Reverse(systembytes);
//                //
//                bs.WriteUInt(0, value, !BitConverter.IsLittleEndian);
//                if (!BytesEqual(systembytes, bs))
//                {
//                    Debug.LogError("not equal");
//                }
//                temp = bs.ReadUInt(0, !BitConverter.IsLittleEndian);
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
//            byte[] bs = new byte[2];
//            for (int i = 0; i < 100; i++)
//            {
//                var value = (short)rd.Next(0, 10000);
//                var systembytes = BitConverter.GetBytes(value);

//                bs.WriteShort(0, value,BitConverter.IsLittleEndian);
//                if (!BytesEqual(systembytes, bs))
//                {
//                    Debug.LogError("not equal");
//                }
//                var temp = bs.ReadShort(0,BitConverter.IsLittleEndian);
//                if (temp != value)
//                {
//                    Debug.LogError("not equal");
//                }
//                Array.Reverse(systembytes);
//                //
//                bs.WriteShort(0, value, !BitConverter.IsLittleEndian);
//                if (!BytesEqual(systembytes, bs))
//                {
//                    Debug.LogError("not equal");
//                }
//                temp = bs.ReadShort(0, !BitConverter.IsLittleEndian);
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
//            byte[] bs = new byte[2];
//            for (int i = 0; i < 100; i++)
//            {
//                var value = (ushort)rd.Next(0, 10000);
//                var systembytes = BitConverter.GetBytes(value);

//                bs.WriteUShort(0, value,BitConverter.IsLittleEndian);
//                if (!BytesEqual(systembytes, bs))
//                {
//                    Debug.LogError("not equal");
//                }
//                var temp = bs.ReadUShort(0,BitConverter.IsLittleEndian);
//                if (temp != value)
//                {
//                    Debug.LogError("not equal");
//                }
//                Array.Reverse(systembytes);
//                //
//                bs.WriteUShort(0, value, !BitConverter.IsLittleEndian);
//                if (!BytesEqual(systembytes, bs))
//                {
//                    Debug.LogError("not equal");
//                }
//                temp = bs.ReadUShort(0, !BitConverter.IsLittleEndian);
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
//            byte[] bs = new byte[8];
//            for (int i = 0; i < 100; i++)
//            {
//                var value = (long)rd.Next(0, 10000);
//                var systembytes = BitConverter.GetBytes(value);

//                bs.WriteLong(0, value,BitConverter.IsLittleEndian);
//                if (!BytesEqual(systembytes, bs))
//                {
//                    Debug.LogError("not equal");
//                }
//                var temp = bs.ReadLong(0,BitConverter.IsLittleEndian);
//                if (temp != value)
//                {
//                    Debug.LogError("not equal");
//                }
//                Array.Reverse(systembytes);
//                //
//                bs.WriteLong(0, value, !BitConverter.IsLittleEndian);
//                if (!BytesEqual(systembytes, bs))
//                {
//                    Debug.LogError("not equal");
//                }
//                temp = bs.ReadLong(0, !BitConverter.IsLittleEndian);
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
//            byte[] bs = new byte[8];
//            for (int i = 0; i < 100; i++)
//            {
//                var value = (ulong)rd.Next(0, 10000);
//                var systembytes = BitConverter.GetBytes(value);

//                bs.WriteULong(0, value,BitConverter.IsLittleEndian);
//                if (!BytesEqual(systembytes, bs))
//                {
//                    Debug.LogError("not equal");
//                }
//                var temp = bs.ReadULong(0,BitConverter.IsLittleEndian);
//                if (temp != value)
//                {
//                    Debug.LogError("not equal");
//                }
//                Array.Reverse(systembytes);
//                //
//                bs.WriteULong(0, value, !BitConverter.IsLittleEndian);
//                if (!BytesEqual(systembytes, bs))
//                {
//                    Debug.LogError("not equal");
//                }
//                temp = bs.ReadULong(0, !BitConverter.IsLittleEndian);
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
//            byte[] bs = new byte[4];
//            for (int i = 0; i < 100; i++)
//            {
//                var value = (float)rd.Next(0, 10000) * 0.001f;
//                var systembytes = BitConverter.GetBytes(value);

//                bs.WriteFloat(0, value,BitConverter.IsLittleEndian);
//                if (!BytesEqual(systembytes, bs))
//                {
//                    Debug.LogError("not equal");
//                }
//                var temp = bs.ReadFloat(0,BitConverter.IsLittleEndian);
//                if (temp.SimpleEqual(value) == false)
//                {
//                    Debug.LogError("not equal");
//                }
//                Array.Reverse(systembytes);
//                //
//                bs.WriteFloat(0, value, !BitConverter.IsLittleEndian);
//                if (!BytesEqual(systembytes, bs))
//                {
//                    Debug.LogError("not equal");
//                }
//                temp = bs.ReadFloat(0, !BitConverter.IsLittleEndian);
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
//            byte[] bs = new byte[8];
//            for (int i = 0; i < 100; i++)
//            {
//                var value = (double)rd.Next(0, 10000) * 0.001;
//                var systembytes = BitConverter.GetBytes(value);

//                bs.WriteDouble(0, value,BitConverter.IsLittleEndian);
//                if (!BytesEqual(systembytes, bs))
//                {
//                    Debug.LogError("not equal");
//                }
//                var temp = bs.ReadDouble(0,BitConverter.IsLittleEndian);
//                if (temp.SimpleEqual(value) == false)
//                {
//                    Debug.LogError("not equal");
//                }
//                Array.Reverse(systembytes);
//                //
//                bs.WriteDouble(0, value, !BitConverter.IsLittleEndian);
//                if (!BytesEqual(systembytes, bs))
//                {
//                    Debug.LogError("not equal");
//                }
//                temp = bs.ReadDouble(0, !BitConverter.IsLittleEndian);
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
//            byte[] bs = new byte[2];
//            for (int i = 0; i < 100; i++)
//            {
//                var value = (char)rd.Next(0, 100) ;
//                var systembytes = BitConverter.GetBytes(value);

//                bs.WriteChar(0, value,BitConverter.IsLittleEndian);
//                if (!BytesEqual(systembytes, bs))
//                {
//                    Debug.LogError("not equal");
//                }
//                var temp = bs.ReadChar(0,BitConverter.IsLittleEndian);
//                if (temp.SimpleEqual(value) == false)
//                {
//                    Debug.LogError("not equal");
//                }
//                Array.Reverse(systembytes);
//                //
//                bs.WriteChar(0, value, !BitConverter.IsLittleEndian);
//                if (!BytesEqual(systembytes, bs))
//                {
//                    Debug.LogError("not equal");
//                }
//                temp = bs.ReadChar(0, !BitConverter.IsLittleEndian);
//                if (temp.SimpleEqual(value) == false)
//                {
//                    Debug.LogError("not equal");
//                }
//            }
//        }

//        [Test]
//        public void TestBytesBool()
//        {
//            byte[] bs = new byte[1];
//            for (int i = 0; i < 100; i++)
//            {
//                var value = (bool)(i % 2 ==0);
//                var systembytes = BitConverter.GetBytes(value);

//                bs.WriteBool(0, value);
//                if (!BytesEqual(systembytes, bs))
//                {
//                    Debug.LogError("not equal");
//                }
//                var temp = bs.ReadBool(0);
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
//            byte[] bs = new byte[1];
//            for (int i = 0; i < 100; i++)
//            {
//                var value = (byte)rd.Next(0, 100);
//                var systembytes = new byte[1]{value};

//                bs.WriteByte(0, value);
//                if (!BytesEqual(systembytes, bs))
//                {
//                    Debug.LogError("not equal");
//                }
//                var temp = bs.ReadByte(0);
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
//            byte[] bs = new byte[1];
//            for (int i = 0; i < 100; i++)
//            {
//                var value = (sbyte)rd.Next(0, 100);
 
//                bs.WriteSByte(0, value);

//                var temp = bs.ReadSByte(0);
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


//            for (int i = 0; i < 100; i++)
//            {
//                var value = rd.Next(0, 100).ToString()+"你好么打算打打噶女阿萨>S1s "+ rd.Next(-298,2135);
//                var systembytes = Encoding.UTF8.GetBytes(value);

//                var bs = new byte[systembytes.Length + 4];

//                bs.WriteString(0, value, BitConverter.IsLittleEndian);

//                var temp = bs.ReadString(0, BitConverter.IsLittleEndian);
//                if (temp.SimpleEqual(value) == false)
//                {
//                    Debug.LogError("not equal");
//                }
//                Array.Reverse(systembytes, 0, systembytes.Length);
//                //
//                bs.WriteString(0, value, !BitConverter.IsLittleEndian);

//                temp = bs.ReadString(0, !BitConverter.IsLittleEndian);
//                if (temp.SimpleEqual(value) == false)
//                {
//                    Debug.LogError("not equal");
//                }
//            }
//        }
//    }


//}
