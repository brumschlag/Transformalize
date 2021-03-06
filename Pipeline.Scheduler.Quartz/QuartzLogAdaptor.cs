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
using Common.Logging.Simple;
using Common.Logging;

namespace Pipeline.Scheduler.Quartz {

    public class QuartzLogAdaptor : AbstractSimpleLoggerFactoryAdapter {

        readonly Contracts.IContext _context;

        public QuartzLogAdaptor(Contracts.IContext context, LogLevel level, bool showLevel, bool showDateTime, bool showLogName, string dateTimeFormat) :base(level, showDateTime, showLogName, showLevel, dateTimeFormat) {
            _context = context;
        }

        protected override ILog CreateLogger(string name, LogLevel level, bool showLevel, bool showDateTime, bool showLogName, string dateTimeFormat) {
            return new QuartzLogger(_context, level, showLevel, showDateTime, showLogName, dateTimeFormat);
        }
        
    }
}
