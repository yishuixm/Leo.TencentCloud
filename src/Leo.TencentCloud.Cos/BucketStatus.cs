using System;
using System.Collections.Generic;
using System.Text;

namespace Leo.TencentCloud.Cos
{
    /// <summary>
    /// 存储桶存在且有读取权限，返回 HTTP 状态码为200。
    /// 无存储桶读取权限，返回 HTTP 状态码为403。
    /// 存储桶不存在，返回 HTTP 状态码为404。
    /// Unknow:未知
    /// Available:可用的
    /// PermissionDenied:没有权限
    /// NotFound:不存在
    /// </summary>
    public enum BucketStatus
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unknow = 0,
        /// <summary>
        /// 可用的
        /// </summary>
        Available = 200,
        /// <summary>
        /// 没有权限
        /// </summary>
        PermissionDenied = 403,
        /// <summary>
        /// 不存在
        /// </summary>
        NotFound = 404
    }
}
