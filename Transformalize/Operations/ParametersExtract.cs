using System.Collections.Generic;
using System.Data;
using Transformalize.Model;
using Transformalize.Rhino.Etl.Core;
using Transformalize.Rhino.Etl.Core.Operations;
using System.Linq;

namespace Transformalize.Operations {
    public class ParametersExtract : InputCommandOperation {
        private readonly string _sql;
        private Dictionary<string, Field> _parameters;

        private string BuildSql(Process process) {
            _parameters = process.Parameters;
            var fields = new FieldSqlWriter(process.Parameters).Alias().Write();
            var tflWhereClause = string.Format(" WHERE [TflBatchId] IN ({0})", string.Join(", ", process.Entities.Select(kv=>kv.Value.TflBatchId)));
            var sql = string.Format("SELECT [TflKey], {0} FROM {1}{2};", fields, process.View, tflWhereClause);
            Debug("{0} | SQL:\r\n{1}", process.Name, sql);
            return sql;
        }

        public ParametersExtract(Process process)
            : base(process.MasterEntity.OutputConnection.ConnectionString) {
            UseTransaction = false;
            _sql = BuildSql(process);
        }

        protected override Row CreateRowFromReader(IDataReader reader) {
            var row = new Row();
            var index = 1;
            row["TflKey"] = reader.GetValue(0);
            foreach (var p in _parameters) {
                row[p.Key] = reader.GetValue(index);
                index++;
            }
            return row;
        }

        protected override void PrepareCommand(IDbCommand cmd) {
            cmd.CommandText = _sql;
            cmd.CommandTimeout = 0;
        }
    }
}