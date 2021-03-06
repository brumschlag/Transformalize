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
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Cfg.Net.Contracts;
using Pipeline.Context;
using Pipeline.Contracts;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RazorEngine.Text;

namespace Pipeline.Template.Razor {
    public class RazorTemplateEngine : ITemplateEngine {

        private readonly PipelineContext _context;
        private readonly Configuration.Template _template;

        // Using Cfg-NET's "Reader" to read content, files, or web addresses with possible parameters.
        private readonly IReader _templateReader;
        private readonly IRazorEngineService _service;

        public RazorTemplateEngine(PipelineContext context, Configuration.Template template, IReader templateReader) {

            _context = context;
            _template = template;
            _templateReader = templateReader;

            var config = new TemplateServiceConfiguration {
                EncodedStringFactory = _template.ContentType == "html" ? (IEncodedStringFactory)new HtmlEncodedStringFactory() : new RawStringFactory(),
                Language = Language.CSharp,
                CachingProvider = new DefaultCachingProvider(t => { })
            };

            _service = RazorEngineService.Create(config);
        }

        public string Render() {

            var p = new Dictionary<string, string>();
            var l = new Cfg.Net.Loggers.MemoryLogger();

            // get template
            _context.Debug(() => $"Reading {_template.File}");
            var templateContent = _templateReader.Read(_template.File, p, l);

            if (l.Errors().Any()) {
                foreach (var error in l.Errors()) {
                    _context.Error(error);
                }
                return string.Empty;
            }

            // get parameters (other than process)
            var parameters = new ExpandoObject();
            foreach (var parameter in _template.Parameters) {
                ((IDictionary<string, object>)parameters).Add(parameter.Name, parameter.Value);
            }
            if (p.Any()) {
                foreach (var parameter in p) {
                    ((IDictionary<string, object>)parameters)[parameter.Key] = parameter.Value;
                }
            }

            try {
                _context.Debug(() => $"Compiling {_template.Name}.");
                return _service.RunCompile(templateContent, _template.Name, null, new {
                    _context.Process,
                    Parameters = parameters
                });
            } catch (Exception ex) {
                _context.Error($"Error parsing template {_template.Name}.");
                _context.Error(ex, ex.Message.Replace("{", "{{").Replace("}", "}}"));
                return string.Empty;
            }
        }

    }
}
