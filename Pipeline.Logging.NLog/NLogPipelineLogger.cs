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
using System;
using System.IO;
using System.Linq;
using NLog;
using NLog.Targets;
using NLog.Targets.Wrappers;
using Pipeline.Context;
using Pipeline.Contracts;
using LogLevel = Pipeline.Contracts.LogLevel;
using NLogLevel = global::NLog.LogLevel;

namespace Pipeline.Logging.NLog {
    public class NLogPipelineLogger : IPipelineLogger {

        const string Context = "{0} | {1} | {2} | {3}";
        readonly Logger _log;

        public NLogPipelineLogger(string name) {
            var invalids = name.Intersect(Path.GetInvalidPathChars()).ToArray();
            if (invalids.Any()) {
                throw new ArgumentException("The log name contains invalid path characters: {0}.", string.Join(", ", invalids));
            }
            ReConfiguredLogLevel(name);
            _log = LogManager.GetLogger("TFL");
        }

        static void ReConfiguredLogLevel(string name) {

            var target = LogManager.Configuration.FindTargetByName("file");
            if (target != null) {
                FileTarget file;
                if (target is AsyncTargetWrapper) {
                    file = (FileTarget)((AsyncTargetWrapper)target).WrappedTarget;
                } else {
                    file = (FileTarget)target;
                }
                try {
                    var info = new FileInfo(file.FileName.Render(new LogEventInfo { TimeStamp = DateTime.Now }));
                    if (!info.Name.Contains(name)) {
                        file.FileName = Path.Combine(info.DirectoryName ?? string.Empty, name + "-" + info.Name);
                    }
                } catch (Exception) {
                    // eat it
                }
            }

            target = LogManager.Configuration.FindTargetByName("mail");
            if (target != null) {
                MailTarget mail;
                var wrapper = target as AsyncTargetWrapper;
                if (wrapper == null) {
                    mail = (MailTarget)target;
                } else {
                    mail = (MailTarget)wrapper.WrappedTarget;
                }
                var subject = mail.Subject.Render(new LogEventInfo { TimeStamp = DateTime.Now });
                if (!subject.Contains(name)) {
                    mail.Subject = subject + ": " + name;
                }
            }

            LogManager.ReconfigExistingLoggers();
        }

        static string ForLog(PipelineContext context) {
            return string.Format(Context, context.ForLog);
        }

        public LogLevel LogLevel
        {
            get
            {
                var cfg = LogManager.Configuration;
                if (cfg == null) {
                    return LogLevel.None;
                }
                foreach (var rule in LogManager.Configuration.LoggingRules) {
                    if (rule.Targets.Any(t => t.Name == "console")) {
                        var level = rule.Levels.FirstOrDefault();
                        return level == null ? LogLevel.None : TranslateLogLevel(level);
                    }
                }

                return LogLevel.Info;
            }
        }

        public void Debug(PipelineContext context, Func<string> lamda) {
            if (!_log.IsDebugEnabled)
                return;
            _log.Debug("debug | " + ForLog(context) + " | " + lamda());
        }

        public void Info(PipelineContext context, string message, params object[] args) {
            if (!_log.IsInfoEnabled)
                return;
            var custom = string.Format(message, args);
            _log.Info("info  | " + ForLog(context) + " | " + custom);
        }

        public void Warn(PipelineContext context, string message, params object[] args) {
            if (!_log.IsWarnEnabled)
                return;
            var custom = string.Format(message, args);
            _log.Warn("warn  | " + ForLog(context) + " | " + custom);
        }

        public void Error(PipelineContext context, string message, params object[] args) {
            if (!_log.IsErrorEnabled)
                return;
            var custom = string.Format(message, args);
            _log.Error("error | " + ForLog(context) + " | " + custom);
        }

        public void Error(PipelineContext context, Exception exception, string message, params object[] args) {
            if (!_log.IsErrorEnabled)
                return;
            var custom = string.Format(message, args);
            _log.Error(exception, "error | " + ForLog(context) + " | " + custom);
            if (exception.StackTrace != null) {
                _log.Error("error | " + ForLog(context) + " | " + exception.StackTrace.Replace(Environment.NewLine, "   "));
            }

        }

        public void Clear() {
            LogManager.Flush();
        }

        public LogLevel TranslateLogLevel(NLogLevel level) {
            switch (level.Name) {
                case "Debug":
                case "Trace":
                    return LogLevel.Debug;
                case "Error":
                    return LogLevel.Error;
                case "Warn":
                    return LogLevel.Warn;
                case "Off":
                    return LogLevel.None;
                default:
                    return LogLevel.Info;
            }

        }
    }
}
