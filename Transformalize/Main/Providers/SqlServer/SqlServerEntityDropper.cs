namespace Transformalize.Main.Providers.SqlServer {
    public class SqlServerEntityDropper : DatabaseEntityDropper {
        public SqlServerEntityDropper() : base(new SqlServerEntityExists()) { }
    }
}