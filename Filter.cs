using Aveva.Core.Database;
using Aveva.Core.Database.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiPdms.Navis.Utilities
{
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
