using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;

namespace CommonUtils
{
    #region Small

    [StructLayout(LayoutKind.Explicit)]
    public struct Int32Field
    {
        [FieldOffset(0)] public int IntVal;
        [FieldOffset(0)] public byte Byte1;
        [FieldOffset(1)] public byte Byte2;
        [FieldOffset(2)] public byte Byte3;
        [FieldOffset(3)] public byte Byte4;

        public Int32Field(int value)
        {
            Byte1 = Byte2 = Byte3 = Byte4 = 0;
            IntVal = value;
        }

        public Int32Field(byte[] bs,int offset)
        {
            IntVal = 0;
            Byte1 = bs[offset];
            Byte2 = bs[offset + 1];
            Byte3 = bs[offset + 2];
            Byte4 = bs[offset + 3];
        }

        public Int32Field(Stream stream, int offset)
        {
            IntVal = 0;
            stream.Position = offset;
            Byte1 = (byte)stream.ReadByte();
            Byte2 = (byte)stream.ReadByte();
            Byte3 = (byte)stream.ReadByte();
            Byte4 = (byte)stream.ReadByte();

        }

        public void Fill(byte[] bs,int offset)
        {
            bs[offset] = Byte1;
            bs[offset + 1] = Byte2;
            bs[offset + 2] = Byte3;
            bs[offset + 3] = Byte4;
        }

        public void Fill(Stream stream, int offset)
        {
            stream.Position = offset;
            stream.WriteByte(Byte1);
            stream.WriteByte(Byte2);
            stream.WriteByte(Byte3);
            stream.WriteByte(Byte4);
        }

        public static implicit operator int(Int32Field value)
        {
            return value.IntVal;
        }

        public static implicit operator Int32Field(int value)
        {
            return new Int32Field(value);
        }
    }


    [StructLayout(LayoutKind.Explicit)]
    public struct Uint32Field
    {
        [FieldOffset(0)] public uint UintVal;
        [FieldOffset(0)] public byte Byte1;
        [FieldOffset(1)] public byte Byte2;
        [FieldOffset(2)] public byte Byte3;
        [FieldOffset(3)] public byte Byte4;

        public Uint32Field(uint value)
        {
            Byte1 = Byte2 = Byte3 = Byte4= 0;
            UintVal = value;
        }

        public Uint32Field(byte[] bs, int offset)
        {
            UintVal = 0;
            Byte1 = bs[offset];
            Byte2 = bs[offset + 1];
            Byte3 = bs[offset + 2];
            Byte4 = bs[offset + 3];
        }

        public Uint32Field(Stream stream, int offset)
        {
            UintVal = 0;
            stream.Position = offset;
            Byte1 = (byte)stream.ReadByte();
            Byte2 = (byte)stream.ReadByte();
            Byte3 = (byte)stream.ReadByte();
            Byte4 = (byte)stream.ReadByte();

        }

        public void Fill(Stream stream, int offset)
        {
            stream.Position = offset;
            stream.WriteByte(Byte1);
            stream.WriteByte(Byte2);
            stream.WriteByte(Byte3);
            stream.WriteByte(Byte4);
        }

        public void Fill(byte[] bs, int offset)
        {
            bs[offset] = Byte1;
            bs[offset + 1] = Byte2;
            bs[offset + 2] = Byte3;
            bs[offset + 3] = Byte4;
        }

        public static implicit operator uint(Uint32Field value)
        {
            return value.UintVal;
        }

        public static implicit operator Uint32Field(uint value)
        {
            return new Uint32Field(value);
        }
    }


    [StructLayout(LayoutKind.Explicit)]
    public struct ShortField
    {
        [FieldOffset(0)] public short ShortVal;
        [FieldOffset(0)] public byte Byte1;
        [FieldOffset(1)] public byte Byte2;


        public ShortField(short value)
        {
            Byte1 = Byte2 = 0;
            ShortVal = value;
        }

        public ShortField(byte[] bs, int offset)
        {
            ShortVal = 0;
            Byte1 = bs[offset];
            Byte2 = bs[offset + 1];
        }

        public ShortField(Stream stream, int offset)
        {
            ShortVal = 0;
            stream.Position = offset;
            Byte1 = (byte)stream.ReadByte();
            Byte2 = (byte)stream.ReadByte();
        }

        public void Fill(byte[] bs, int offset)
        {
            bs[offset] = Byte1;
            bs[offset + 1] = Byte2;

        }

        public void Fill(Stream stream, int offset)
        {
            stream.Position = offset;
            stream.WriteByte(Byte1);
            stream.WriteByte(Byte2);
  
        }

        public static implicit operator short(ShortField value)
        {
            return value.ShortVal;
        }

        public static implicit operator ShortField(short value)
        {
            return new ShortField(value);
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct UShortField
    {
        [FieldOffset(0)] public ushort UShortVal;
        [FieldOffset(0)] public byte Byte1;
        [FieldOffset(1)] public byte Byte2;


        public UShortField(ushort value)
        {
            Byte1 = Byte2 = 0;
            UShortVal = value;
        }

        public UShortField(byte[] bs, int offset)
        {
            UShortVal = 0;
            Byte1 = bs[offset];
            Byte2 = bs[offset + 1];
        }

        public UShortField(Stream stream, int offset)
        {
            UShortVal = 0;
            stream.Position = offset;
            Byte1 = (byte)stream.ReadByte();
            Byte2 = (byte)stream.ReadByte();
        }

        public void Fill(byte[] bs, int offset)
        {
            bs[offset] = Byte1;
            bs[offset + 1] = Byte2;
        }

        public void Fill(Stream stream, int offset)
        {
            stream.Position = offset;
            stream.WriteByte(Byte1);
            stream.WriteByte(Byte2);
        }

        public static implicit operator ushort(UShortField value)
        {
            return value.UShortVal;
        }

        public static implicit operator UShortField(ushort value)
        {
            return new UShortField(value);
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct CharField
    {
        [FieldOffset(0)] public char CharVal;
        [FieldOffset(0)] public byte Byte1;
        [FieldOffset(1)] public byte Byte2;


        public CharField(char value)
        {
            Byte1 = Byte2 = 0;
            CharVal = value;
        }

        public CharField(byte[] bs, int offset)
        {
            CharVal = '0';
            Byte1 = bs[offset];
            Byte2 = bs[offset + 1];
        }

        public CharField(Stream stream, int offset)
        {
            CharVal = '0';
            stream.Position = offset;
            Byte1 = (byte)stream.ReadByte();
            Byte2 = (byte)stream.ReadByte();
        }

        public void Fill(byte[] bs, int offset)
        {
            bs[offset] = Byte1;
            bs[offset + 1] = Byte2;
        }

        public void Fill(Stream stream, int offset)
        {
            stream.Position = offset;
            stream.WriteByte(Byte1);
            stream.WriteByte(Byte2);
        }

        public static implicit operator char(CharField value)
        {
            return value.CharVal;
        }

        public static implicit operator CharField(char value)
        {
            return new CharField(value);
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct LongField
    {
        [FieldOffset(0)] public long LongVal;
        [FieldOffset(0)] public byte Byte1;
        [FieldOffset(1)] public byte Byte2;
        [FieldOffset(2)] public byte Byte3;
        [FieldOffset(3)] public byte Byte4;
        [FieldOffset(4)] public byte Byte5;
        [FieldOffset(5)] public byte Byte6;
        [FieldOffset(6)] public byte Byte7;
        [FieldOffset(7)] public byte Byte8;

        public LongField(long value)
        {
            Byte1 = Byte2 = Byte3 = Byte4 = 0;
            Byte5 = Byte6 = Byte7 = Byte8 = 0;
            LongVal = value;
        }

        public LongField(byte[] bs, int offset)
        {
            LongVal = 0;
            Byte1 = bs[offset];
            Byte2 = bs[offset + 1];
            Byte3 = bs[offset + 2];
            Byte4 = bs[offset + 3];
            Byte5 = bs[offset + 4];
            Byte6=  bs[offset + 5];
            Byte7 = bs[offset + 6];
            Byte8 = bs[offset + 7];
        }

        public LongField(Stream stream, int offset)
        {
            LongVal = 0;
            stream.Position = offset;
            Byte1 = (byte)stream.ReadByte();
            Byte2 = (byte)stream.ReadByte();
            Byte3 = (byte)stream.ReadByte();
            Byte4 = (byte)stream.ReadByte();
            Byte5 = (byte)stream.ReadByte();
            Byte6 = (byte)stream.ReadByte();
            Byte7 = (byte)stream.ReadByte();
            Byte8 = (byte)stream.ReadByte();
        }

        public void Fill(byte[] bs, int offset)
        {
            bs[offset] = Byte1;
            bs[offset + 1] = Byte2;
            bs[offset + 2] = Byte3;
            bs[offset + 3] = Byte4;
            bs[offset + 4] = Byte5;
            bs[offset + 5] = Byte6;
            bs[offset + 6] = Byte7;
            bs[offset + 7] = Byte8;
        }

        public void Fill(Stream stream, int offset)
        {
            stream.Position = offset;
            stream.WriteByte(Byte1);
            stream.WriteByte(Byte2);
            stream.WriteByte(Byte3);
            stream.WriteByte(Byte4);
            stream.WriteByte(Byte5);
            stream.WriteByte(Byte6);
            stream.WriteByte(Byte7);
            stream.WriteByte(Byte8);
        }


        public static implicit operator long(LongField value)
        {
            return value.LongVal;
        }

        public static implicit operator LongField(long value)
        {
            return new LongField(value);
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct ULongField
    {
        [FieldOffset(0)] public ulong ULongVal;
        [FieldOffset(0)] public byte Byte1;
        [FieldOffset(1)] public byte Byte2;
        [FieldOffset(2)] public byte Byte3;
        [FieldOffset(3)] public byte Byte4;
        [FieldOffset(4)] public byte Byte5;
        [FieldOffset(5)] public byte Byte6;
        [FieldOffset(6)] public byte Byte7;
        [FieldOffset(7)] public byte Byte8;

        public ULongField(ulong value)
        {
            Byte1 = Byte2 = Byte3 = Byte4 = 0;
            Byte5 = Byte6 = Byte7 = Byte8 = 0;
            ULongVal = value;
        }

        public ULongField(byte[] bs, int offset)
        {
            ULongVal = 0;
            Byte1 = bs[offset];
            Byte2 = bs[offset + 1];
            Byte3 = bs[offset + 2];
            Byte4 = bs[offset + 3];
            Byte5 = bs[offset + 4];
            Byte6 = bs[offset + 5];
            Byte7 = bs[offset + 6];
            Byte8 = bs[offset + 7];
        }


        public ULongField(Stream stream, int offset)
        {
            ULongVal = 0;
            stream.Position = offset;
            Byte1 = (byte)stream.ReadByte();
            Byte2 = (byte)stream.ReadByte();
            Byte3 = (byte)stream.ReadByte();
            Byte4 = (byte)stream.ReadByte();
            Byte5 = (byte)stream.ReadByte();
            Byte6 = (byte)stream.ReadByte();
            Byte7 = (byte)stream.ReadByte();
            Byte8 = (byte)stream.ReadByte();
        }

        public void Fill(byte[] bs, int offset)
        {
            bs[offset] = Byte1;
            bs[offset + 1] = Byte2;
            bs[offset + 2] = Byte3;
            bs[offset + 3] = Byte4;
            bs[offset + 4] = Byte5;
            bs[offset + 5] = Byte6;
            bs[offset + 6] = Byte7;
            bs[offset + 7] = Byte8;
        }


        public void Fill(Stream stream, int offset)
        {
            stream.Position = offset;
            stream.WriteByte(Byte1);
            stream.WriteByte(Byte2);
            stream.WriteByte(Byte3);
            stream.WriteByte(Byte4);
            stream.WriteByte(Byte5);
            stream.WriteByte(Byte6);
            stream.WriteByte(Byte7);
            stream.WriteByte(Byte8);
        }

        public static implicit operator ulong(ULongField value)
        {
            return value.ULongVal;
        }

        public static implicit operator ULongField(ulong value)
        {
            return new ULongField(value);
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct FloatField
    {
        [FieldOffset(0)] public float FloatVal;
        [FieldOffset(0)] public byte Byte1;
        [FieldOffset(1)] public byte Byte2;
        [FieldOffset(2)] public byte Byte3;
        [FieldOffset(3)] public byte Byte4;

        public FloatField(float value)
        {
            Byte1 = Byte2 = Byte3 = Byte4 = 0;
            FloatVal = value;
        }

        public FloatField(byte[] bs, int offset)
        {
            FloatVal = 0;
            Byte1 = bs[offset];
            Byte2 = bs[offset + 1];
            Byte3 = bs[offset + 2];
            Byte4 = bs[offset + 3];

        }

        public FloatField(Stream stream, int offset)
        {
            FloatVal = 0;
            stream.Position = offset;
            Byte1 = (byte)stream.ReadByte();
            Byte2 = (byte)stream.ReadByte();
            Byte3 = (byte)stream.ReadByte();
            Byte4 = (byte)stream.ReadByte();
        }

        public void Fill(byte[] bs, int offset)
        {
            bs[offset] = Byte1;
            bs[offset + 1] = Byte2;
            bs[offset + 2] = Byte3;
            bs[offset + 3] = Byte4;
        }


        public void Fill(Stream stream, int offset)
        {
            stream.Position = offset;
            stream.WriteByte(Byte1);
            stream.WriteByte(Byte2);
            stream.WriteByte(Byte3);
            stream.WriteByte(Byte4);
        }

        public static implicit operator float(FloatField value)
        {
            return value.FloatVal;
        }

        public static implicit operator FloatField(float value)
        {
            return new FloatField(value);
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct DoubleField
    {
        [FieldOffset(0)] public double DoubleVal;
        [FieldOffset(0)] public byte Byte1;
        [FieldOffset(1)] public byte Byte2;
        [FieldOffset(2)] public byte Byte3;
        [FieldOffset(3)] public byte Byte4;
        [FieldOffset(4)] public byte Byte5;
        [FieldOffset(5)] public byte Byte6;
        [FieldOffset(6)] public byte Byte7;
        [FieldOffset(7)] public byte Byte8;

        public DoubleField(double value)
        {
            Byte1 = Byte2 = Byte3 = Byte4 = 0;
            Byte5 = Byte6 = Byte7 = Byte8 = 0;
            DoubleVal = value;
        }

        public DoubleField(byte[] bs, int offset)
        {
            DoubleVal = 0;
            Byte1 = bs[offset];
            Byte2 = bs[offset + 1];
            Byte3 = bs[offset + 2];
            Byte4 = bs[offset + 3];
            Byte5 = bs[offset + 4];
            Byte6 = bs[offset + 5];
            Byte7 = bs[offset + 6];
            Byte8 = bs[offset + 7];
        }

        public DoubleField(Stream stream, int offset)
        {
            DoubleVal = 0;
            stream.Position = offset;
            Byte1 = (byte)stream.ReadByte();
            Byte2 = (byte)stream.ReadByte();
            Byte3 = (byte)stream.ReadByte();
            Byte4 = (byte)stream.ReadByte();
            Byte5 = (byte)stream.ReadByte();
            Byte6 = (byte)stream.ReadByte();
            Byte7 = (byte)stream.ReadByte();
            Byte8 = (byte)stream.ReadByte();
        }

        public void Fill(byte[] bs, int offset)
        {
            bs[offset] = Byte1;
            bs[offset + 1] = Byte2;
            bs[offset + 2] = Byte3;
            bs[offset + 3] = Byte4;
            bs[offset + 4] = Byte5;
            bs[offset + 5] = Byte6;
            bs[offset + 6] = Byte7;
            bs[offset + 7] = Byte8;
        }

        public void Fill(Stream stream, int offset)
        {
            stream.Position = offset;
            stream.WriteByte(Byte1);
            stream.WriteByte(Byte2);
            stream.WriteByte(Byte3);
            stream.WriteByte(Byte4);
            stream.WriteByte(Byte5);
            stream.WriteByte(Byte6);
            stream.WriteByte(Byte7);
            stream.WriteByte(Byte8);
        }

        public static implicit operator double(DoubleField value)
        {
            return value.DoubleVal;
        }

        public static implicit operator DoubleField(double value)
        {
            return new DoubleField(value);
        }
    }

    #endregion

    #region Revert
    [StructLayout(LayoutKind.Explicit)]
    public struct RInt32Field
    {
        [FieldOffset(0)] public int IntVal;
        [FieldOffset(3)] public byte Byte1;
        [FieldOffset(2)] public byte Byte2;
        [FieldOffset(1)] public byte Byte3;
        [FieldOffset(0)] public byte Byte4;

        public RInt32Field(int value)
        {
            Byte1 = Byte2 = Byte3 = Byte4 = 0;
            IntVal = value;
        }

        public RInt32Field(byte[] bs, int offset)
        {
            IntVal = 0;
            Byte1 = bs[offset];
            Byte2 = bs[offset + 1];
            Byte3 = bs[offset + 2];
            Byte4 = bs[offset + 3];
        }

        public RInt32Field(Stream stream, int offset)
        {
            IntVal = 0;
            stream.Position = offset;
            Byte1 = (byte)stream.ReadByte();
            Byte2 = (byte)stream.ReadByte();
            Byte3 = (byte)stream.ReadByte();
            Byte4 = (byte)stream.ReadByte();
        }

        public void Fill(byte[] bs, int offset)
        {
            bs[offset] = Byte1;
            bs[offset + 1] = Byte2;
            bs[offset + 2] = Byte3;
            bs[offset + 3] = Byte4;
        }

        public void Fill(Stream stream, int offset)
        {
            stream.Position = offset;
            stream.WriteByte(Byte1);
            stream.WriteByte(Byte2);
            stream.WriteByte(Byte3);
            stream.WriteByte(Byte4);
        }

        public static implicit operator int(RInt32Field value)
        {
            return value.IntVal;
        }

        public static implicit operator RInt32Field(int value)
        {
            return new RInt32Field(value);
        }
    }


    [StructLayout(LayoutKind.Explicit)]
    public struct RUint32Field
    {
        [FieldOffset(0)] public uint UintVal;
        [FieldOffset(3)] public byte Byte1;
        [FieldOffset(2)] public byte Byte2;
        [FieldOffset(1)] public byte Byte3;
        [FieldOffset(0)] public byte Byte4;

        public RUint32Field(uint value)
        {
            Byte1 = Byte2 = Byte3 = Byte4 = 0;
            UintVal = value;
        }

        public RUint32Field(byte[] bs, int offset)
        {
            UintVal = 0;
            Byte1 = bs[offset];
            Byte2 = bs[offset + 1];
            Byte3 = bs[offset + 2];
            Byte4 = bs[offset + 3];
        }

        public RUint32Field(Stream stream, int offset)
        {
            UintVal = 0;
            stream.Position = offset;
            Byte1 = (byte)stream.ReadByte();
            Byte2 = (byte)stream.ReadByte();
            Byte3 = (byte)stream.ReadByte();
            Byte4 = (byte)stream.ReadByte();
        }

        public void Fill(byte[] bs, int offset)
        {
            bs[offset] = Byte1;
            bs[offset + 1] = Byte2;
            bs[offset + 2] = Byte3;
            bs[offset + 3] = Byte4;
        }

        public void Fill(Stream stream, int offset)
        {
            stream.Position = offset;
            stream.WriteByte(Byte1);
            stream.WriteByte(Byte2);
            stream.WriteByte(Byte3);
            stream.WriteByte(Byte4);
        }

        public static implicit operator uint(RUint32Field value)
        {
            return value.UintVal;
        }

        public static implicit operator RUint32Field(uint value)
        {
            return new RUint32Field(value);
        }
    }


    [StructLayout(LayoutKind.Explicit)]
    public struct RShortField
    {
        [FieldOffset(0)] public short ShortVal;
        [FieldOffset(1)] public byte Byte1;
        [FieldOffset(0)] public byte Byte2;


        public RShortField(short value)
        {
            Byte1 = Byte2 = 0;
            ShortVal = value;
        }

        public RShortField(byte[] bs, int offset)
        {
            ShortVal = 0;
            Byte1 = bs[offset];
            Byte2 = bs[offset + 1];
        }

        public RShortField(Stream stream, int offset)
        {
            ShortVal = 0;
            stream.Position = offset;
            Byte1 = (byte)stream.ReadByte();
            Byte2 = (byte)stream.ReadByte();
        }

        public void Fill(byte[] bs, int offset)
        {
            bs[offset] = Byte1;
            bs[offset + 1] = Byte2;

        }

        public void Fill(Stream stream, int offset)
        {
            stream.Position = offset;
            stream.WriteByte(Byte1);
            stream.WriteByte(Byte2);
        }

        public static implicit operator short(RShortField value)
        {
            return value.ShortVal;
        }

        public static implicit operator RShortField(short value)
        {
            return new RShortField(value);
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct RUShortField
    {
        [FieldOffset(0)] public ushort UShortVal;
        [FieldOffset(1)] public byte Byte1;
        [FieldOffset(0)] public byte Byte2;


        public RUShortField(ushort value)
        {
            Byte1 = Byte2 = 0;
            UShortVal = value;
        }

        public RUShortField(byte[] bs, int offset)
        {
            UShortVal = 0;
            Byte1 = bs[offset];
            Byte2 = bs[offset + 1];
        }

        public RUShortField(Stream stream, int offset)
        {
            UShortVal = 0;
            stream.Position = offset;
            Byte1 = (byte)stream.ReadByte();
            Byte2 = (byte)stream.ReadByte();
        }
        public void Fill(byte[] bs, int offset)
        {
            bs[offset] = Byte1;
            bs[offset + 1] = Byte2;
        }

        public void Fill(Stream stream, int offset)
        {
            stream.Position = offset;
            stream.WriteByte(Byte1);
            stream.WriteByte(Byte2);
        }

        public static implicit operator ushort(RUShortField value)
        {
            return value.UShortVal;
        }

        public static implicit operator RUShortField(ushort value)
        {
            return new RUShortField(value);
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct RCharField
    {
        [FieldOffset(0)] public char CharVal;
        [FieldOffset(1)] public byte Byte1;
        [FieldOffset(0)] public byte Byte2;


        public RCharField(char value)
        {
            Byte1 = Byte2 = 0;
            CharVal = value;
        }

        public RCharField(byte[] bs, int offset)
        {
            CharVal = '0';
            Byte1 = bs[offset];
            Byte2 = bs[offset + 1];
        }


        public RCharField(Stream stream, int offset)
        {
            CharVal = '0';
            stream.Position = offset;
            Byte1 = (byte)stream.ReadByte();
            Byte2 = (byte)stream.ReadByte();
        }

        public void Fill(byte[] bs, int offset)
        {
            bs[offset] = Byte1;
            bs[offset + 1] = Byte2;
        }

        public void Fill(Stream stream, int offset)
        {
            stream.Position = offset;
            stream.WriteByte(Byte1);
            stream.WriteByte(Byte2);
        }

        public static implicit operator char(RCharField value)
        {
            return value.CharVal;
        }

        public static implicit operator RCharField(char value)
        {
            return new RCharField(value);
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct RLongField
    {
        [FieldOffset(0)] public long LongVal;
        [FieldOffset(7)] public byte Byte1;
        [FieldOffset(6)] public byte Byte2;
        [FieldOffset(5)] public byte Byte3;
        [FieldOffset(4)] public byte Byte4;
        [FieldOffset(3)] public byte Byte5;
        [FieldOffset(2)] public byte Byte6;
        [FieldOffset(1)] public byte Byte7;
        [FieldOffset(0)] public byte Byte8;

        public RLongField(long value)
        {
            Byte1 = Byte2 = Byte3 = Byte4 = 0;
            Byte5 = Byte6 = Byte7 = Byte8 = 0;
            LongVal = value;
        }

        public RLongField(byte[] bs, int offset)
        {
            LongVal = 0;
            Byte1 = bs[offset];
            Byte2 = bs[offset + 1];
            Byte3 = bs[offset + 2];
            Byte4 = bs[offset + 3];
            Byte5 = bs[offset + 4];
            Byte6 = bs[offset + 5];
            Byte7 = bs[offset + 6];
            Byte8 = bs[offset + 7];
        }

        public RLongField(Stream stream, int offset)
        {
            LongVal = 0;
            stream.Position = offset;
            Byte1 = (byte)stream.ReadByte();
            Byte2 = (byte)stream.ReadByte();
            Byte3 = (byte)stream.ReadByte();
            Byte4 = (byte)stream.ReadByte();
            Byte5 = (byte)stream.ReadByte();
            Byte6 = (byte)stream.ReadByte();
            Byte7 = (byte)stream.ReadByte();
            Byte8 = (byte)stream.ReadByte();
        }

        public void Fill(byte[] bs, int offset)
        {
            bs[offset] = Byte1;
            bs[offset + 1] = Byte2;
            bs[offset + 2] = Byte3;
            bs[offset + 3] = Byte4;
            bs[offset + 4] = Byte5;
            bs[offset + 5] = Byte6;
            bs[offset + 6] = Byte7;
            bs[offset + 7] = Byte8;
        }

        public void Fill(Stream stream, int offset)
        {
            stream.Position = offset;
            stream.WriteByte(Byte1);
            stream.WriteByte(Byte2);
            stream.WriteByte(Byte3);
            stream.WriteByte(Byte4);
            stream.WriteByte(Byte5);
            stream.WriteByte(Byte6);
            stream.WriteByte(Byte7);
            stream.WriteByte(Byte8);
        }


        public static implicit operator long(RLongField value)
        {
            return value.LongVal;
        }

        public static implicit operator RLongField(long value)
        {
            return new RLongField(value);
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct RULongField
    {
        [FieldOffset(0)] public ulong ULongVal;
        [FieldOffset(7)] public byte Byte1;
        [FieldOffset(6)] public byte Byte2;
        [FieldOffset(5)] public byte Byte3;
        [FieldOffset(4)] public byte Byte4;
        [FieldOffset(3)] public byte Byte5;
        [FieldOffset(2)] public byte Byte6;
        [FieldOffset(1)] public byte Byte7;
        [FieldOffset(0)] public byte Byte8;

        public RULongField(ulong value)
        {
            Byte1 = Byte2 = Byte3 = Byte4 = 0;
            Byte5 = Byte6 = Byte7 = Byte8 = 0;
            ULongVal = value;
        }

        public RULongField(byte[] bs, int offset)
        {
            ULongVal = 0;
            Byte1 = bs[offset];
            Byte2 = bs[offset + 1];
            Byte3 = bs[offset + 2];
            Byte4 = bs[offset + 3];
            Byte5 = bs[offset + 4];
            Byte6 = bs[offset + 5];
            Byte7 = bs[offset + 6];
            Byte8 = bs[offset + 7];
        }

        public RULongField(Stream stream, int offset)
        {
            ULongVal = 0;
            stream.Position = offset;
            Byte1 = (byte)stream.ReadByte();
            Byte2 = (byte)stream.ReadByte();
            Byte3 = (byte)stream.ReadByte();
            Byte4 = (byte)stream.ReadByte();
            Byte5 = (byte)stream.ReadByte();
            Byte6 = (byte)stream.ReadByte();
            Byte7 = (byte)stream.ReadByte();
            Byte8 = (byte)stream.ReadByte();
        }

        public void Fill(byte[] bs, int offset)
        {
            bs[offset] = Byte1;
            bs[offset + 1] = Byte2;
            bs[offset + 2] = Byte3;
            bs[offset + 3] = Byte4;
            bs[offset + 4] = Byte5;
            bs[offset + 5] = Byte6;
            bs[offset + 6] = Byte7;
            bs[offset + 7] = Byte8;
        }

        public void Fill(Stream stream, int offset)
        {
            stream.Position = offset;
            stream.WriteByte(Byte1);
            stream.WriteByte(Byte2);
            stream.WriteByte(Byte3);
            stream.WriteByte(Byte4);
            stream.WriteByte(Byte5);
            stream.WriteByte(Byte6);
            stream.WriteByte(Byte7);
            stream.WriteByte(Byte8);
        }

        public static implicit operator ulong(RULongField value)
        {
            return value.ULongVal;
        }

        public static implicit operator RULongField(ulong value)
        {
            return new RULongField(value);
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct RFloatField
    {
        [FieldOffset(0)] public float FloatVal;
        [FieldOffset(3)] public byte Byte1;
        [FieldOffset(2)] public byte Byte2;
        [FieldOffset(1)] public byte Byte3;
        [FieldOffset(0)] public byte Byte4;

        public RFloatField(float value)
        {
            Byte1 = Byte2 = Byte3 = Byte4 = 0;
            FloatVal = value;
        }

        public RFloatField(byte[] bs, int offset)
        {
            FloatVal = 0;
            Byte1 = bs[offset];
            Byte2 = bs[offset + 1];
            Byte3 = bs[offset + 2];
            Byte4 = bs[offset + 3];

        }

        public RFloatField(Stream stream, int offset)
        {
            FloatVal = 0;
            stream.Position = offset;
            Byte1 = (byte)stream.ReadByte();
            Byte2 = (byte)stream.ReadByte();
            Byte3 = (byte)stream.ReadByte();
            Byte4 = (byte)stream.ReadByte();
        }

        public void Fill(byte[] bs, int offset)
        {
            bs[offset] = Byte1;
            bs[offset + 1] = Byte2;
            bs[offset + 2] = Byte3;
            bs[offset + 3] = Byte4;
        }

        public void Fill(Stream stream, int offset)
        {
            stream.Position = offset;
            stream.WriteByte(Byte1);
            stream.WriteByte(Byte2);
            stream.WriteByte(Byte3);
            stream.WriteByte(Byte4);
        }

        public static implicit operator float(RFloatField value)
        {
            return value.FloatVal;
        }

        public static implicit operator RFloatField(float value)
        {
            return new RFloatField(value);
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct RDoubleField
    {
        [FieldOffset(0)] public double DoubleVal;
        [FieldOffset(7)] public byte Byte1;
        [FieldOffset(6)] public byte Byte2;
        [FieldOffset(5)] public byte Byte3;
        [FieldOffset(4)] public byte Byte4;
        [FieldOffset(3)] public byte Byte5;
        [FieldOffset(2)] public byte Byte6;
        [FieldOffset(1)] public byte Byte7;
        [FieldOffset(0)] public byte Byte8;

        public RDoubleField(double value)
        {
            Byte1 = Byte2 = Byte3 = Byte4 = 0;
            Byte5 = Byte6 = Byte7 = Byte8 = 0;
            DoubleVal = value;
        }

        public RDoubleField(byte[] bs, int offset)
        {
            DoubleVal = 0;
            Byte1 = bs[offset];
            Byte2 = bs[offset + 1];
            Byte3 = bs[offset + 2];
            Byte4 = bs[offset + 3];
            Byte5 = bs[offset + 4];
            Byte6 = bs[offset + 5];
            Byte7 = bs[offset + 6];
            Byte8 = bs[offset + 7];
        }

        public RDoubleField(Stream stream, int offset)
        {
            DoubleVal = 0;
            stream.Position = offset;
            Byte1 = (byte)stream.ReadByte();
            Byte2 = (byte)stream.ReadByte();
            Byte3 = (byte)stream.ReadByte();
            Byte4 = (byte)stream.ReadByte();
            Byte5 = (byte)stream.ReadByte();
            Byte6 = (byte)stream.ReadByte();
            Byte7 = (byte)stream.ReadByte();
            Byte8 = (byte)stream.ReadByte();
        }
        public void Fill(byte[] bs, int offset)
        {
            bs[offset] = Byte1;
            bs[offset + 1] = Byte2;
            bs[offset + 2] = Byte3;
            bs[offset + 3] = Byte4;
            bs[offset + 4] = Byte5;
            bs[offset + 5] = Byte6;
            bs[offset + 6] = Byte7;
            bs[offset + 7] = Byte8;
        }

        public void Fill(Stream stream, int offset)
        {
            stream.Position = offset;
            stream.WriteByte(Byte1);
            stream.WriteByte(Byte2);
            stream.WriteByte(Byte3);
            stream.WriteByte(Byte4);
            stream.WriteByte(Byte5);
            stream.WriteByte(Byte6);
            stream.WriteByte(Byte7);
            stream.WriteByte(Byte8);
        }

        public static implicit operator double(RDoubleField value)
        {
            return value.DoubleVal;
        }

        public static implicit operator RDoubleField(double value)
        {
            return new RDoubleField(value);
        }
    }

    #endregion
}


