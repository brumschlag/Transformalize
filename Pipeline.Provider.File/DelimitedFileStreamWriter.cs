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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileHelpers;
using FileHelpers.Dynamic;
using Pipeline.Context;
using Pipeline.Contracts;

namespace Pipeline.Provider.File {
    public class DelimitedFileStreamWriter : IWrite {

        private readonly OutputContext _context;
        private readonly Stream _stream;
        private readonly DelimitedClassBuilder _builder;
        private readonly string _delimiter;

        public DelimitedFileStreamWriter(OutputContext context, Stream stream) {
            _context = context;
            _stream = stream;
            _delimiter = string.IsNullOrEmpty(context.Connection.Delimiter) ? "," : context.Connection.Delimiter;

            _builder = new DelimitedClassBuilder(Utility.Identifier(context.Entity.OutputTableName(context.Process.Name))) {
                IgnoreEmptyLines = true,
                Delimiter = _delimiter,
                IgnoreFirstLines = 0
            };

            foreach (var field in context.OutputFields) {
                var fieldBuilder = _builder.AddField(field.FieldName(), typeof(string));
                fieldBuilder.FieldQuoted = true;
                fieldBuilder.QuoteChar = context.Connection.TextQualifier;
                fieldBuilder.QuoteMode = QuoteMode.OptionalForBoth;
                fieldBuilder.FieldOptional = field.Optional;
            }

        }

        public void Write(IEnumerable<IRow> rows) {

            ErrorMode errorMode;
            Enum.TryParse(_context.Connection.ErrorMode, true, out errorMode);

            FileHelperAsyncEngine engine;

            if (_context.Connection.Header == Constants.DefaultSetting) {
                var headerText = string.Join(_delimiter, _context.OutputFields.Select(f => f.Label.Replace(_delimiter, " ")));
                engine = new FileHelperAsyncEngine(_builder.CreateRecordClass()) { ErrorMode = errorMode, HeaderText = headerText };
            } else {
                engine = new FileHelperAsyncEngine(_builder.CreateRecordClass()) { ErrorMode = errorMode };
            }

            _context.Debug(() => "Writing to stream.");

            var writer = new StreamWriter(_stream);

            using (engine.BeginWriteStream(writer)) {
                foreach (var row in rows) {
                    for (var i = 0; i < _context.OutputFields.Length; i++) {
                        var field = _context.OutputFields[i];
                        switch (field.Type) {
                            case "byte[]":
                                engine[i] = Convert.ToBase64String((byte[])row[field]);
                                break;
                            case "string":
                                engine[i] = row[field];
                                break;
                            case "datetime":
                                engine[i] = row[field] is string ? Convert.ToDateTime(row[field]).ToString("o") : ((DateTime)row[field]).ToString("o");
                                break;
                            default:
                                engine[i] = row[field].ToString();
                                break;
                        }
                    }
                    engine.WriteNextValues();
                }
            }

        }
    }
}