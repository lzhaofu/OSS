using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSS.Data.MainModule;
using OSS.Data.Models;
using OSS.Data.Util;

namespace OSS.Business.MainModule
{
    public class T_ProductApkService
    {
        static object _myLock = new object();//排他锁
        private readonly T_ProductApkRepository _productApkRepository;
        public T_ProductApkService()
        {
            _productApkRepository = new T_ProductApkRepository();
        }

        /// <summary>
        /// 重置未扫描
        /// </summary>
        /// <returns></returns>
        public bool ResetNotScanningToWait()
        {
            try
            {
                using (var scope = TransactionUtilities.CreateTransactionScopeWithNoLock())
                {
                    _productApkRepository.ChangeScanningToWait();
                    scope.Complete();
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 获取未更新列表
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public List<string> GetNotNoticeList(int num)
        {
            var list = _productApkRepository.GetNotNoticeList(num);
            if (list.Any())
            {
                _productApkRepository.BatchNotNoticeScaning(list);
            }
            return list;
        }

        /// <summary>
        /// 提交未更新
        /// </summary>
        /// <param name="strId"></param>
        public void PostNotNotice(string strId)
        {
            var entity = new T_ProductApk();
            try
            {
                using (var scope = TransactionUtilities.CreateTransactionScopeWithNoLock())
                {
                    var db = new APKDWContext();
                    var id = int.Parse(strId);
                    entity = db.T_ProductApk.Find(id);
                    entity.ScanFlag = 1;
                    var sd=ALiYunOssHelp.Update("sd", @"C:\Users\donson\Desktop\QQ图片20160128113942.png");
                    entity.RemoteFilePath = sd;
                    db.Entry(entity).State = EntityState.Modified;
                    db.SaveChanges();
                    scope.Complete();
                }
            }
            catch (InvalidOperationException e)
            {
                lock (_myLock)
                {
                    //LoggerHelper.Log("【发起提醒】失败，失败原因:" + (e.InnerException == null ? e.Message : e.InnerException.ToString()));
                }
            }
            catch (Exception ex)
            {
                lock (_myLock)
                {
                    //LoggerHelper.Log("【未提醒】失败，失败原因:" + (ex.InnerException == null ? ex.Message : ex.InnerException.ToString()));
                    //避免数据库异常下，无法记录错误日志
                    var db = new APKDWContext();
                    entity.ScanFlag = 1;
                    db.Entry(entity).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
        }
    }
}
