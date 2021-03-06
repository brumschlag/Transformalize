﻿#region license
// Transformalize
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

using System.Collections.Generic;
using System.Linq;
using Autofac;
using Cfg.Net.Contracts;
using Cfg.Net.Environment;
using Cfg.Net.Ext;
using Cfg.Net.Reader;
using Cfg.Net.Shorthand;
using Orchard.FileSystems.AppData;
using Orchard.Templates.Services;
using Pipeline.Configuration;
using Pipeline.Context;
using Pipeline.Contracts;
using Pipeline.Nulls;
using Pipeline.Scripting.Jint;
using Pipeline.Web.Orchard.Impl;
//using Pipeline.Web.Orchard.Impl;
using IParser = Pipeline.Contracts.IParser;
using System;

// ReSharper disable PossibleMultipleEnumeration

namespace Pipeline.Web.Orchard.Modules {

    public class RootModule : Module {
        private readonly ITemplateProcessor _templateProcessor;

        public RootModule() {
        }

        public RootModule(ITemplateProcessor templateProcessor) {
            _templateProcessor = templateProcessor;
        }

        protected override void Load(ContainerBuilder builder) {

            if (_templateProcessor == null) {
                return; // being called on Orchart start up
            }

            builder.RegisterType<Cfg.Net.Serializers.XmlSerializer>().As<ISerializer>();
            builder.Register(ctx => new JintValidator("js")).Named<IValidator>("js");

            builder.Register(ctx => new EnvironmentModifier(
                new PlaceHolderModifier(),
                new ParameterModifier())
            ).As<IRootModifier>();

            // This reader is used to load the initial configuration and nested resources for tfl actions, etc.
            builder.RegisterType<FileReader>().Named<IReader>("file");
            builder.RegisterType<WebReader>().Named<IReader>("web");
            builder.Register<IReader>(ctx => new DefaultReader(
                ctx.ResolveNamed<IReader>("file"),
                new ReTryingReader(ctx.ResolveNamed<IReader>("web"), attempts: 3))
            );

            // transform choices
            builder.Register<ITransform>((ctx, p) => new JintTransform(p.TypedAs<PipelineContext>(), ctx.Resolve<IReader>())).Named<ITransform>("js");

            if (_templateProcessor != null) {
                builder.Register<ITransform>((ctx, p) => {
                    var c = p.TypedAs<PipelineContext>();
                    try {
                        _templateProcessor.Verify(c.Transform.Template);
                        return new OrchardRazorTransform(c, _templateProcessor);
                    } catch (Exception ex) {
                        c.Warn(ex.Message);
                        return new NullTransform(c);
                    }
                }).Named<ITransform>("razor");
            } else {
                builder.Register<ITransform>((ctx, p) => new NullTransform(p.TypedAs<PipelineContext>())).Named<ITransform>("razor");
            }

            // parser choices
            builder.RegisterType<JintParser>().Named<IParser>("js");

            // input row condition
            builder.Register<IRowCondition>((ctx, p) => new JintRowCondition(p.TypedAs<InputContext>(), p.TypedAs<string>())).As<IRowCondition>();

            builder.Register((ctx, p) => {

                var dependencies = new List<IDependency> {
                    ctx.Resolve<IReader>(),
                    ctx.Resolve<ISerializer>(),
                    new PlaceHolderModifier(),
                    ctx.Resolve<IRootModifier>(),
                    ctx.ResolveNamed<IValidator>("js"),
                    new PlaceHolderValidator()
                };

                if (!string.IsNullOrEmpty(ctx.ResolveNamed<string>("sh"))) {
                    var shr = new ShorthandRoot(ctx.ResolveNamed<string>("sh"), ctx.ResolveNamed<IReader>("file"));
                    if (shr.Errors().Any()) {
                        var context = ctx.IsRegistered<IContext>() ? ctx.Resolve<IContext>() : new PipelineContext(ctx.IsRegistered<IPipelineLogger>() ? ctx.Resolve<IPipelineLogger>() : new OrchardLogger(), new Process { Name = "Error" }.WithDefaults());
                        foreach (var error in shr.Errors()) {
                            context.Error(error);
                        }
                        context.Error("Please fix you shorthand configuration.  No short-hand is being processed.");
                        dependencies.Add(new NullValidator("sh"));
                        dependencies.Add(new NullNodeModifier("sh"));
                    } else {
                        dependencies.Add(new ShorthandValidator(shr, "sh"));
                        dependencies.Add(new ShorthandModifier(shr, "sh"));
                    }
                } else {
                    dependencies.Add(new NullValidator("sh"));
                    dependencies.Add(new NullNodeModifier("sh"));
                }

                var process = new Process(dependencies.ToArray());

                switch (p.Count()) {
                    case 2:
                        process.Load(
                            p.Named<string>("cfg"),
                            p.Named<Dictionary<string, string>>("parameters")
                        );
                        break;
                    default:
                        process.Load(p.Named<string>("cfg"));
                        break;
                }

                // this might be put into it's own type and injected (or not)
                if (process.Entities.Count == 1) {
                    var entity = process.Entities.First();
                    if (!entity.Fields.Any(f => f.Input) && ctx.IsRegistered<ISchemaReader>()) {
                        var schemaReader = ctx.Resolve<ISchemaReader>(new TypedParameter(typeof(Process), process));
                        var newEntity = schemaReader.Read(entity).Entities.First();
                        foreach (var sf in newEntity.Fields.Where(f => f.Name == Constants.TflKey || f.Name == Constants.TflDeleted || f.Name == Constants.TflBatchId || f.Name == Constants.TflHashCode)) {
                            sf.Alias = newEntity.Name + sf.Name;
                        }
                        process.Entities.Clear();
                        process.Entities.Add(newEntity);
                        process = new Process(process.Serialize(), ctx.Resolve<ISerializer>());
                    }
                }

                return process;
            }).As<Process>();

        }
    }
}
