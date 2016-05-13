﻿#region license
// Transformalize
// A Configurable ETL solution specializing in incremental denormalization.
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
using System.Dynamic;
using Pipeline.Configuration;

namespace Pipeline.Contracts {
    public interface IRow {
        object this[IField field] { get; set; }
        string GetString(IField field);
        void SetString(IField field, string value);
        string ToString();
        ExpandoObject ToExpandoObject(Field[] fields);
        Dictionary<string, string> ToStringDictionary(Field[] fields);
        ExpandoObject ToFriendlyExpandoObject(Field[] fields);
        IEnumerable<object> ToEnumerable(Field[] fields);
        bool Match(Field[] fields, IRow other);
    }
}