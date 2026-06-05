using Aveva.Core.Database;
using Aveva.Core.Database.Filters;
using System;
using System.Collections;
using System.Collections.Generic;

namespace AiPdms.Navis.Utilities
{
    public class E3DDBElementCollection : IDisposable
    {

        public List<DbElement> AllSitesAibel { get; set; } = new List<DbElement>();
        public DBElementCollection dbElementCollection { get; set; }

        List<string> PurposeList = new List<string>();

        public E3DDBElementCollection()
        {

        }

        public E3DDBElementCollection(List<string> purposeList)
        {
            this.PurposeList = purposeList;
        }

        public E3DDBElementCollection(DbElement site, List<string> exportTypes)
        {
            dbElementCollection = new DBElementCollection(site)
            {
                Filter = new ExportScanFilter(exportTypes),
                IncludeRoot = true
            };
        }

        ~E3DDBElementCollection()
        {

        }

        /// <summary>
        /// Collect all sites
        /// </summary>
        /// <returns></returns>

        public List<DbElement> CollectSites()
        {
            DbElement wlrd = MDB.CurrentMDB.GetFirstWorld(Aveva.Core.Database.DbType.Design);
            TypeFilter filter = new TypeFilter(DbElementTypeInstance.SITE);
                        
            dbElementCollection = new DBElementCollection(wlrd)
            {
                Filter = filter,
                IncludeRoot = true
            };

            DbAttribute sitePurpose = DbAttribute.GetDbAttribute("Purpose");

            foreach (DbElement dbItem in dbElementCollection)
            {
                if (!dbItem.IsValid || dbItem.IsNull)
                {
                    continue;
                }

               
                if (dbItem.IsAttributeValid(sitePurpose))
                {
                    string sitePurposeString = dbItem.GetAsString(sitePurpose);
                    if (sitePurposeString == "ARC")
                    {
                        AllSitesAibel.Add(dbItem);
                    }
                    else if (sitePurposeString == "ELE")
                    {
                        AllSitesAibel.Add(dbItem);
                    }
                    else if (sitePurposeString == "HS")
                    {
                        AllSitesAibel.Add(dbItem);
                    }
                    else if (sitePurposeString == "HVA")
                    {
                        AllSitesAibel.Add(dbItem);
                    }
                    else if (sitePurposeString == "INS")
                    {
                        AllSitesAibel.Add(dbItem);
                    }
                    else if (sitePurposeString == "MEC")
                    {
                        AllSitesAibel.Add(dbItem);
                    }
                    else if (sitePurposeString == "PIP")
                    {
                        AllSitesAibel.Add(dbItem);
                    }
                    else if (sitePurposeString == "SAF")
                    {
                        AllSitesAibel.Add(dbItem);
                    }
                    else if (sitePurposeString == "STL")
                    {
                        AllSitesAibel.Add(dbItem);
                    }
                    else if (sitePurposeString == "TEL")
                    {
                        AllSitesAibel.Add(dbItem);
                    }
                }

            }//foreach end

            return AllSitesAibel;

        }//method end

        public List<DbElement> CollectSitesWithSelectedPurpose()
        {
            DbElement wlrd = MDB.CurrentMDB.GetFirstWorld(Aveva.Core.Database.DbType.Design);
            TypeFilter filter = new TypeFilter(DbElementTypeInstance.SITE);

            dbElementCollection = new DBElementCollection(wlrd)
            {
                Filter = filter,
                IncludeRoot = true
            };

            DbAttribute sitePurpose = DbAttribute.GetDbAttribute("Purpose");

            foreach (DbElement dbItem in dbElementCollection)
            {
                if (!dbItem.IsValid || dbItem.IsNull)
                {
                    continue;
                }


                if (dbItem.IsAttributeValid(sitePurpose))
                {
                    string sitePurposeString = dbItem.GetAsString(sitePurpose);
                    if (PurposeList.Contains(sitePurposeString))
                    {
                        AllSitesAibel.Add(dbItem);
                    }
   
                }

            }//foreach end

            return AllSitesAibel;

        }//method end

        public void Dispose()
        {
            dbElementCollection = null;
        }
    }//class end

}//namespace end
