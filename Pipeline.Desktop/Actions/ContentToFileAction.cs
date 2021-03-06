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
using System.IO;
using Pipeline.Configuration;
using Pipeline.Context;
using Pipeline.Contracts;

namespace Pipeline.Desktop.Actions {
    public class ContentToFileAction : IAction {
        private readonly PipelineContext _context;
        private readonly Action _action;

        public ContentToFileAction(PipelineContext context, Action action) {
            _context = context;
            _action = action;
        }

        public ActionResponse Execute() {
            var to = new FileInfo(_action.To);
            if (string.IsNullOrEmpty(_action.Content)) {
                _context.Warn("Nothing to write to {0}", to.Name);
            } else {
                _context.Info("Writing content to {0}", to.Name);
                File.WriteAllText(to.FullName, _action.Content);
            }
            return new ActionResponse();
        }
    }
}
