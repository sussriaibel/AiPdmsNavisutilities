using Aveva.Core.Database;
using Aveva.Core.Database.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.ExceptionServices;
using System.Security;


namespace AiPdms.Navis.Utilities
{

    public class ExportScanFilter : BaseFilter
    {
        private readonly List<string> _exportTypes;
        private readonly DbAttribute _typeAttr;

        public ExportScanFilter(List<string> exportTypes)
        {
            _exportTypes = exportTypes ?? new List<string>();
            _typeAttr = DbAttribute.GetDbAttribute("Type");   // resolve ONCE, not per node
        }

        public override object Clone() => new ExportScanFilter(_exportTypes);

        // Yield every element so members still export - unchanged.
        public override bool Valid(DbElement element) => true;

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        public override bool ScanBelow(DbElement element)
        {
            try
            {
                string t = element.GetAsString(_typeAttr);
                if (string.IsNullOrEmpty(t)) return false;   // FIX: don't descend into empty/unknown nodes

                for (int i = 0; i < _exportTypes.Count; i++)
                {
                    string tok = _exportTypes[i];
                    if (!string.IsNullOrEmpty(tok) && tok.Contains(t)) return true;
                }
                return false;
            }
            catch
            {
                return false;   // unreadable/corrupt -> never descend (same guard as before)
            }
        }
    }

    public class NoidFilter : BaseFilter
    {
        public NoidFilter()
        {

        }


        public override object Clone()
        {
            return new NoidFilter();
        }

        public override bool ScanBelow(DbElement element)
        {
            return true;
        }

        public override bool Valid(DbElement element)
        {
            try
            {
                var xx = element.EvaluateAsString(DbExpression.Parse(":noid")).Length >= 2;

                return xx;
            }
            catch (Exception)
            {
                return false;
            }
        }

  
    }

    public class AttaFilter : BaseFilter
    {
        public AttaFilter()
        {


        }


        public override object Clone()
        {
            return new AttaFilter();
        }

        public override bool ScanBelow(DbElement element)
        {
            return true;
        }

        public override bool Valid(DbElement element)
        {
            try
            {
                //var xx = element.EvaluateAsString(DbExpression.Parse("name")).Substring(0, 12) == element.Owner.Owner.FullName();

                var xx = element.EvaluateAsString(DbExpression.Parse("name")).Contains(element.Owner.Owner.Name());
                    

                return xx;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
