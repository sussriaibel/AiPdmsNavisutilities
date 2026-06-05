using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Aveva.Core.Database;
using Aveva.Core.Database.Filters;
using Ps = Aveva.Core.Database.DbPseudoAttribute;
using System.Runtime.ExceptionServices;
using System.Security;

namespace AiPdms.Navis.Utilities
{
    public struct DictionaryKeyTuple<T1, T2>
    {
        public readonly T1 Item1;
        public readonly T2 Item2;

        public DictionaryKeyTuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
    }

    public class ExportAttibutes
    {
        public string FileWritePath { get; set; } = null;

        int pdDepth = -99;

        int currentElementDepth;

        DbElement lastElement = null;

        bool reachedLastElement = false;

        public List<string> LinesToWrite { get; set; } = new List<string>();

        public List<string> InvalidAttribures { get; set; } = new List<string>();

        public Dictionary<string, int> BranchTubeSequenceDictionary { get; set; } = new Dictionary<string, int>();

        public Dictionary<string, object[]> WeldedattachmentPrefabTaskSfref { get; set; } = new Dictionary<string, object[]>();

        //Dictionary to store pml 1 expression
        public Dictionary<string, DbExpression> PmlOneExPressionElementProp = new Dictionary<string, DbExpression>();

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        private bool TryEvaluate(DbElement el, DbExpression expr, out string result)
        {
            result = "";
            try
            {
                if (el == null || !el.IsValid || el.IsNull || el.IsDeleted) return false;
                result = el.EvaluateAsString(expr);
                return true;
            }
            catch (AccessViolationException ex)
            {
                Console.WriteLine("Skipping expression (AVE): " + ex.Message);
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public ExportAttibutes()
        {
            this.PmlOneExPressionElementProp.Add("Sequence", DbExpression.Parse("Sequence"));
            this.PmlOneExPressionElementProp.Add("SFREF", DbExpression.Parse("flnn of owner of sfref"));
            this.PmlOneExPressionElementProp.Add(":prefabtask", DbExpression.Parse(":prefabtask of owner of sfref"));
            this.PmlOneExPressionElementProp.Add(":workordno", DbExpression.Parse(":WorkOrdNo of supref"));
            this.PmlOneExPressionElementProp.Add("OWNE", DbExpression.Parse("OWNE"));
        }

        private string GetPurposeSafe(DbElement element)
        {
            try
            {
                if (element == null || !element.IsValid || element.IsNull || element.IsDeleted)
                    return "";

                DbAttribute purposeAttr = DbAttribute.GetDbAttribute("Purpose");

                if (element.ElementType == DbElementTypeInstance.SITE)
                {
                    if (element.IsAttributeValid(purposeAttr))
                        return element.GetAsString(purposeAttr);

                    return "";
                }

                var owners = element.GetElementArray(DbAttributeInstance.OWNLST);

                if (owners == null || owners.Length == 0)
                {
                    Console.WriteLine("No OWNLST found for: " + element.FullName());
                    return "";
                }

                DbElement topOwner = owners[0];

                if (topOwner == null || !topOwner.IsValid || topOwner.IsNull || topOwner.IsDeleted)
                    return "";

                if (topOwner.IsAttributeValid(purposeAttr))
                    return topOwner.GetAsString(purposeAttr);

                return "";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not get purpose for element: " + ex.Message);
                return "";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbElements"></param>
        /// <param name="attribLibrary"></param>
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        public void WriteAttributeToTextFile(List<DbElement> dbElements, AttribLibrary attribLibrary)
        {
           
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                double totalElapsedMinutes = 0;

                WriteHeadeeFooter(true);
                Console.WriteLine();
                Console.WriteLine("Export attributes started");
                Console.WriteLine("Total AibelSites = " + dbElements.Count);
                int siteCount = 1;

                // Compute ONCE - never changes per element.
                var exportElementTypes = attribLibrary.TypeOfElementsToExport.Distinct().ToList();

                foreach (DbElement dbelement in dbElements)
                {
                    if (dbelement != null && dbelement.IsValid && !dbelement.IsNull && !dbelement.IsDeleted)
                    {
                        // && site.Members().Length != 0

                        pdDepth = -99;
                        var stopwatchSite = new Stopwatch();
                        stopwatchSite.Start();
                        Console.WriteLine("Exporting attributes for site no. " + siteCount + " of " + dbElements.Count + " " + dbelement.FullName() + " started");
             
                        int siteDepth = Convert.ToInt32(dbelement.GetAsString(DbAttribute.GetDbAttribute("DDEPTH")));
                        string sitePurposeString = GetPurposeSafe(dbelement);


                        //DBElementCollection elementFromSites = new DBElementCollection(site)
                        //{
                        //    IncludeRoot = true
                        //};

                        using (E3DDBElementCollection elementCollection = new E3DDBElementCollection(dbelement, attribLibrary.TypeOfElementsToExport))
                        {
                            DbAttribute elementType = DbAttribute.GetDbAttribute("Type");

                            // Read the live DB cursor fully into a list BEFORE doing any
                            // per-element work. This stops the nested DBElementCollection
                            // enumerations in GetTubeSequence() and CabelLaderSegmentUpdater()
                            // from corrupting this parent cursor mid-iteration.
                            List<DbElement> snapshot = new List<DbElement>();
                            try
                            {
                                foreach (DbElement snapItem in elementCollection.dbElementCollection)
                                {
                                    snapshot.Add(snapItem);
                                }
                            }
                            catch (Exception exEnum)
                            {
                                Console.WriteLine("Enumeration stopped early for this site: " + exEnum.Message);
                                // proceed with whatever was collected so the file still gets written
                            }

                            foreach (DbElement item in snapshot)
                            {
                                //Console.WriteLine(item.Name());
                                try
                                { 
                                if (item != null && item.IsValid && !item.IsNull && !item.IsDeleted)
                                {
                                        string typeCurrentElement = item.GetAsString(elementType);

                                        string typeOwnerElement = "";
                                        try
                                        {
                                            if (item.Owner != null && item.Owner.IsValid && !item.Owner.IsNull && !item.Owner.IsDeleted)
                                            {
                                                typeOwnerElement = item.Owner.GetAsString(elementType);
                                            }
                                        }
                                        catch
                                        {
                                            typeOwnerElement = "";
                                        }

                                        var matchElementCurrent = !string.IsNullOrWhiteSpace(typeCurrentElement)
                                            ? exportElementTypes.FirstOrDefault(stringToCheck =>
                                                !string.IsNullOrWhiteSpace(stringToCheck) && stringToCheck.Contains(typeCurrentElement))
                                            : null;

                                        var matchElementOwner = !string.IsNullOrWhiteSpace(typeOwnerElement)
                                            ? exportElementTypes.FirstOrDefault(stringToCheck =>
                                                !string.IsNullOrWhiteSpace(stringToCheck) && stringToCheck.Contains(typeOwnerElement))
                                            : null;

                                        {
                                        
                                            if (typeCurrentElement == "SITE") //SITE
                                        {
                                            currentElementDepth = Convert.ToInt32(item.GetAsString(DbAttribute.GetDbAttribute("DDEPTH")));
                                            TextFileWriter(item, attribLibrary.SiteAttrib, currentElementDepth, siteDepth);
                                            lastElement = item;
                                        }
                                        else if (typeCurrentElement == "ZONE") //ZONE
                                        {
                                            currentElementDepth = Convert.ToInt32(item.GetAsString(DbAttribute.GetDbAttribute("DDEPTH")));
                                            TextFileWriter(item, attribLibrary.ZoneAttrib, currentElementDepth, siteDepth);
                                            lastElement = item;
                                        }
                                        else if (typeCurrentElement == "PIPE") //PIPE
                                        {
                                            currentElementDepth = Convert.ToInt32(item.GetAsString(DbAttribute.GetDbAttribute("DDEPTH")));
                                            TextFileWriter(item, attribLibrary.PipeAttrib, currentElementDepth, siteDepth);
                                            lastElement = item;
                                        }
                                        else if (typeCurrentElement == "HVAC") //HVAC
                                        {
                                            currentElementDepth = Convert.ToInt32(item.GetAsString(DbAttribute.GetDbAttribute("DDEPTH")));
                                            TextFileWriter(item, attribLibrary.HvacAttrib, currentElementDepth, siteDepth);
                                            lastElement = item;
                                        }
                                        else if (typeCurrentElement == "BRAN") //BRAN
                                        {
                                            this.BranchTubeSequenceDictionary = this.GetTubeSequence(item);

                                            currentElementDepth = Convert.ToInt32(item.GetAsString(DbAttribute.GetDbAttribute("DDEPTH")));
                                            TextFileWriter(item, attribLibrary.BranAttrib, currentElementDepth, siteDepth);
                                            lastElement = item;
                                        }
                                        else if (typeOwnerElement == "BRAN") //BRAN MEMBER
                                        {
                                            currentElementDepth = Convert.ToInt32(item.GetAsString(DbAttribute.GetDbAttribute("DDEPTH")));

                                            if (item.GetAsString(DbAttributeInstance.TYPE) == "TUBI")
                                            {
                                                currentElementDepth = 5;
                                            }
                                            TextFileWriter(item, attribLibrary.BranMembAttrib, currentElementDepth, siteDepth);

                                            if ((sitePurposeString == "ELE") || (sitePurposeString == "INS"))
                                            {
                                                LinesToWrite.Add(String.Format("\t{0}Segment:= '{1}'", CalculateIndentation(currentElementDepth), CabelLaderSegmentUpdater(item)));
                                            }

                                            lastElement = item;
                                        }
                                        else if (typeCurrentElement == "EQUI") //Equipment
                                        {
                                            currentElementDepth = Convert.ToInt32(item.GetAsString(DbAttribute.GetDbAttribute("DDEPTH")));
                                            TextFileWriter(item, attribLibrary.EquiAttrib, currentElementDepth, siteDepth);
                                            lastElement = item;
                                        }
                                        else if (typeCurrentElement == "STRU") //STRU
                                        {
                                            currentElementDepth = Convert.ToInt32(item.GetAsString(DbAttribute.GetDbAttribute("DDEPTH")));
                                            TextFileWriter(item, attribLibrary.StruAttrib, currentElementDepth, siteDepth);
                                            lastElement = item;
                                        }
                                        else if (typeCurrentElement == "FRMW") //FRMW
                                        {
                                            currentElementDepth = Convert.ToInt32(item.GetAsString(DbAttribute.GetDbAttribute("DDEPTH")));
                                            TextFileWriter(item, attribLibrary.FrmwAttrib, currentElementDepth, siteDepth);
                                            lastElement = item;
                                        }
                                        else if (typeCurrentElement == "SBFR") //SubFramework
                                        {
                                            currentElementDepth = Convert.ToInt32(item.GetAsString(DbAttribute.GetDbAttribute("DDEPTH")));
                                            TextFileWriter(item, attribLibrary.SbfrAttrib, currentElementDepth, siteDepth);
                                            lastElement = item;
                                        }
                                        else if (typeCurrentElement == "GENSEC") //GENSEC
                                        {
                                            var newDepth = SkipDepth(item, exportElementTypes);
                                            TextFileWriter(item, attribLibrary.GensecAttrib, newDepth, siteDepth);
                                            lastElement = item;
                                            currentElementDepth = newDepth;
                                        }
                                        else if (typeCurrentElement == "PANE") //PANE
                                        {
                                            var newDepth = SkipDepth(item, exportElementTypes);
                                            TextFileWriter(item, attribLibrary.PaneAttrib, newDepth, siteDepth);
                                            lastElement = item;
                                            currentElementDepth = newDepth;
                                        }
                                        else if (typeCurrentElement == "REST") //REST
                                        {
                                            currentElementDepth = Convert.ToInt32(item.GetAsString(DbAttribute.GetDbAttribute("DDEPTH")));
                                            TextFileWriter(item, attribLibrary.RestAttrib, currentElementDepth, siteDepth);
                                            lastElement = item;
                                        }
                                        else if (typeCurrentElement == "SUPPO") //SUPPO
                                        {
                                            currentElementDepth = Convert.ToInt32(item.GetAsString(DbAttribute.GetDbAttribute("DDEPTH")));
                                            TextFileWriter(item, attribLibrary.SuppoAttrib, currentElementDepth, siteDepth);
                                            lastElement = item;
                                        }
                                        else if (typeCurrentElement == "HANG") //HANG
                                        {
                                            currentElementDepth = Convert.ToInt32(item.GetAsString(DbAttribute.GetDbAttribute("DDEPTH")));
                                            TextFileWriter(item, attribLibrary.HangAttrib, currentElementDepth, siteDepth);
                                            lastElement = item;
                                        }
                                        else if (typeCurrentElement == "TRUNNI") //TRUNNI
                                        {
                                            currentElementDepth = Convert.ToInt32(item.GetAsString(DbAttribute.GetDbAttribute("DDEPTH")));
                                            TextFileWriter(item, attribLibrary.TrunniAttrib, currentElementDepth, siteDepth);
                                            lastElement = item;
                                        }
                                        else if (typeCurrentElement == "SUPC") //SUPC
                                        {
                                            currentElementDepth = Convert.ToInt32(item.GetAsString(DbAttribute.GetDbAttribute("DDEPTH")));
                                            TextFileWriter(item, attribLibrary.SupcAttrib, currentElementDepth, siteDepth);
                                            lastElement = item;
                                        }
                                        else if (typeCurrentElement == "PCLA" || typeCurrentElement == "TRANCI" || typeCurrentElement == "TRREDU" || typeCurrentElement == "PLAT" || typeCurrentElement == "ANCI") //PCLA, TRANCI, PLAT, TRREDU, 
                                        {
                                            currentElementDepth = Convert.ToInt32(item.GetAsString(DbAttribute.GetDbAttribute("DDEPTH")));
                                            TextFileWriter(item, attribLibrary.PclaAttrib, currentElementDepth, siteDepth);
                                            lastElement = item;

                                        }
                                        else if (typeCurrentElement == "STWALL") //STWALL
                                        {
                                            var newDepth = SkipDepth(item, exportElementTypes);
                                            TextFileWriter(item, attribLibrary.StwallAttrib, newDepth, siteDepth);
                                            lastElement = item;
                                            currentElementDepth = newDepth;
                                        }
                                        else if (typeCurrentElement == "FLOOR") //FLOOR
                                        {
                                            var newDepth = SkipDepth(item, exportElementTypes);
                                            TextFileWriter(item, attribLibrary.FloorAttrib, newDepth, siteDepth);
                                            lastElement = item;
                                            currentElementDepth = newDepth;
                                        }
                                        else if (typeCurrentElement == "WALL") //WALL
                                        {
                                            currentElementDepth = Convert.ToInt32(item.GetAsString(DbAttribute.GetDbAttribute("DDEPTH")));
                                            TextFileWriter(item, attribLibrary.WallAttrib, currentElementDepth, siteDepth);
                                            lastElement = item;
                                        }
                                        else if (typeCurrentElement == "SUBS") //SUBS
                                        {
                                            currentElementDepth = Convert.ToInt32(item.GetAsString(DbAttribute.GetDbAttribute("DDEPTH")));
                                            TextFileWriter(item, attribLibrary.SubsAttrib, currentElementDepth, siteDepth);
                                            lastElement = item;
                                        }
                                        else if (typeCurrentElement == "SUBE") //SUBE
                                        {
                                            currentElementDepth = Convert.ToInt32(item.GetAsString(DbAttribute.GetDbAttribute("DDEPTH")));
                                            TextFileWriter(item, attribLibrary.SubeAttrib, currentElementDepth, siteDepth);
                                            lastElement = item;
                                        }
                                        else if (typeOwnerElement == "SUBE") //SUBE MEMBER
                                        {
                                            currentElementDepth = Convert.ToInt32(item.GetAsString(DbAttribute.GetDbAttribute("DDEPTH")));
                                            TextFileWriter(item, attribLibrary.SubeMembAttrib, currentElementDepth, siteDepth);
                                            lastElement = item;
                                        }
                                        else if (typeOwnerElement == "EQUI" && typeCurrentElement != "SUBE") //EQUI MEMBERS
                                        {
                                            currentElementDepth = Convert.ToInt32(item.GetAsString(DbAttribute.GetDbAttribute("DDEPTH")));
                                            TextFileWriter(item, attribLibrary.EquiMembAttrib, currentElementDepth, siteDepth);
                                            lastElement = item;
                                        }
                                        else if (typeOwnerElement == "SUBS") //SUBS MEMBER
                                        {
                                            currentElementDepth = Convert.ToInt32(item.GetAsString(DbAttribute.GetDbAttribute("DDEPTH")));
                                            TextFileWriter(item, attribLibrary.SubsMembAttrib, currentElementDepth, siteDepth);
                                            lastElement = item;
                                        }
                                        else if (typeCurrentElement == "PNOD") //PNOD
                                        {
                                            var newDepth = SkipDepth(item, exportElementTypes);
                                            TextFileWriter(item, attribLibrary.PnodAttrib, newDepth, siteDepth);
                                            lastElement = item;
                                            currentElementDepth = newDepth;
                                        }

                                    }

                                }

                                //if (LinesToWrite.Count >= 50000)
                                //{
                                //    using (StreamWriter file = new System.IO.StreamWriter(FileWritePath, true))
                                //    {
                                //        foreach (var itemLine in LinesToWrite)
                                //        {
                                //            file.WriteLine(itemLine);
                                //        }

                                //    }

                                //    LinesToWrite.Clear();
                                //}
                                }
                                catch (AccessViolationException ex)
                                {
                                    Console.WriteLine("Skipping corrupt E3D element because AccessViolationException occurred.");
                                    Console.WriteLine(ex.Message);
                                    continue;
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Skipping E3D element because error occurred.");
                                    Console.WriteLine(ex.Message);
                                    continue;
                                }

                            }//foreach end
                        }

                        reachedLastElement = true;

                        if (lastElement != null)
                        {
                            int lastElementDepth = currentElementDepth;

                            if (lastElementDepth > siteDepth)
                            {
                                try
                                {
                                    TextFileWriter(lastElement, attribLibrary.FloorAttrib, lastElementDepth, siteDepth);
                                }
                                catch (AccessViolationException ex)
                                {
                                    Console.WriteLine("Skipping final lastElement because AccessViolationException occurred.");
                                    Console.WriteLine(ex.Message);
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("No exportable elements found under: " + dbelement.FullName());
                        }

                        lastElement = null;

                        stopwatchSite.Stop();

                        var elapsed_timeSite = stopwatchSite.Elapsed.TotalMinutes;

                        totalElapsedMinutes = elapsed_timeSite + totalElapsedMinutes;

                        double averageMinutes = totalElapsedMinutes / siteCount;

                        Console.WriteLine("Exporting attributes for site no. " + siteCount + " of " + dbElements.Count + " " + dbelement.FullName() + " ended" + " - Elapsed Minutes: " + elapsed_timeSite.ToString("0.00") + "- Total minutes: " + totalElapsedMinutes.ToString("0.00") + " Average Minutes: " + averageMinutes.ToString("0.00"));

                        siteCount++;
                        reachedLastElement = false;
                    }

                }  //foreach end 

                WriteHeadeeFooter(false);


                using (System.IO.StreamWriter file = new System.IO.StreamWriter(FileWritePath, true))
                {
                    foreach (var itemLine in LinesToWrite)
                    {
                        file.WriteLine(itemLine);
                    }

                }


                Console.WriteLine("Invalid Attributes");
                foreach (var attri in InvalidAttribures.Distinct())
                {
                    Console.WriteLine(attri);
                }

                stopwatch.Stop();
                var elapsed_time = stopwatch.Elapsed.TotalHours;
                Console.WriteLine("Total elapsed Hours = " + elapsed_time.ToString());
                Console.WriteLine("DO NOT CLOSE !");

            }
            catch (Exception ex1)
            {

                Console.WriteLine(ex1); 
            }

        }//method end

        private Dictionary<string, int> GetTubeSequence(DbElement item)
        {
            Dictionary<string, int> tubes = new Dictionary<string, int>();

            //Aveva.Core.Database.Filters.TypeFilter typeFiltTube = new Aveva.Core.Database.Filters.TypeFilter();

            //typeFiltTube.Add(DbElementTypeInstance.TUBE);

            DBElementCollection collection = new DBElementCollection(item)
            {
                IncludeRoot = false
            };

            int tubeCount = 1;

            foreach (DbElement dbelem in collection)
            {
                
                if (dbelem.GetAsString(DbAttributeInstance.TYPE) == "TUBI")
                {
                    if(!tubes.ContainsKey(dbelem.GetAsString(DbAttributeInstance.REF)))
                    {
                        tubes.Add(dbelem.GetAsString(DbAttributeInstance.REF), tubeCount);

                        tubeCount++;
                    }
                    

                }


            }

            return tubes;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbElement"></param>
        /// <param name="attributes"></param>
        /// <param name="ceDepth"></param>
        /// <param name="ceSiteDepth"></param>
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        private void TextFileWriter(DbElement dbElement, List<string> attributes, int ceDepth, int ceSiteDepth)
        {   
            try
            {

            if (dbElement == null || !dbElement.IsValid || dbElement.IsNull || dbElement.IsDeleted)
            {
               return;
            }
            var purpose = GetPurposeSafe(dbElement);


                if (!reachedLastElement)
                {

                    if ((ceDepth == pdDepth) && (pdDepth != -99))
                    {
                        LinesToWrite.Add(String.Format("{0}END", CalculateIndentation(ceDepth)));
                    }
                    else if (ceDepth < pdDepth)
                    {
                        for (int i = pdDepth; i >= ceDepth; i--)
                        {
                            LinesToWrite.Add(String.Format("{0}END", CalculateIndentation(i)));
                        }
                    }

                    pdDepth = ceDepth;

                    DbAttribute refNoAttribute = DbAttribute.GetDbAttribute("REFNO");
                    string currentEleRef = dbElement.GetAsString(refNoAttribute);

                    if (dbElement.GetAsString(DbAttributeInstance.TYPE) == "TUBI")
                    {
                        if(BranchTubeSequenceDictionary.ContainsKey(dbElement.GetAsString(DbAttributeInstance.REF)))
                        {
                            var tubSeq = BranchTubeSequenceDictionary[dbElement.GetAsString(DbAttributeInstance.REF)];
                            var tubeName = "TUBE " + tubSeq.ToString() + " of BRANCH " + dbElement.Owner.GetAsString(DbAttributeInstance.NAME);

                            //TUBE 9 of BRANCH 

                            LinesToWrite.Add(String.Format("{0}NEW {1}", CalculateIndentation(ceDepth), tubeName));
                        }
                        else
                        {
                            LinesToWrite.Add(String.Format("{0}NEW {1}", CalculateIndentation(ceDepth), dbElement.FullName()));
                        }

                        
                       
                    }
                    else
                    {
                        LinesToWrite.Add(String.Format("{0}NEW {1}", CalculateIndentation(ceDepth), dbElement.FullName()));
                    }
                    

                    LinesToWrite.Add(String.Format("\t{0}RefNo:= '{1}'", CalculateIndentation(ceDepth), currentEleRef));


                    foreach (string attribute in attributes)
                    {
                        if (!String.IsNullOrWhiteSpace(attribute) && !String.IsNullOrEmpty(attribute))
                        { 
                            var attriSplit = attribute.Split(';');
                                
                            if (attriSplit.Length == 2 )
                            {
                                if (!String.IsNullOrWhiteSpace(attriSplit[1]) && !String.IsNullOrEmpty(attriSplit[1]))
                                { 
                                    if (PmlOneExPressionElementProp.ContainsKey(attriSplit[1]))
                                    {
                                        string currentAttributePmlOne = "";

                                        try
                                        {
                                            currentAttributePmlOne = dbElement.EvaluateAsString(this.PmlOneExPressionElementProp[attriSplit[1].Trim()]);
                                        }
                                        catch (Exception)
                                        {
                                            

                                            if (purpose.Trim() == "HS" &&
                                                (attriSplit[1].Trim().ToLower() == ":prefabtask" || attriSplit[1].Trim().ToLower() == "sfref") &&
                                                dbElement.ElementType == DbElementTypeInstance.BRANCH &&
                                                dbElement.GetBool(DbAttributeInstance.ISNAME))
                                            {
                                                
                                                var isoInfoBran = GetPrefabTaskAndSfrefBranch(dbElement);
                                                if (attriSplit[1].Trim().ToLower() == "sfref")
                                                {
                                                    currentAttributePmlOne = isoInfoBran[0].ToString();
                                                }
                                                else if (attriSplit[1].Trim().ToLower() == ":prefabtask")
                                                {
                                                    currentAttributePmlOne = isoInfoBran[1].ToString();
                                                }



                                            }
                                            else if (purpose.Trim() == "HS" &&
                                                    (attriSplit[1].Trim().ToLower() == ":prefabtask" || attriSplit[1].Trim().ToLower() == "sfref") &&
                                                    dbElement.ElementType == DbElementTypeInstance.PCLAMP)
                                            {
                                                

                                                var catref = dbElement.GetElement(DbAttributeInstance.CATR);

                                                if (catref.IsValid && !catref.IsNull)
                                                {
                                                    if (catref.Owner.GetAsString(DbAttributeInstance.PURP).ToLower() == "wa")
                                                    {
                                                        var isoInfoPcla = GetPrefabTaskAndSfrefHanger(dbElement.Owner);

                                                        if (attriSplit[1].Trim().ToLower() == "sfref")
                                                        {
                                                            currentAttributePmlOne = isoInfoPcla[0].ToString();
                                                        }
                                                        else if (attriSplit[1].Trim().ToLower() == ":prefabtask")
                                                        {
                                                            currentAttributePmlOne = isoInfoPcla[1].ToString();
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                InvalidAttribures.Add(attribute);
                                                continue;
                                            }


                                        }

                                        if (attriSplit[1].Trim() == "NAME")
                                        {
                                            if (!currentAttributePmlOne.Contains("="))
                                            {
                                                currentAttributePmlOne = currentAttributePmlOne.Replace("/", "");
                                            }

                                        }
                                        //MatchCollection matches = Regex.Matches(currentAttributePmlOne, "^(.+)$", RegexOptions.Multiline);

                                        var singleLine = currentAttributePmlOne.Replace("\n", " ");

                                        LinesToWrite.Add(String.Format("\t{0}{1}:= '{2}'", CalculateIndentation(ceDepth), attriSplit[1], singleLine));
                                    }
                                    else
                                    {
                                        DbAttribute attriString = DbAttribute.GetDbAttribute(attriSplit[1].Trim());

                                        if (attriString == null)
                                        {
                                            InvalidAttribures.Add(attribute);
                                            continue;
                                        }


                                        if (!attriString.IsPseudo)
                                        {
                                            if (dbElement.IsAttributeValid(attriString))
                                            {
                                                string currentAttribute = dbElement.GetAsString(attriString);

                                                if (attriSplit[1].Trim().ToLower() == ":workordno" && 
                                                    purpose.Trim() == "HS" && 
                                                    dbElement.ElementType == DbElementTypeInstance.FRMWORK  && 
                                                    (currentAttribute.Trim().ToLower() == "unset" || currentAttribute.Trim() == ""))
                                                {
                                                    try
                                                    {
                                                        currentAttribute = dbElement.EvaluateAsString(this.PmlOneExPressionElementProp[attriSplit[1].Trim().ToLower()]);
                                                    }
                                                    catch (Exception)
                                                    {

                                                        currentAttribute = "unset";
                                                    }
                                                    
                                                }
                                                

                                                if (attriSplit[1].Trim() == "NAME")
                                                {
                                                    if (!currentAttribute.Contains("="))
                                                    {
                                                        currentAttribute = currentAttribute.Replace("/", "");
                                                    }

                                                }
                                                //MatchCollection matches = Regex.Matches(currentAttribute, "^(.+)$", RegexOptions.Multiline);

                                                var singleLine = currentAttribute.Replace("\n", " ");

                                                LinesToWrite.Add(String.Format("\t{0}{1}:= '{2}'", CalculateIndentation(ceDepth), attriSplit[1], singleLine));
                                            }
                                            else
                                            {

                                                InvalidAttribures.Add(attribute);

                                            }
                                        }
                                        else
                                        {
                                            string currentAttribute = "";

                                            if (!this.PmlOneExPressionElementProp.ContainsKey(attriSplit[1].Trim()))
                                            {
                                                this.PmlOneExPressionElementProp.Add(attriSplit[1].Trim(), DbExpression.Parse(attriSplit[1].Trim()));
                                            }

                                            if (!TryEvaluate(dbElement, this.PmlOneExPressionElementProp[attriSplit[1].Trim()], out currentAttribute))
                                            {
                                                InvalidAttribures.Add(attribute);
                                            }

                                            if (attriSplit[1].Trim() == "NAME")
                                            {
                                                if (!currentAttribute.Contains("="))
                                                {
                                                    currentAttribute = currentAttribute.Replace("/", "");
                                                }

                                            }
                                            //MatchCollection matches = Regex.Matches(currentAttribute, "^(.+)$", RegexOptions.Multiline);

                                            var singleLine = currentAttribute.Replace("\n", " ");

                                            LinesToWrite.Add(String.Format("\t{0}{1}:= '{2}'", CalculateIndentation(ceDepth), attriSplit[1], singleLine));

                                        }
                                    }

                                }
                            }

                        }
                    }


                }
                else if (reachedLastElement)
                {
                    if (ceDepth > ceSiteDepth)
                    {
                        for (int j = ceDepth; j >= ceSiteDepth; j--)
                        {

                            LinesToWrite.Add(String.Format("{0}END", CalculateIndentation(j)));
                        }
                    }

                }
            }
            catch (AccessViolationException ex2)
            {
                Console.WriteLine("AccessViolationException in TextFileWriter. Skipping this element.");
                Console.WriteLine(ex2.Message);
                throw;
            }
            catch (Exception ex2)
            {
                Console.WriteLine(ex2);
            }




        }//method end

        private object[] GetPrefabTaskAndSfrefBranch(DbElement dbElement)
        {
            //var refTrunArrti = DbAttribute.GetDbAttribute(":RefTrunnion");
            object[] isoInfo = new object[2];
            isoInfo[0] = "";

            isoInfo[1] = "";

            var branchName = dbElement.GetAsString(DbAttributeInstance.FLNN);

            DbElement finalHanger = null;

            var nameSplit = branchName.Split('/');

            if(nameSplit.Length > 1)
            {
                var restElement = DbElement.GetElement("/" + nameSplit[0]);

                if (restElement.IsValid && !restElement.IsNull)
                {
                    var hangers = restElement.Members().ToList().FindAll(h => h.GetAsString(DbAttribute.GetDbAttribute(":WA")).ToUpper() == "Y");
                    if (hangers.Any())
                    {
                        foreach (var hanger in hangers)
                        {
                            var hangerIndex = hanger.GetElementArray(DbAttribute.GetDbAttribute(":RefTrunnion")).ToList().FindIndex(t => t == dbElement);

                            if(hangerIndex != -1)
                            {
                                finalHanger = hanger;

                                break;
                            }
                        }
                    }
                }
            }

            if(finalHanger != null)
            {
                return GetPrefabTaskAndSfrefHanger(finalHanger);
            }

            return isoInfo;
        }

        private object[] GetPrefabTaskAndSfrefHanger(DbElement finalHanger)
        {
            object[] isoInfo = new object[2];
            isoInfo[0] = "";
            isoInfo[1] = "";
            var href = finalHanger.GetElement(DbAttributeInstance.HREF);
            if (href.IsValid && !href.IsNull)
            {
                if (!this.WeldedattachmentPrefabTaskSfref.ContainsKey(href.GetAsString(DbAttributeInstance.FLNN)))
                {

                    var taskNo = href.EvaluateAsString(this.PmlOneExPressionElementProp["SFREF"]);

                    var workOrNo = href.EvaluateAsString(this.PmlOneExPressionElementProp[":prefabtask"]);

                    isoInfo[0] = taskNo;

                    isoInfo[1] = workOrNo;

                    this.WeldedattachmentPrefabTaskSfref[href.GetAsString(DbAttributeInstance.FLNN)] = isoInfo;

                }

                return this.WeldedattachmentPrefabTaskSfref[href.GetAsString(DbAttributeInstance.FLNN)];
            }

            return isoInfo;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="header"></param>
        private void WriteHeadeeFooter(bool header)
        {

            if (header)
            {
                //Get world refno
                DbElement el = MDB.CurrentMDB.GetFirstWorld(DbType.Design);
                DbAttribute refNoAttribute = DbAttribute.GetDbAttribute("REFNO");
                string worldRenNo = el.GetAsString(refNoAttribute);              

                //Get name
                string mdbName = MDB.CurrentMDB.Name;
                LinesToWrite.Add("CADC_Attributes_File v1.0 , start: NEW , end: END , name_end: := , sep: &end&");
                LinesToWrite.Add("NEW Header Information");
                LinesToWrite.Add("\tSource:= PDMS Data &end& Date:=" + DateTime.Now.ToString("dddd yyyy MMMM") + " &end& Time:= " + DateTime.Now.ToString("HH:mm:ss"));
                LinesToWrite.Add("\tProject:= " + mdbName.ToLower() + "ModelExport &end& MDB:= /" + mdbName + " &end& Element:= " + worldRenNo);
                LinesToWrite.Add("END");
                LinesToWrite.Add("NEW /*");

            }
            else if (!header)
            {
                LinesToWrite.Add("END");
            }


        }//method end

        /// <summary>
        /// getting attribute value
        /// </summary>
        /// <param name="dbElement"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>

        private string GetAttribute(DbElement dbElement, string attribute)
        {
            string attributeValue = "not found";
            DbAttribute attributeString = DbAttribute.GetDbAttribute(attribute);
            if (dbElement.IsAttributeValid(attributeString))
            {
                attributeValue = dbElement.GetAsString(attributeString);
            }

            return attributeValue;
        }

        /// <summary>
        /// get intendation
        /// </summary>
        /// <param name="depth"></param>
        /// <returns></returns>

        private string CalculateIndentation(int depth)
        {
            try
            {
                string indentation = "";

                if (depth == 0)
                {
                    indentation = "";
                }
                else if (depth == 1)
                {
                    indentation = "\t";
                }
                else if (depth == 2)
                {
                    indentation = "\t\t";
                }
                else if (depth == 3)
                {
                    indentation = "\t\t\t";
                }
                else if (depth == 4)
                {
                    indentation = "\t\t\t\t";
                }
                else if (depth == 5)
                {
                    indentation = "\t\t\t\t\t";
                }
                else if (depth == 6)
                {
                    indentation = "\t\t\t\t\t\t";
                }
                else if (depth == 7)
                {
                    indentation = "\t\t\t\t\t\t\t";
                }

                return indentation;

            }
            catch (Exception ex)
            {
                
                Console.WriteLine(ex);
            }

            return "";

        }//method end

        /// <summary>
        /// skif the depth if the element is deep inside
        /// </summary>
        /// <param name="dbElement"></param>
        /// <returns></returns>

        private int SkipDepth(DbElement dbElement, List<string> ownerElementsType)
        {
            try
            {
                DbAttribute elementType = DbAttribute.GetDbAttribute("Type");
                DbAttribute Ddepth = DbAttribute.GetDbAttribute("DDEPTH");

                string dbElementType = dbElement.GetAsString(elementType);
                int dbElementDepth = Convert.ToInt32(dbElement.GetAsString(Ddepth));
                int newDepth = dbElementDepth;

                string typeOwnerElement = "";

                DbElement ownerElement = dbElement.Owner;
                int dbElementOwnerDepth = Convert.ToInt32(ownerElement.GetAsString(Ddepth));

                var ownerType = new List<string>() { "EQUI", "FRMW", "SBFR", "GENSEC", "PANE", "STWALL" };
                while (true)
                {
                    typeOwnerElement = ownerElement.GetAsString(elementType);
                    dbElementOwnerDepth = Convert.ToInt32(ownerElement.GetAsString(Ddepth));
                    var matchOwner = ownerElementsType.FirstOrDefault(stringToCheck => stringToCheck.Contains(typeOwnerElement));
                    if (dbElementDepth - dbElementOwnerDepth > 1)
                    {
                        newDepth = dbElementOwnerDepth + 1;
                    }

                    if (matchOwner != null)
                    {
                        break;
                    }

                    ownerElement = ownerElement.Owner;

                }

                if (((typeOwnerElement == "PANE") || (typeOwnerElement == "GENSEC")) && (dbElementDepth > 6))
                {
                    var ownerOwnerType = new List<string>() { "EQUI", "FRMW", "SBFR", "STWALL", "STRU" };
                    DbElement ownerOwnerElement = ownerElement.Owner;
                    while (true)
                    {

                        string typeOwnerOwnerElement = ownerOwnerElement.GetAsString(elementType);
                        int dbElementOwnerOwnerDepth = Convert.ToInt32(ownerOwnerElement.GetAsString(Ddepth));
                        var matchOwnerOwner = ownerOwnerType.FirstOrDefault(stringToCheck => stringToCheck.Contains(typeOwnerOwnerElement));

                        newDepth = dbElementOwnerOwnerDepth + 2;

                        if (matchOwnerOwner != null)
                        {
                            break;
                        }

                        ownerOwnerElement = ownerOwnerElement.Owner;

                    }

                }

                return newDepth;
            }
            catch (Exception exp)
            {

                throw new Exception(exp.ToString());
            }



        }//method end

        public string CabelLaderSegmentUpdater(DbElement controlObject)
        {
            string attaName = "";

            try
            {
                //Console.WriteLine(controlObject.Name());

                DBElementCollection dbElementWithNoid;

                NoidFilter noidFilter = new NoidFilter();

                dbElementWithNoid = new DBElementCollection(controlObject.Owner, noidFilter);

                DBElementCollection dbElementAtta;

                AttaFilter attaFilter = new AttaFilter();

                dbElementAtta = new DBElementCollection(controlObject.Owner, attaFilter);

                int attaCount = 0;

                foreach (DbElement itemToCount in dbElementAtta)
                {
                    attaCount++;
                }


                if (attaCount == 0)
                {
                    return attaName;
                }


                var sequNoEle = Convert.ToInt32(controlObject.EvaluateAsString(this.PmlOneExPressionElementProp["Sequence"]));


                int finalSeq = 1;

                DbElement stopSeqEle = null;

                DbElement previousElement = null;

                foreach (DbElement itemNoid in dbElementWithNoid)
                {
                    int sequNoNoid = Convert.ToInt32(itemNoid.EvaluateAsString(this.PmlOneExPressionElementProp["Sequence"]));

                    finalSeq = sequNoNoid;

                    stopSeqEle = itemNoid;

                    if ((sequNoNoid >= finalSeq) && (sequNoNoid > sequNoEle))
                    {
                        break;
                    }

                    previousElement = itemNoid;
                }

                int stopSeqNum = Convert.ToInt32(stopSeqEle.EvaluateAsString(this.PmlOneExPressionElementProp["Sequence"]));

                int prevSeqNum = Convert.ToInt32(previousElement.EvaluateAsString(this.PmlOneExPressionElementProp["Sequence"]));

                foreach (DbElement itemAtta in dbElementAtta)
                {
                    int seqNoAtta = Convert.ToInt32(itemAtta.EvaluateAsString(this.PmlOneExPressionElementProp["Sequence"]));

                    if ((seqNoAtta > prevSeqNum) && (seqNoAtta < stopSeqNum))
                    {
                        return itemAtta.FullName().After("/");
                    }

                }

                dbElementWithNoid = null;

                dbElementAtta = null;

                return attaName;
            }
            catch (Exception)
            {

                return attaName;
            }

            //return attaName;

            //while (dbElementAtta.MoveNext())
            //{ 
            //    DbElement currentItem = (DbElement)dbElementAtta.Current;

            //    int seqNoAtta = Convert.ToInt32(currentItem.EvaluateAsString(DbExpression.Parse("Sequence")));

            //    if ((seqNoAtta > prevSeqNum) && (seqNoAtta < stopSeqNum))
            //    {
            //        return currentItem.FullName().After("/");
            //    }

            //}

        }//method end

    }//class end

}//namespace end


