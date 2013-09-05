﻿/*
Transformalize - Replicate, Transform, and Denormalize Your Data...
Copyright (C) 2013 Dale Newman

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using NUnit.Framework;
using Transformalize.Core;
using Transformalize.Core.Process_;
using Transformalize.Runner;

namespace Transformalize.Test.Integration {
    [TestFixture]
    public class Es {
        private const string CONFIGURATION_FILE = @"c:\etl\rhinoetl\tfl\Es.xml";

        [Test]
        public void MetaData()
        {
            var process = new ProcessReader(new ProcessXmlConfigurationReader(CONFIGURATION_FILE).Read()).Read();
            process.Options = new Options("{'mode':'metadata'}");
            new ProcessRunner(process).Run();
        }

        [Test]
        public void Init()
        {
            var process = new ProcessReader(new ProcessXmlConfigurationReader(CONFIGURATION_FILE).Read()).Read();
            process.Options = new Options { Mode = Modes.Initialize };
            new ProcessRunner(process).Run();
        }

        [Test]
        public void Normal()
        {
            var process = new ProcessReader(new ProcessXmlConfigurationReader(CONFIGURATION_FILE).Read()).Read();
            process.Options = new Options { Mode = Modes.Normal };
            new ProcessRunner(process).Run();
        }

        [Test]
        public void Test()
        {
            var process = new ProcessReader(new ProcessXmlConfigurationReader(CONFIGURATION_FILE).Read()).Read();
            process.Options = new Options("{'mode':'test','top':1,'loglevel':'trace'}");
            new ProcessRunner(process).Run();
        }



    }
}