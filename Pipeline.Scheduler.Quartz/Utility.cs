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
using Common.Logging;

namespace Pipeline.Scheduler.Quartz {
    public static class Utility {
        public static LogLevel ConvertLevel(Contracts.LogLevel level) {
            switch (level) {
                case Contracts.LogLevel.Debug:
                    return LogLevel.Debug;
                case Contracts.LogLevel.Info:
                    return LogLevel.Info;
                case Contracts.LogLevel.Warn:
                    return LogLevel.Warn;
                case Contracts.LogLevel.Error:
                    return LogLevel.Error;
                case Contracts.LogLevel.None:
                    return LogLevel.Off;
                default:
                    return LogLevel.Info;
            }
        }
    }
}
