using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace OSS.Data.Models
{
    public partial class APKDWContext : DbContext
    {
        static APKDWContext()
        {
            Database.SetInitializer<APKDWContext>(null);
        }

        public APKDWContext()
            : base("Name=APKDWContext")
        {
        }

        public DbSet<T_Product> T_Product { get; set; }
        public DbSet<T_ProductApk> T_ProductApk { get; set; }
        public DbSet<T_User> T_User { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // 移除EF的表名公约   
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            // 移除对MetaData表的查询验证 
            modelBuilder.Conventions.Remove<IncludeMetadataConvention>();

            //关闭外键级联删除
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
        }
    }
}
