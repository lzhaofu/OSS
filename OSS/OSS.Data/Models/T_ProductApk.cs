using System;
using System.Collections.Generic;

namespace OSS.Data.Models
{
    public partial class T_ProductApk
    {
        public int ID { get; set; }
        public string PDAPKName { get; set; }
        public Nullable<int> ProductId { get; set; }
        public string ApkFileName { get; set; }
        public string LocalFilePath { get; set; }
        public string RemoteFilePath { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public Nullable<int> ScanFlag { get; set; }
    }
}
