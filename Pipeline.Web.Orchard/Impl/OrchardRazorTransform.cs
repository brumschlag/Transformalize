
using Orchard.Templates.Services;
using Pipeline.Configuration;
using Pipeline.Contracts;
using Pipeline.Transforms;

namespace Pipeline.Web.Orchard.Impl {
    public class OrchardRazorTransform : BaseTransform {
        private readonly ITemplateProcessor _processor;

        private readonly Field[] _input;

        public OrchardRazorTransform(IContext context, ITemplateProcessor processor) : base(context) {
            _processor = processor;
            _input = MultipleInput();
        }

        public override IRow Transform(IRow row) {
            row[Context.Field] = Context.Field.Convert(_processor.Process(Context.Transform.Template, Context.Key, null, row.ToFriendlyExpandoObject(_input)));
            Increment();
            return row;
        }
    }
}