using System;
using System.Collections.Generic;
using System.Text;

namespace BestHTTP.Addons.cURLParser.Editor.Utils
{
    public class DataContext
    {
        public List<ParsedOption> dataOptions = new List<ParsedOption>();
        public bool hadContentTypeHeader;
        public string addContentTypeIfNotPresent;

        public DataHandler dataHandler;

        public virtual void Apply(StringBuilder sb, GeneratorContext generatorContext)
        {
            if (dataHandler != null)
                dataHandler.Apply(sb, this, generatorContext);
        }
    }
}
