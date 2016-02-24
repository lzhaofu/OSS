using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSS.Data.Models;

namespace OSS.Data.MainModule
{
    public class T_ProductApkRepository
    {
        public void ChangeScanningToWait()
        {
            using (var db = new APKDWContext())
            {
                db.Database.ExecuteSqlCommand("dbo.[NotScanningToWait]");
            }
        }

        /// <summary>
        /// 获取未更新列表
        /// </summary>
        /// <param name="topNum"></param>
        /// <returns></returns>
        public List<string> GetNotNoticeList(int topNum)
        {
            using (var db = new APKDWContext())
            {
                var list = new List<string>();
                var myQuery = (from x in db.T_ProductApk
                               where x.ScanFlag == 3
                               select x).Take<T_ProductApk>(topNum);
                list = myQuery.Select(x => x.ID.ToString()).ToList();
                return list;
            }
        }

        /// <summary>
        /// 批量更新未提醒列表
        /// </summary>
        /// <param name="idList"></param>
        public void BatchNotNoticeScaning(List<string> idList)
        {
            using (var db=new APKDWContext())
            {
                string list = string.Format("{0}", string.Join(",", idList.ToArray()));
                var para = new SqlParameter[]
                {
                    new SqlParameter
                    {
                        ParameterName    = "@ID",
                        SqlDbType = SqlDbType.VarChar,
                        Value = list.ToString()
                    }
                };
               db.Database.ExecuteSqlCommand("dbo.[NotNoticeBatchUpdateScanning] @ID", para);
            }
        }
    }
}
