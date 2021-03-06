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
using System.Linq;
using NUnit.Framework;

namespace Pipeline.Test {

    [TestFixture]
    public class MatchTransform {

        [Test(Description = "Match Transformation")]
        public void Match() {

            const string xml = @"
    <add name='TestProcess'>
      <entities>
        <add name='TestData' pipeline='linq'>
          <rows>
            <add Field1='Local/3114@tolocalext-fc5d,1' />
            <add Field1='SIP/Generic-Vitel_Outbound-0002b45a' />
          </rows>
          <fields>
            <add name='Field1' />
          </fields>
          <calculated-fields>
            <add name='MatchField1' t='copy(Field1).match(3[0-9]{3}(?=@))' default='None' />
            <add name='MatchAnyField' t='copy(Field1,Field2).match(3[0-9]{3}(?=@))' default='None' />
          </calculated-fields>
        </add>
      </entities>
    </add>";

            var composer = new CompositionRoot();
            var controller = composer.Compose(xml);
            var output = controller.Read().ToArray();

            var cf = composer.Process.Entities.First().CalculatedFields.ToArray();
            Assert.AreEqual("3114", output[0][cf[0]]);
            Assert.AreEqual("3114", output[0][cf[1]]);
            Assert.AreEqual("None", output[1][cf[0]]);
            Assert.AreEqual("None", output[1][cf[1]]);
        }
    }
}
