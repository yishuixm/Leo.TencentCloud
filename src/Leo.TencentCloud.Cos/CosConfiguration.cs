using System;
using System.Collections.Generic;
using System.Text;

namespace Leo.TencentCloud.Cos
{
    public class CosConfiguration
    {
        /// <summary>
        /// 设置默认的区域, COS 地域的简称请参照 https://cloud.tencent.com/document/product/436/6224
        /// </summary>
        public string Region { get; protected set; }
        /// <summary>
        /// 桶名
        /// </summary>
        public string Bucket { get; protected set; }
        /// <summary>
        /// 每次请求签名有效时长，单位为秒
        /// </summary>
        public long DurationSecond { get; protected set; }
        /// <summary>
        /// 如果桶不存在是否创建
        /// </summary>
        public bool CreateBucketIfNoExists { get; protected set; }

        protected CosConfiguration() { }
        public CosConfiguration(string region = "", string bucket = "", long durationSecond = 600L, bool createBucketIfNoExists = false)
        {
            Region = region;
            Bucket = bucket;
            DurationSecond = durationSecond;
            CreateBucketIfNoExists = createBucketIfNoExists;
        }
    }
}
