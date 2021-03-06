﻿#region license
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
using System.Linq;
using Pipeline.Configuration;
using Pipeline.Context;
using Pipeline.Contracts;
using Pipeline.Extensions;

namespace Pipeline.Provider.Ado {
    public class AdoEntityWriter : IWrite {
        private readonly OutputContext _output;
        private readonly Field[] _keys;
        private readonly ITakeAndReturnRows _matcher;
        private readonly IWrite _updater;
        private readonly IWrite _inserter;

        public AdoEntityWriter(OutputContext output, ITakeAndReturnRows matcher, IWrite inserter, IWrite updater) {
            _output = output;
            _keys = output.Entity.GetPrimaryKey();
            _matcher = matcher;
            _inserter = inserter;
            _updater = updater;
        }

        public void Write(IEnumerable<IRow> rows) {
            var tflHashCode = _output.Entity.TflHashCode();
            var tflDeleted = _output.Entity.TflDeleted();

            foreach (var part in rows.Partition(_output.Entity.InsertSize)) {

                var inserts = new List<IRow>(_output.Entity.InsertSize);
                var updates = new List<IRow>(_output.Entity.InsertSize);
                var batchCount = 0;

                if (_output.Entity.IsFirstRun) {
                    foreach (var row in part) {
                        inserts.Add(row);
                        batchCount++;
                    }
                } else {
                    var batch = part.ToArray();
                    var matching = _matcher.Read(batch).ToArray();
                    for (int i = 0, batchLength = batch.Length; i < batchLength; i++) {
                        var row = batch[i];
                        var match = matching.FirstOrDefault(f => f.Match(_keys, row));
                        if (match == null) {
                            inserts.Add(row);
                        } else {
                            if (match[tflDeleted].Equals(true) || !match[tflHashCode].Equals(row[tflHashCode])) {
                                updates.Add(row);
                            }
                        }
                        batchCount++;
                    }
                }

                if (inserts.Any()) {
                    _inserter.Write(inserts);
                }

                if (updates.Any()) {
                    _updater.Write(updates);
                }

                _output.Increment(batchCount);
            }

            _output.Info("{0} inserts into {1}", _output.Entity.Inserts, _output.Connection.Name);

            _output.Info("{0} updates to {1}", _output.Entity.Updates, _output.Connection.Name);
        }
    }
}
