#region license
// Transformalize
// A Configurable ETL Solution Specializing in Incremental Denormalization.
// Copyright 2013 Dale Newman
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//       http://www.apache.org/licenses/LICENSE-2.0
//   
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
using System.Collections.Generic;
using Pipeline.Context;
using Pipeline.Contracts;
using SolrNet;
using SolrNet.Commands.Parameters;

namespace Pipeline.Provider.Solr {
    public class SolrOutputVersionDetector : IOutputVersionDetector {

        private readonly OutputContext _context;
        private readonly ISolrReadOnlyOperations<Dictionary<string, object>> _solr;

        public SolrOutputVersionDetector(OutputContext context, ISolrReadOnlyOperations<Dictionary<string, object>> solr) {
            _context = context;
            _solr = solr;
        }

        public object Detect() {
            if (string.IsNullOrEmpty(_context.Entity.Version))
                return null;

            var version = _context.Entity.GetVersionField();

            _context.Debug(() => $"Detecting Max Output Version: {_context.Connection.Database}.{version.Alias.ToLower()}.");

            var result = _solr.Query(
                new SolrQueryByField(_context.Entity.TflDeleted().Alias, "false"),
                new QueryOptions {
                    StartOrCursor = new StartOrCursor.Start(0),
                    Rows = 1,
                    Fields = new List<string> { version.Alias },
                    OrderBy = new List<SortOrder> { new SortOrder(version.Alias, Order.DESC) }
                });

            var value = result.NumFound > 0 ? result[0][version.Alias] : null;
            if (value != null && value.GetType() != Constants.TypeSystem()[version.Type]) {
                value = version.Convert(value);
            }
            _context.Debug(() => $"Found value: {value ?? "null"}");
            return value;
        }
    }
}