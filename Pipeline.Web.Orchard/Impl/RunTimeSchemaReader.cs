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

using System.Linq;
using Autofac;
using Orchard.FileSystems.AppData;
using Orchard.Templates.Services;
using Pipeline.Configuration;
using Pipeline.Contracts;
using Pipeline.Nulls;
using Pipeline.Web.Orchard.Modules;

namespace Pipeline.Web.Orchard.Impl {

    public class RunTimeSchemaReader : IRunTimeSchemaReader {

        public Process Process { get; set; }
        public Schema Read(Process process) {
            Process = process;
            return Read();
        }

        private readonly IContext _host;
        private readonly IAppDataFolder _appDataFolder;
        private readonly ITemplateProcessor _templateProcessor;

        public RunTimeSchemaReader(IContext host, IAppDataFolder appDataFolder, ITemplateProcessor templateProcessor) {
            _host = host;
            _appDataFolder = appDataFolder;
            _templateProcessor = templateProcessor;
        }

        public RunTimeSchemaReader(Process process, IContext host, IAppDataFolder appDataFolder) {
            Process = process;
            _host = host;
            _appDataFolder = appDataFolder;
        }

        public Schema Read(Entity entity) {
            if (Process == null) {
                _host.Error("RunTimeSchemaReader executed without a Process");
                return new Schema();
            }

            var container = new ContainerBuilder();
            container.RegisterInstance(_host.Logger).SingleInstance();
            container.Register(c => new Process(
                new NullValidator("sh"),
                new NullNodeModifier("sh")
            )).As<Process>();
            container.RegisterCallback(new ContextModule(Process).Configure);
            container.RegisterCallback(new AdoModule(Process).Configure);
            container.RegisterCallback(new SolrModule(Process).Configure);
            container.RegisterCallback(new ElasticModule(Process).Configure);
            container.RegisterCallback(new InternalModule(Process).Configure);
            container.RegisterCallback(new FileModule(Process, _appDataFolder, _templateProcessor).Configure);
            container.RegisterCallback(new ExcelModule(Process, _appDataFolder, _templateProcessor).Configure);
            container.RegisterCallback(new WebModule(Process).Configure);

            using (var scope = container.Build().BeginLifetimeScope()) {
                var reader = scope.ResolveNamed<ISchemaReader>(Process.Connections.First().Key);
                return reader.Read(entity);
            }
        }

        public Schema Read() {

            if (Process == null) {
                _host.Error("RunTimeSchemaReader executed without a Process");
                return new Schema();
            }

            var container = new ContainerBuilder();
            container.RegisterInstance(_host.Logger).SingleInstance();
            container.RegisterCallback(new ContextModule(Process).Configure);

            container.RegisterCallback(new AdoModule(Process).Configure);
            container.RegisterCallback(new SolrModule(Process).Configure);
            container.RegisterCallback(new InternalModule(Process).Configure);
            container.RegisterCallback(new FileModule(Process, _appDataFolder, _templateProcessor).Configure);
            container.RegisterCallback(new ExcelModule(Process, _appDataFolder, _templateProcessor).Configure);

            using (var scope = container.Build().BeginLifetimeScope()) {
                var reader = scope.ResolveNamed<ISchemaReader>(Process.Connections.First().Key);
                return Process.Entities.Count == 1 ? reader.Read(Process.Entities.First()) : reader.Read();
            }

        }
    }
}