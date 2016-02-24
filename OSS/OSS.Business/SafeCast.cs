using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSS.Business
{
    public class SafeCast
    {
        #region Contructor

        /// <summary>
        /// 静态构造函数（该类不需要被实例化）
        /// </summary>
        static SafeCast()
        {

        }

        #endregion

        #region Method

        #region SafeCastString

        /// <summary>
        /// 将Object类型数据安全转换为String类型数据
        /// </summary>
        /// <param name="objStr">Object类型数据</param>
        /// <returns>返回String类型数据</returns>
        public static string SafeCastString(object objStr)
        {
            string strResult = string.Empty;

            if (null == objStr)
            {
                return "";
            }

            try
            {
                strResult = objStr.ToString();
            }
            catch { }

            return strResult;
        }

        #endregion

        #region SafeCastInt

        /// <summary>
        /// 将Object类型数据安全转换为Int类型数据
        /// </summary>
        /// <param name="objInt">Object类型数据</param>
        /// <returns>返回Int类型数据（转换失败，默认值为0）</returns>
        public static int SafeCastInt(object objInt)
        {
            return SafeCastInt(objInt, 0);
        }

        /// <summary>
        /// 将Object类型数据安全转换为Int类型数据
        /// </summary>
        /// <param name="objInt">Object类型数据</param>
        /// <param name="nDefault">转换失败默认值</param>
        /// <returns>返回Int类型数据</returns>
        public static int SafeCastInt(object objInt, int nDefault)
        {
            int nValue = nDefault;

            try
            {
                nValue = Int32.Parse(objInt.ToString());
            }
            catch (Exception)
            {
                nValue = nDefault;
            }

            return nValue;
        }

        #endregion

        #region SafeCastBool

        /// <summary>
        /// 将Object类型数据安全转换为Bool类型数据
        /// </summary>
        /// <param name="objBool">Object类型数据</param>
        /// <returns>返回Bool类型数据（转换失败，默认值为'False'）</returns>
        public static bool SafeCastBool(object objBool)
        {
            return SafeCastBool(objBool, false);
        }

        /// <summary>
        /// 将Object类型数据安全转换为Bool类型数据
        /// </summary>
        /// <param name="objBool">Object类型数据</param>
        /// <param name="bDefault">转换失败默认值</param>
        /// <returns>返回Bool类型数据</returns>
        public static bool SafeCastBool(object objBool, bool bDefault)
        {
            bool bValue = bDefault;

            try
            {
                bValue = bool.Parse(objBool.ToString());
            }
            catch (Exception)
            {
                bValue = bDefault;
            }

            return bValue;
        }

        #endregion

        #region SafeCastFloat

        /// <summary>
        /// 将Object类型数据安全转换为Float类型数据
        /// </summary>
        /// <param name="objFloat">Object类型数据</param>
        /// <returns>返回Float类型数据（转换失败，默认值为0）</returns>
        public static float SafeCastFloat(object objFloat)
        {
            return SafeCastFloat(objFloat, 0);
        }

        /// <summary>
        /// 将Object类型数据安全转换为Float类型数据
        /// </summary>
        /// <param name="objFloat">Object类型数据</param>
        /// <param name="fDefault">转换失败默认值</param>
        /// <returns>返回Float类型数据</returns>
        public static float SafeCastFloat(object objFloat, float fDefault)
        {
            float fValue = fDefault;

            try
            {
                fValue = float.Parse(objFloat.ToString());
            }
            catch (Exception)
            {
                fValue = fDefault;
            }

            return fValue;
        }

        #endregion

        #region SafeCastDouble

        /// <summary>
        /// 将Object类型数据安全转换为Double类型数据
        /// </summary>
        /// <param name="objDouble">Object类型数据</param>
        /// <returns>返回Double类型数据（转换失败，默认值为0）</returns>
        public static double SafeCastDouble(object objDouble)
        {
            return SafeCastDouble(objDouble, 0);
        }

        /// <summary>
        /// 将Object类型数据安全转换为Double类型数据
        /// </summary>
        /// <param name="objDouble">Object类型数据</param>
        /// <param name="dDefault">转换失败默认值</param>
        /// <returns>返回Double类型数据</returns>
        public static double SafeCastDouble(object objDouble, double dDefault)
        {
            double dValue = dDefault;

            try
            {
                dValue = double.Parse(objDouble.ToString());
            }
            catch (Exception)
            {
                dValue = dDefault;
            }

            return dValue;
        }

        #endregion

        #region SafeCastLong

        /// <summary>
        /// 将Object类型数据安全转换为Long类型数据
        /// </summary>
        /// <param name="objLong">Object类型数据</param>
        /// <returns>返回Long类型数据（转换失败，默认值为0）</returns>
        public static long SafeCastLong(object objLong)
        {
            return SafeCastLong(objLong, 0);
        }

        /// <summary>
        /// 将Object类型数据安全转换为Long类型数据
        /// </summary>
        /// <param name="objLong">Object类型数据</param>
        /// <param name="dDefault">转换失败默认值</param>
        /// <returns>返回Long类型数据</returns>
        public static long SafeCastLong(object objLong, long dDefault)
        {
            long lValue = dDefault;

            try
            {
                lValue = long.Parse(objLong.ToString());
            }
            catch (Exception)
            {
                lValue = dDefault;
            }

            return lValue;
        }

        #endregion

        #region SafeCastDateTime

        /// <summary>
        /// 将Object类型数据安全转换为DateTime类型数据
        /// </summary>
        /// <param name="objDt">Object类型数据</param>
        /// <returns>返回DateTime类型数据</returns>
        public static DateTime SafeCastDateTime(object objDt)
        {
            DateTime dtValue = DateTime.MinValue;

            try
            {
                dtValue = DateTime.Parse(objDt.ToString());
            }
            catch { }

            return dtValue;
        }

        #endregion

        #region SafeCastDecimal

        /// <summary>
        /// 将Object类型数据安全转换为Decimal类型数据
        /// </summary>
        /// <param name="objInt">Object类型数据</param>
        /// <returns>返回Decimal类型数据（转换失败，默认值为0）</returns>
        public static decimal SafeCastDecimal(object objInt)
        {
            return SafeCastDecimal(objInt, 0);
        }

        /// <summary>
        /// 将Object类型数据安全转换为Decimal类型数据
        /// </summary>
        /// <param name="objInt">Object类型数据</param>
        /// <param name="nDefault">转换失败默认值</param>
        /// <returns>返回Decimal类型数据</returns>
        public static decimal SafeCastDecimal(object objInt, decimal nDefault)
        {
            decimal nValue = nDefault;

            try
            {
                nValue = decimal.Parse(objInt.ToString());
            }
            catch (Exception)
            {
                nValue = nDefault;
            }

            return nValue;
        }

        #endregion

        #region 把数组string[]按照分隔符转换成string
        /// <summary>
        /// 把数组string[]按照分隔符转换成string
        /// </summary>
        /// <param name="list"></param>
        /// <param name="speater"></param>
        /// <returns></returns>
        public static string GetArrayStr(string[] list, string speater = ",")
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < list.Length; i++)
            {
                if (i == list.Length - 1)
                {
                    sb.Append(list[i]);
                }
                else
                {
                    sb.Append(list[i]);
                    sb.Append(speater);
                }
            }
            return sb.ToString();
        }
        #endregion

        #region  dataTable某列转换为字符串
        /// <summary>
        /// dataTable某列转换为字符
        /// </summary>
        /// <param name="dataTable">数据表</param>
        /// <param name="name">列名</param>
        /// <returns>字段值数组</returns>
        public static string FieldToString(DataTable dataTable, string name)
        {
            int rowCount = 0;
            string stringList = "";

            if (dataTable == null)
            {
                return "";
            }

            foreach (DataRow dataRow in dataTable.Rows)
            {
                if (!string.IsNullOrEmpty(dataRow[name].ToString()))
                {
                    rowCount++;
                    stringList += dataRow[name].ToString() + ",";
                }
            }
            if (rowCount > 0)
            {
                stringList = stringList.TrimEnd(',');
            }
            return stringList;
        }
        #endregion

        #region 移除SQL敏感词
        /// <summary>
        /// 移除SQL敏感词 返回新字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ReplaceSQLFilter(string str)
        {
            //移除空格
            str = SafeCastString(str).Trim();

            if (string.IsNullOrEmpty(str))
            {
                return "";
            }
            string[] pattern = { "'", "--", "select", "insert", "delete", "from", "count\\(", "drop table", "update", "truncate", "asc\\(", "mid\\(", "char\\(", "xp_cmdshell", "exec   master", "netlocalgroup administrators", "net user", "or", "and" };
            for (int i = 0; i < pattern.Length; i++)
            {
                str = str.Replace(pattern[i].ToString(), "");
            }
            return str;
        }
        #endregion

        #endregion
    }
}
