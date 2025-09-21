using System;
using System.Text;

namespace BestHTTP.Addons.cURLParser.Editor.Utils
{
    public class DataHandler
    {
        public virtual void Apply(StringBuilder sb, DataContext context, GeneratorContext generatorContext)
        {
            if (!context.hadContentTypeHeader && context.addContentTypeIfNotPresent != null)
                sb.AppendLine($"{generatorContext.indent}request.AddHeader(\"Content-Type\", \"{context.addContentTypeIfNotPresent}\");");
        }
    }

    public abstract class BaseFormDataHandler : DataHandler
    {
        public abstract string FormUsage { get; }

        public override void Apply(StringBuilder sb, DataContext context, GeneratorContext generatorContext)
        {
            if (context.hadContentTypeHeader)
            {
                // do nothing, going to overwrite it...
            }

            sb.AppendLine();
            sb.AppendLine($"{generatorContext.indent}request.FormUsage = HTTPFormUsage.{FormUsage};");

            foreach (var data in context.dataOptions)
            {
                int separatorIdx = data.value.IndexOf("=");
                string key = data.value.Substring(0, separatorIdx);
                string value = data.value.Substring(separatorIdx + 1);

                sb.AppendLine($"{generatorContext.indent}request.AddField(\"{key}\", \"{value}\");");
            }
        }
    }

    public class UrlEncodedFormDataHandler : BaseFormDataHandler
    {
        public override string FormUsage => "UrlEncoded";
    }

    public class RawUrlEncodedDataHandler : DataHandler
    {
        public override void Apply(StringBuilder sb, DataContext context, GeneratorContext generatorContext)
        {
            base.Apply(sb, context, generatorContext);

            string dataStr = null;
            if (context.dataOptions != null)
                for (int i = 0; i < context.dataOptions.Count; ++i)
                {
                    if (i > 0)
                        dataStr += "&";
                    dataStr += context.dataOptions[i].value;
                }

            if (dataStr != null)
            {
                sb.AppendLine();
                sb.AppendLine($"{generatorContext.indent}string data = @\"{dataStr.Replace("\\\"", "\"\"")}\";");
                sb.AppendLine($"{generatorContext.indent}request.RawData = System.Text.Encoding.UTF8.GetBytes(data);");
            }
        }
    }

    public class MultipartFormDataHandler : BaseFormDataHandler
    {
        public override string FormUsage => "Multipart";
    }
}
