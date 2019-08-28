using UnityEngine;
using System.Collections;
using System;
using System.Text;

namespace AssetBundleSystem
{
    public struct UserRefHistory:IEquatable<UserRefHistory>
    {
        public string AddRefTarget;
        /// <summary>
        /// first
        /// </summary>
        public float AddTime;

        public bool Equals(UserRefHistory other)
        {
            return AddRefTarget.Equals(other.AddRefTarget);
        }

        public void ToDetail(StringBuilder stringBuilder, string header ="")
        {
            if (stringBuilder != null)
            {
                stringBuilder.AppendFormat("{0}UserRefHistory Infomation:{{\n", header);
                stringBuilder.AppendFormat("\t{0}tRefTarget Path:{1}\n", header, this.AddRefTarget);
                stringBuilder.AppendFormat("\t{0}tAddTime :{1}\n", header, this.AddTime);

                stringBuilder.AppendFormat("{0}}}\n", header);

            }

        }
    }
}