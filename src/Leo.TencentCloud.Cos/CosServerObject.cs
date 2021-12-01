using COSXML;
using COSXML.Auth;
using COSXML.Model.Bucket;
using COSXML.Model.Object;
using COSXML.Model.Tag;
using COSXML.Transfer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Leo.TencentCloud.Cos
{
    public class CosServerObject
    {
        private readonly CAPIConfiguration _capi;
        private readonly CosConfiguration _cos;
        private readonly CosXml _cosXml;

        private BucketStatus _bucketStatus;

        public BucketStatus BucketStatus => _bucketStatus;

        /// <summary>
        /// 对象存储服务器对象
        /// </summary>
        /// <param name="appId">云 API 密钥 AppId, 获取 API 密钥请参照 https://console.cloud.tencent.com/cam/capi</param>
        /// <param name="region">设置默认的区域, COS 地域的简称请参照 https://cloud.tencent.com/document/product/436/6224</param>
        /// <param name="secretId">云 API 密钥 SecretId, 获取 API 密钥请参照 https://console.cloud.tencent.com/cam/capi</param>
        /// <param name="secretKey">云 API 密钥 SecretKey, 获取 API 密钥请参照 https://console.cloud.tencent.com/cam/capi</param>
        /// <param name="durationSecond">每次请求签名有效时长，单位为秒</param>
        public CosServerObject(CAPIConfiguration capi, CosConfiguration cos)
        {
            CosXmlConfig config = new CosXmlConfig.Builder()
                .SetAppid(capi.AppId)
                .SetRegion(cos.Region)
                .Build();

            QCloudCredentialProvider qCloudCredentialProvider = new DefaultQCloudCredentialProvider(capi.SecretId, capi.SecretKey, cos.DurationSecond);

            _capi = capi;
            _cos = cos;
            _cosXml = new CosXmlServer(config, qCloudCredentialProvider);

            HeadBucket();

            if (_cos.CreateBucketIfNoExists)
            {
                CreateBucket();
            }
        }

        /// <summary>
        /// 检索存储桶及其权限
        /// </summary>
        /// <returns></returns>
        protected BucketStatus HeadBucket()
        {
            BucketStatus status;
            HeadBucketRequest request = new HeadBucketRequest($"{_cos.Bucket}-{_capi.AppId}");
            HeadBucketResult result;
            try
            {
                result = _cosXml.HeadBucket(request);

                switch (result.httpCode)
                {
                    case 200:
                        status = BucketStatus.Available; break;
                    case 403:
                        status = BucketStatus.PermissionDenied; break;
                    case 404:
                        status = BucketStatus.NotFound; break;
                    default:
                        status = BucketStatus.Unknow; break;
                }
            }
            catch (COSXML.CosException.CosServerException serverEx)
            {
                if (serverEx.statusCode == 403)
                {
                    status = BucketStatus.PermissionDenied;
                }
                else if (serverEx.statusCode == 404)
                {
                    status = BucketStatus.NotFound;
                }
                else
                {
                    status = BucketStatus.Unknow;
                }
            }

            //执行请求
            return _bucketStatus = status;
        }

        /// <summary>
        /// 创建桶
        /// </summary>
        protected void CreateBucket()
        {
            if (_bucketStatus == BucketStatus.NotFound)
            {
                PutBucketRequest request = new PutBucketRequest($"{_cos.Bucket}-{_capi.AppId}");

                _cosXml.PutBucket(request);
            }
        }

        /// <summary>
        /// 获取访问地址，如果没有开通公共读必须加CDN否则无法访问
        /// </summary>
        /// <param name="cosPath"></param>
        /// <returns></returns>
        public string GetCosPathUrl(string cosPath, bool isSign = false)
        {
            if (isSign)
            {
                PreSignatureStruct preSignatureStruct = new PreSignatureStruct();
                // APPID 获取参考 https://console.cloud.tencent.com/developer
                preSignatureStruct.appid = _capi.AppId;
                // 存储桶所在地域, COS 地域的简称请参照 https://cloud.tencent.com/document/product/436/6224
                preSignatureStruct.region = _cos.Region;
                // 存储桶名称，此处填入格式必须为 bucketname-APPID, 其中 APPID 获取参考 https://console.cloud.tencent.com/developer
                preSignatureStruct.bucket = $"{_cos.Bucket}-{_capi.AppId}";
                preSignatureStruct.key = cosPath; //对象键
                preSignatureStruct.httpMethod = "GET"; //HTTP 请求方法
                preSignatureStruct.isHttps = true; //生成 HTTPS 请求 URL
                preSignatureStruct.signDurationSecond = 600; //请求签名时间为600s
                preSignatureStruct.headers = null;//签名中需要校验的 header
                preSignatureStruct.queryParameters = null; //签名中需要校验的 URL 中请求参数

                return _cosXml.GenerateSignURL(preSignatureStruct);
            }
            else
                return _cosXml.GetObjectUrl(_cos.Bucket, cosPath);
        }
        
        /// <summary>
        /// 获取对象列表
        /// </summary>
        /// <param name="nextMarker"></param>
        /// <returns></returns>
        public GetBucketResult GetObjects(string nextMarker = null)
        {
            GetBucketRequest request = new GetBucketRequest(_cos.Bucket);
            if (!string.IsNullOrWhiteSpace(nextMarker))
            {
                //上一次拉取数据的下标
                request.SetMarker(nextMarker);
            }
            //执行请求
            return _cosXml.GetBucket(request);
        }

        /// <summary>
        /// 删除对象
        /// </summary>
        /// <param name="cosPath"></param>
        /// <returns></returns>
        public DeleteObjectResult DeleteObject(string cosPath)
        {
            DeleteObjectRequest request = new DeleteObjectRequest(_cos.Bucket, cosPath);
            //执行请求
            return _cosXml.DeleteObject(request);
        }

        /// <summary>
        /// 删除多个对象
        /// </summary>
        /// <param name="cosPaths"></param>
        /// <returns></returns>
        public DeleteMultiObjectResult DeleteObjects(List<string> cosPaths)
        {
            DeleteMultiObjectRequest request = new DeleteMultiObjectRequest(_cos.Bucket);
            //设置返回结果形式
            request.SetDeleteQuiet(false);
            request.SetObjectKeys(cosPaths);
            //执行请求
            return _cosXml.DeleteMultiObjects(request);
        }

        /// <summary>
        /// 高级接口上传文件
        /// </summary>
        /// <param name="bucket">存储桶名称，此处填入格式必须为 bucketname-APPID, 其中 APPID 获取参考 https://console.cloud.tencent.com/developer</param>
        /// <param name="cosPath">对象在存储桶中的位置标识符，即称对象键</param>
        /// <param name="srcPath">本地文件绝对路径</param>
        /// <param name="progressCallback"></param>
        public async Task<COSXMLUploadTask.UploadTaskResult> TransferUploadFileAsync(string cosPath, string srcPath, Action<long, long> progressCallback = null)
        {
            // 初始化 TransferConfig
            TransferConfig transferConfig = new TransferConfig();

            // 初始化 TransferManager
            TransferManager transferManager = new TransferManager(_cosXml, transferConfig);

            // 上传对象
            COSXMLUploadTask uploadTask = new COSXMLUploadTask(_cos.Bucket, cosPath);
            uploadTask.SetSrcPath(srcPath);

            uploadTask.progressCallback = delegate (long completed, long total)
            {
                progressCallback?.Invoke(completed, total);
            };
            
            return await transferManager.UploadAsync(uploadTask);
        }

        /// <summary>
        /// 高级接口上传二进制
        /// </summary>
        /// <param name="bucket">存储桶名称，此处填入格式必须为 bucketname-APPID, 其中 APPID 获取参考 https://console.cloud.tencent.com/developer</param>
        /// <param name="cosPath">对象在存储桶中的位置标识符，即称对象键</param>
        /// <param name="data">二进制数据</param>
        /// <param name="progressCallback"></param>
        /// <returns></returns>
        public async Task<COSXMLUploadTask.UploadTaskResult> TransferUploadBytesAsync(string cosPath, byte[] data, Action<long, long> progressCallback = null)
        {

            // 初始化 TransferConfig
            TransferConfig transferConfig = new TransferConfig();

            // 初始化 TransferManager
            TransferManager transferManager = new TransferManager(_cosXml, transferConfig);

            // 上传对象
            COSXMLUploadTask uploadTask = new COSXMLUploadTask(new PutObjectRequest(_cos.Bucket, cosPath, data));

            uploadTask.progressCallback = delegate (long completed, long total)
            {
                progressCallback?.Invoke(completed, total);
            };

            return await transferManager.UploadAsync(uploadTask);
        }

        /// <summary>
        /// 高级接口上传文件流
        /// </summary>
        /// <param name="bucket">存储桶名称，此处填入格式必须为 bucketname-APPID, 其中 APPID 获取参考 https://console.cloud.tencent.com/developer</param>
        /// <param name="cosPath">对象在存储桶中的位置标识符，即称对象键</param>
        /// <param name="fileStream">文件流</param>
        /// <param name="progressCallback"></param>
        /// <returns></returns>
        public async Task<COSXMLUploadTask.UploadTaskResult> TransferUploadStreamAsync(string cosPath, FileStream fileStream, Action<long, long> progressCallback = null)
        {
            // 初始化 TransferConfig
            TransferConfig transferConfig = new TransferConfig();

            // 初始化 TransferManager
            TransferManager transferManager = new TransferManager(_cosXml, transferConfig);

            long offset = 0L;
            long sendLength = fileStream.Length;
            // 上传对象
            COSXMLUploadTask uploadTask = new COSXMLUploadTask(new PutObjectRequest(_cos.Bucket, cosPath, fileStream, offset, sendLength));

            uploadTask.progressCallback = delegate (long completed, long total)
            {
                progressCallback?.Invoke(completed, total);
            };

            return await transferManager.UploadAsync(uploadTask);
        }
    }
}
