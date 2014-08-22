using Transformalize.Main.Providers.SqlServer;

namespace Transformalize.Main.Providers.SqlCe {
    public class SqlCeDependencies : AbstractConnectionDependencies {
        public SqlCeDependencies()
            : base(
                new SqlCeTableQueryWriter(),
                new SqlCeConnectionChecker(),
                new SqlCeEntityRecordsExist(),
                new SqlCeEntityDropper(),
                new SqlCeEntityCreator(),
                new FalseViewWriter(),
                new SqlCeTflWriter(),
                new DatabaseScriptRunner(),
                new SqlServerDataTypeService()
            ) { }
    }
}