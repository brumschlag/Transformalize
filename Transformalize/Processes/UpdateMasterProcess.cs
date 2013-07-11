using System;
using System.Linq;
using Transformalize.Model;
using Transformalize.Operations;
using Transformalize.Rhino.Etl.Core;

namespace Transformalize.Processes {

    public class UpdateMasterProcess : EtlProcess {

        private readonly Process _process;

        public UpdateMasterProcess(ref Process process) : base(process.Name) {
            _process = process;
        }

        protected override void Initialize() {
            foreach (var entity in _process.Entities) {
                Register(new EntityUpdateMaster(_process, entity.Value));
            }
        }

        protected override void PostProcessing() {

            var errors = GetAllErrors().ToArray();
            if (errors.Any()) {
                foreach (var error in errors) {
                    Error(error.InnerException, "Message: {0}\r\nStackTrace:{1}\r\n", error.Message, error.StackTrace);
                }
                throw new InvalidOperationException("Houstan.  We have a problem.");
            }

            base.PostProcessing();
        }

    }

}