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
using Pipeline.Configuration;
using Pipeline.Contracts;

namespace Pipeline {

    public class MapReaderAction : IAction {

        private readonly IContext _context;
        private readonly Map _map;
        private readonly IMapReader _mapReader;

        public MapReaderAction(IContext context, Map map, IMapReader mapReader) {
            _context = context;
            _map = map;
            _mapReader = mapReader;
        }

        public ActionResponse Execute() {
            var response = new ActionResponse();
            try {
                _map.Items.AddRange(_mapReader.Read(_context));
            } catch (Exception ex) {
                response.Code = 500;
                response.Content = "Could not read map " + _map.Name + ". Using query: " + _map.Query + ". Error: " + ex.Message;
            }
            return response;
        }
    }


    public class ActionResponse {
        public ActionResponse() {
        }
        public ActionResponse(string content) {
            Content = content;
        }
        public ActionResponse(int code) {
            Code = code;
        }
        public ActionResponse(int code, string content) {
            Code = code;
            Content = content;
        }

        public string Content { get; set; } = string.Empty;
        public int Code { get; set; } = 200;
    }
}
