#region license
// Transformalize
// A Configurable ETL Solution Specializing in Incremental Denormalization.
// Copyright 2013 Dale Newman
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System.Linq;
using Autofac;
using Cfg.Net.Contracts;
using Orchard.Templates.Services;
using Pipeline.Configuration;
using Pipeline.Context;
using Pipeline.Contracts;
using Pipeline.Nulls;
using Pipeline.Web.Orchard.Impl;

namespace Pipeline.Web.Orchard.Modules {
    public class TemplateModule : Module {
        private readonly Process _process;
        private readonly ITemplateProcessor _templateProcessor;

        public TemplateModule() { }

        public TemplateModule(Process process, ITemplateProcessor templateProcessor) {
            _process = process;
            _templateProcessor = templateProcessor;
        }

        protected override void Load(ContainerBuilder builder) {

            if (_process == null)
                return;

            foreach (var t in _process.Templates.Where(t => t.Enabled)) {
                var template = t;
                builder.Register<ITemplateEngine>(ctx => {
                    var context = new PipelineContext(ctx.Resolve<IPipelineLogger>(), _process);
                    context.Debug(() => $"Registering {template.Engine} Engine for {t.Key}");
                    switch (template.Engine) {
                        case "razor":
                            return new OrchardRazorTemplateEngine(context, _templateProcessor, template, ctx.Resolve<IReader>());
                        default:
                            return new NullTemplateEngine();
                    }
                }).Named<ITemplateEngine>(t.Key);
            }

        }
    }
}