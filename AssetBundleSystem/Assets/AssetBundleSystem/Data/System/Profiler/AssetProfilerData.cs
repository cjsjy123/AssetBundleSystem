using UnityEngine;
using System.Collections;

namespace AssetBundleSystem
{
    internal struct AssetProfilerData
    {
        public bool IsProfilering;

        public int Offset { get; private set; }

        public int DataCount { get; internal set; }

        public FrameProfilerData[] Datas;

        public void Dispose()
        {
            if(Datas != null)
            {
                for (int i = 0; i < Datas.Length; i++)
                {
                    Datas[i].Dispose();
                }
            }

            Datas = null;
        }

        public void Add(ref FrameProfilerData data)
        {
            if(Datas == null)
            {
                Datas = new FrameProfilerData[512];
            }
            //old
            Datas[Offset].Dispose();

            Datas[Offset] = data;
            Offset = (Offset + 1) % Datas.Length;
        }

        public FrameProfilerData GetLastestData()
        {
            if(Datas == null)
            {
                FrameProfilerData data = new FrameProfilerData();
                data.Frame = -1;
                return data;
            }
            else if(DataCount >= Datas.Length)
            {
                var realoffset = AssetBundleFunction.GetRingOffset(Offset - 1, 0, Datas.Length);
                return Datas[realoffset];
            }

            if (Offset == 0)
            {
                FrameProfilerData data = new FrameProfilerData();
                data.Frame = -1;
                return data;
            }
            return Datas[Offset - 1];
        }
    }

}

