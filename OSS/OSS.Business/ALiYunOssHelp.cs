using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aliyun.OSS;
using Aliyun.OSS.Common;

namespace OSS.Business
{
    public class ALiYunOssHelp
    {
        
        ///// <summary>
        ///// 阿里云accessKeyId
        ///// </summary>
        //static string accessKeyId;//=ossSetting.AccessKeyId;// =appSettingHelp.GetAppSettingValue("accessKeyId"); //"aqn7CybbZ6Iq9ehT";//"<your access key id>"; 
        ///// <summary>
        ///// 阿里云accessKeySecret
        ///// </summary>
        //static string accessKeySecret;// = ossSetting.AccessKeySecret;// = appSettingHelp.GetAppSettingValue("accessKeySecret");  //"nDlp2otsEXteHDsQ6gDDkuRTuinezY";// "<your access key secret>";
        ///// <summary>
        ///// 阿里云endpoint
        ///// </summary>
        //static string endpoint;// = ossSetting.Endpoint;// = appSettingHelp.GetAppSettingValue("endpoint"); //"oss-cn-shenzhen.aliyuncs.com";// "<valid host name>";
        ///// <summary>
        ///// 阿里云存储空间名称
        ///// </summary>
        //static string bucketName;// = ossSetting.BucketName;// = appSettingHelp.GetAppSettingValue("bucketName"); //"wxbuappfiles";// "<valid host name>";
        ///// <summary>
        ///// 阿里云上传时分片大小 单位字节
        ///// </summary>
        //static int partSize;// = ossSetting.PartSize;// = SafeCast.SafeCastInt(appSettingHelp.GetAppSettingValue("partSize")); //5 * 1024 * 1024;
        ///// <summary>
        ///// 采取普通上传和分片式上传的分界线 单位字节
        ///// </summary>
        //static int LimitFileSize;// = ossSetting.LimitFileSize;// SafeCast.SafeCastInt(appSettingHelp.GetAppSettingValue("LimitFileSize"));  //80 * 1024 * 1024;

        //static OssClient client;


        static string accessKeyId = ConfigurationManager.AppSettings["AccessKeyId"];// ossSetting.AccessKeyId;
        static string accessKeySecret = ConfigurationManager.AppSettings["AccessKeySecret"];// ossSetting.AccessKeySecret;
        static string endpoint = ConfigurationManager.AppSettings["Endpoint"];// ossSetting.Endpoint;
        static string bucketName = ConfigurationManager.AppSettings["BucketName"];// ossSetting.BucketName;
        static int partSize = SafeCast.SafeCastInt(ConfigurationManager.AppSettings["PartSize"]);//ossSetting.PartSize;
        static int LimitFileSize = SafeCast.SafeCastInt(ConfigurationManager.AppSettings["LimitFileSize"]);// ossSetting.LimitFileSize;
        static OssClient client = new OssClient(endpoint, accessKeyId, accessKeySecret);


        public ALiYunOssHelp()
        {

        }

        //public static string URL = "http://<bucket>.<region>.aliyuncs.com/<object>?OSSAccessKeyId=<user access_key_id>&Expires=<unix time>&Signature=<signature_string>";
        //public static string URL = "http://{0}.aliyuncs.com/{1}?OSSAccessKeyId={2}&Expires=<unix time>&Signature=<signature_string>";


        public static string Update(string FileName, string FilePath)
        {
            var fi = new FileInfo(FilePath);
            var fileSize = fi.Length;
            if (fileSize <= LimitFileSize)//小文件 简单上传
            {
                return PutObject(FileName, FilePath);
            }
            else//大文件 分片式上传上传
            {
                return UploadMultipart(FileName, FilePath);
            }
        }

        #region 简单上传
        /// <summary>
        /// 上传文件 文件大小不能超过100M
        /// </summary>
        /// <param name="FileName">文件名称（例: test.txt）</param>
        /// <param name="FilePath">本地文件路径（例: D:\test.txt）</param>
        public static string PutObject(string FileName, string FilePath)
        {
            //bool result = false;
            try
            {
                var path = SetPath();
                var oos_FilePath = path + FileName;
                using (var fs = File.Open(FilePath, FileMode.Open))
                {
                    client.PutObject(bucketName, oos_FilePath, fs);
                }
                return oos_FilePath;
                //result = true;
            }
            catch (OssException ex)
            {

                Console.WriteLine("Failed with error code: {0}; Error info: {1}. \nRequestID:{2}\tHostID:{3}",
                    ex.ErrorCode, ex.Message, ex.RequestId, ex.HostId);
                var baseinfo = string.Format("baseinfo({0} {1} {2} {3} {4} {5})",
                   "accessKeyId:" + accessKeyId,
                   "accessKeySecret:" + accessKeySecret,
                   "endpoint:" + endpoint,
                   "bucketName:" + bucketName,
                   "partSize:" + partSize,
                   "LimitFileSize:" + LimitFileSize
                   );
               // LogManager.Error(ex, string.Format("Failed with error code: {0}; Error info: {1}. \nRequestID:{2}\tHostID:{3} FileName:{4} FilePath:{5} 报错方法 PutObject(string FileName, string FilePath) " ,ex.ErrorCode, ex.Message, ex.RequestId, ex.HostId, FileName, FilePath) + baseinfo);
                return "";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed with error info: {0}", ex.Message);
                var baseinfo = string.Format("baseinfo({0} {1} {2} {3} {4} {5})",
                   "accessKeyId:" + accessKeyId,
                   "accessKeySecret:" + accessKeySecret,
                   "endpoint:" + endpoint,
                   "bucketName:" + bucketName,
                   "partSize:" + partSize,
                   "LimitFileSize:" + LimitFileSize
                   );
               //LogManager.Error(ex, string.Format("报错时间{0} FileName:{1} FilePath:{2} 报错方法{3}", DateTime.Now, FileName, FilePath, "PutObject(string FileName, string FilePath) ")+baseinfo);
                return "";
            }
            //return result;
        }
        #endregion

        #region 分片式上传
        /// <summary>
        /// 分片式上传 可以上传超过100M的文件
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="FilePath"></param>
        public static string UploadMultipart(string FileName, string FilePath)
        {
            try
            {
                var path = SetPath();
                var oos_FilePath = path + FileName;
                var uploadId = InitiateMultipartUpload(bucketName, oos_FilePath);
                var partETags = UploadParts(bucketName, oos_FilePath, FilePath, uploadId, partSize);
                var completeResult = CompleteUploadPart(bucketName, oos_FilePath, uploadId, partETags);
                Console.WriteLine(@"Upload multipart result : " + completeResult.Location);
                return oos_FilePath;
            }
            catch (OssException ex)
            {
                Console.WriteLine("Failed with error code: {0}; Error info: {1}. \nRequestID:{2}\tHostID:{3}",
                    ex.ErrorCode, ex.Message, ex.RequestId, ex.HostId);
                var baseinfo = string.Format("baseinfo({0} {1} {2} {3} {4} {5})",
                  "accessKeyId:" + accessKeyId,
                  "accessKeySecret:" + accessKeySecret,
                  "endpoint:" + endpoint,
                  "bucketName:" + bucketName,
                  "partSize:" + partSize,
                  "LimitFileSize:" + LimitFileSize
                  );
                //LogManager.Error(ex, string.Format("Failed with error code: {0}; Error info: {1}. \nRequestID:{2}\tHostID:{3} FileName:{4} FilePath:{5} 报错方法 UploadMultipart(string FileName, string FilePath) ",ex.ErrorCode, ex.Message, ex.RequestId, ex.HostId, FileName, FilePath) + baseinfo);
                return "";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed with error info: {0}", ex.Message);
                var baseinfo = string.Format("baseinfo({0} {1} {2} {3} {4} {5})",
                  "accessKeyId:" + accessKeyId,
                  "accessKeySecret:" + accessKeySecret,
                  "endpoint:" + endpoint,
                  "bucketName:" + bucketName,
                  "partSize:" + partSize,
                  "LimitFileSize:" + LimitFileSize
                  );
                //LogManager.Error(ex, string.Format("报错时间{0} FileName:{1} FilePath:{2} 报错方法{3}", DateTime.Now, FileName, FilePath, "UploadMultipart(string FileName, string FilePath) ") + baseinfo);
                return "";
            }
        }
        private static string InitiateMultipartUpload(String bucketName, String objectName)
        {
            var request = new InitiateMultipartUploadRequest(bucketName, objectName);
            var result = client.InitiateMultipartUpload(request);
            return result.UploadId;
        }
        private static List<PartETag> UploadParts(String bucketName, String objectName, String fileToUpload,
                                                String uploadId, int partSize)
        {
            var fi = new FileInfo(fileToUpload);
            var fileSize = fi.Length;
            var partCount = fileSize / partSize;
            if (fileSize % partSize != 0)
            {
                partCount++;
            }

            var partETags = new List<PartETag>();
            using (var fs = File.Open(fileToUpload, FileMode.Open))
            {
                for (var i = 0; i < partCount; i++)
                {
                    var skipBytes = (long)partSize * i;
                    fs.Seek(skipBytes, 0);
                    var size = (partSize < fileSize - skipBytes) ? partSize : (fileSize - skipBytes);
                    var request = new UploadPartRequest(bucketName, objectName, uploadId)
                    {
                        InputStream = fs,
                        PartSize = size,
                        PartNumber = i + 1
                    };

                    var result = client.UploadPart(request);
                    Console.WriteLine("oss:" + result.PartETag);

                    partETags.Add(result.PartETag);
                }
            }
            return partETags;
        }
        private static CompleteMultipartUploadResult CompleteUploadPart(String bucketName, String objectName,
          String uploadId, List<PartETag> partETags)
        {
            var completeMultipartUploadRequest =
                new CompleteMultipartUploadRequest(bucketName, objectName, uploadId);
            foreach (var partETag in partETags)
            {
                completeMultipartUploadRequest.PartETags.Add(partETag);
            }

            return client.CompleteMultipartUpload(completeMultipartUploadRequest);
        }
        #endregion

        #region 帮助方法
        /// <summary>
        /// 阿里云文件夹
        /// </summary>
        /// <returns></returns>
        public static string SetPath()
        {
            var Path = DateTime.Now.ToString("yyyyMM/");
            var listObjectsRequest = new ListObjectsRequest(bucketName);
            listObjectsRequest.Prefix = Path;
            var result = client.ListObjects(listObjectsRequest);
            if (result.CommonPrefixes.Count() != 0)
            {
                using (var stream = new MemoryStream())
                {
                    client.PutObject(bucketName, Path, stream);
                }
            }
            return Path;
        }
        #endregion
    }
}
