using Aveva.Core.Database;
using Aveva.Core.Database.Filters;
using Aveva.Core.PMLNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.ExceptionServices;
using System.Security;

namespace AiPdms.Navis.Utilities
{
	[PMLNetCallable()]
	public class AiPdmsNavisUtilities
	{
        AttribLibrary attribLibrary;

        PMLDataReader pMLDataReader;

        E3DDBElementCollection e3DDBElementCollection;

        ExportAttibutes exportAttibutes;

        private const int FileBufferSize = 64 * 1024; // 64KB

        [PMLNetCallable()]
		public AiPdmsNavisUtilities()
		{

		}

		[PMLNetCallable()]
		public void Assign(AiPdmsNavisUtilities that)
		{


		}

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
		[PMLNetCallable()]
		public void ExportNavisAttributes(string pmlDataPath, string outputFile)
		{
            try
            {
                if (File.Exists(outputFile))
                {
                    File.Delete(outputFile);
                }

                attribLibrary = new AttribLibrary();

                pMLDataReader = new PMLDataReader();

                var listCollection = pMLDataReader.CollectAttribute(pmlDataPath, attribLibrary);

                e3DDBElementCollection = new E3DDBElementCollection();

                var siteCollection = e3DDBElementCollection.CollectSites();

                ExportAttribute(e3DDBElementCollection.AllSitesAibel, attribLibrary, outputFile);
            }
            catch (Exception exMain)
            {

                Console.WriteLine(exMain);
            }
            finally
            {
                attribLibrary = null;

                pMLDataReader = null;

                e3DDBElementCollection = null;

                GC.Collect();
                
            }
  
        }//method ExportNavisAttributes

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [PMLNetCallable()]
        public void ExportNavisAttributes(string pmlDataPath, string outputFile, Hashtable inputArray, bool exportAll)
        {
            try
            {
                if (File.Exists(outputFile))
                {
                    File.Delete(outputFile);
                }

                attribLibrary = new AttribLibrary();

                pMLDataReader = new PMLDataReader();

                var listCollection = pMLDataReader.CollectAttribute(pmlDataPath, attribLibrary);

                List<string> inputArrayList = Enumerable.Cast<string>(inputArray.Values).ToList();

                if (exportAll)
                {
                  
                    e3DDBElementCollection = new E3DDBElementCollection(inputArrayList);

                    var siteCollection = e3DDBElementCollection.CollectSitesWithSelectedPurpose();

                    ExportAttribute(e3DDBElementCollection.AllSitesAibel, attribLibrary, outputFile);
                }
                else
                {
                    List<DbElement> dbelements = new List<DbElement>();

                    foreach (var item in inputArrayList)
                    {
                        DbElement dbElement = DbElement.GetElement(item);

                        if(dbElement.IsValid && !dbElement.IsNull)
                        {
                            dbelements.Add(dbElement);
                        }
                        else
                        {
                            Console.WriteLine($"{item} is not valid");
                        }
                    }

                    ExportAttribute(dbelements, attribLibrary, outputFile);
                }


            }
            catch (Exception exMain)
            {

                Console.WriteLine(exMain);
            }
            finally
            {
                attribLibrary = null;

                pMLDataReader = null;

                e3DDBElementCollection = null;

                GC.Collect();

            }

        }//method ExportNavisAttributes

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pmlDataPath"></param>
        /// <param name="outputFile"></param>
        /// <param name="element"></param>

        [PMLNetCallable()]
        public void ExportNavisAttributesForSelected(string pmlDataPath, string outputFile, Hashtable elements)
        {
            
            try
            {
                if (File.Exists(outputFile))
                {
                    File.Delete(outputFile);
                }

                 attribLibrary = new AttribLibrary();

                pMLDataReader = new PMLDataReader();

                var listCollection = pMLDataReader.CollectAttribute(pmlDataPath, attribLibrary);

                using (var e3DDBElementCollection = new E3DDBElementCollection())
                {

                    foreach (DictionaryEntry item in elements)
                    {
                        DbElement elementToList = DbElement.GetElement(Convert.ToString(item.Value).Trim());
                        if (elementToList.IsValid && !elementToList.IsNull)
                        {
                            e3DDBElementCollection.AllSitesAibel.Add(elementToList);
                        }
                        else
                        {
                            Console.WriteLine(item.Value + " is not a valid element");
                        }
                    }

                    List<string> distinctType = attribLibrary.TypeOfElementsToExport.Distinct().ToList();

                    if (e3DDBElementCollection.AllSitesAibel.Count != 0)
                    {
                        ExportAttribute(e3DDBElementCollection.AllSitesAibel, attribLibrary, outputFile);
                    }
                    else
                    {
                        Console.WriteLine("Number of elements colected = 0");
                    }
                }
                
            }
            catch (Exception exMain)
            {
                Console.WriteLine(exMain);
            }
            finally
            {
                attribLibrary = null;

                pMLDataReader = null;

                e3DDBElementCollection = null;

                GC.Collect();

            }


        }//method ExportNavisAttributesForSelected

        [PMLNetCallable()]
        public void ExportNavisAttributesForSelectedDiscipline(string pmlDataPath, string outputFile, Hashtable elements, double appendNumber)
        {

            try
            {
                // Append the integer to the output file name
                string outputFileWithNumber = Path.Combine(Path.GetDirectoryName(outputFile), Path.GetFileNameWithoutExtension(outputFile) + ((int)appendNumber) + Path.GetExtension(outputFile));

                if (File.Exists(outputFileWithNumber))
                {
                    File.Delete(outputFileWithNumber);
                }

                attribLibrary = new AttribLibrary();

                pMLDataReader = new PMLDataReader();

                var listCollection = pMLDataReader.CollectAttribute(pmlDataPath, attribLibrary);

                using (var e3DDBElementCollection = new E3DDBElementCollection())
                {

                    foreach (DictionaryEntry item in elements)
                    {
                        DbElement elementToList = DbElement.GetElement(Convert.ToString(item.Value).Trim());
                        if (elementToList.IsValid && !elementToList.IsNull)
                        {
                            e3DDBElementCollection.AllSitesAibel.Add(elementToList);
                        }
                        else
                        {
                            Console.WriteLine(item.Value + " is not a valid element");
                        }
                    }

                    List<string> distinctType = attribLibrary.TypeOfElementsToExport.Distinct().ToList();

                    if (e3DDBElementCollection.AllSitesAibel.Count != 0)
                    {
                        // record start
                        var exportStart = DateTime.UtcNow;
                        try
                        {
                            ExportAttribute(e3DDBElementCollection.AllSitesAibel, attribLibrary, outputFileWithNumber);
                            var exportEnd = DateTime.UtcNow;
                            double durationMs = (exportEnd - exportStart).TotalMilliseconds;
                            double sizeMb = File.Exists(outputFileWithNumber) ? new FileInfo(outputFileWithNumber).Length / 1024.0 / 1024.0 : 0.0;

                            // call structured message: OP|DURATION_MS|STATUS|EXTRA
                            string structured = $"EXPORT|{durationMs.ToString(System.Globalization.CultureInfo.InvariantCulture)}|OK|";
                            int sites = e3DDBElementCollection.AllSitesAibel?.Count ?? 0;
                            AppendLogForBase(outputFileWithNumber, structured, sites);

                        }
                        catch (Exception ex)
                        {
                            var exportEnd = DateTime.UtcNow;
                            double durationMs = (exportEnd - exportStart).TotalMilliseconds;
                            string structured = $"EXPORT|{durationMs.ToString(System.Globalization.CultureInfo.InvariantCulture)}|ERROR|{ex.Message}";
                            int sites = e3DDBElementCollection.AllSitesAibel?.Count ?? 0;
                            AppendLogForBase(outputFileWithNumber, structured, sites);
                            throw;
                        }
                    }

                }

            }
            catch (Exception exMain)
            {
                Console.WriteLine(exMain);
            }
            finally
            {
                attribLibrary = null;

                pMLDataReader = null;

                e3DDBElementCollection = null;

                GC.Collect();

            }

        }//method export per discipline


        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentElement"></param>
        /// <returns></returns>

        [PMLNetCallable()]
        public string ExportAttributeTest(string currentElement)
        {
            DbElement suppHang = DbElement.GetElement(currentElement);

            //DbAttribute Ddepth = DbAttribute.GetDbAttribute("DDEPTH");
            //string ddeString = suppHang.GetAsString(Ddepth);

            string ddeString1 = suppHang.FullName();

            string ddeString2 = suppHang.Name();

            return ddeString1 + ddeString2;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="siteCollection"></param>
        /// <param name="attribLibrary"></param>
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]     
        public void ExportAttribute(List<DbElement> siteCollection, AttribLibrary attribLibrary, string exportPath)
        {
            try
            {
                exportAttibutes = new ExportAttibutes
                {
                    FileWritePath = exportPath
                };

                exportAttibutes.WriteAttributeToTextFile(siteCollection, attribLibrary);
            }
            catch (AccessViolationException ex)
            {
                Console.WriteLine("AccessViolationException in ExportAttribute. Export stopped safely.");
                Console.WriteLine(ex);
            }
            catch (Exception ex2)
            {
                Console.WriteLine(ex2);
            }
            finally
            {
                exportAttibutes = null;
                GC.Collect();
            }
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        [PMLNetCallable()]
        public void MergeAndManipulateFiles(string baseFileName, string destinationFile, double fileCountDouble)
        {
            var mergeStart = DateTime.UtcNow;

            int fileCount = (int)fileCountDouble;

            string dir = Path.GetDirectoryName(baseFileName) ?? string.Empty;
            string name = Path.GetFileNameWithoutExtension(baseFileName);
            string ext = Path.GetExtension(baseFileName);

            Func<int, string> PartPath = i => Path.Combine(dir, name + i + ext);
            string tmpDest = Path.Combine(dir, name + "__merge_tmp" + ext);

            try
            {
                if (fileCount <= 0)
                {
                    Console.WriteLine("MergeAndManipulateFiles: fileCount <= 0, ingenting å merge.");
                    return;
                }
                if (!Directory.Exists(dir))
                {
                    Console.WriteLine("MergeAndManipulateFiles: directory does not exist: " + dir);
                    return;
                }

                // collect existing parts (1..fileCount)
                var parts = new List<string>();
                for (int i = 1; i <= fileCount; i++)
                {
                    string p = PartPath(i);
                    if (File.Exists(p)) parts.Add(p);
                    else Console.WriteLine("MergeAndManipulateFiles: missing " + p);
                }

                if (parts.Count == 0)
                {
                    Console.WriteLine("MergeAndManipulateFiles: no part files found.");
                    return;
                }

                using (var outStream = new FileStream(
                           tmpDest,
                           FileMode.Create,
                           FileAccess.Write,
                           FileShare.None,
                           FileBufferSize,
                           FileOptions.SequentialScan))
                {
                    bool headerWritten = false; // write header ONLY once (from first file)
                    bool wroteAnything = false; // for safety
                    int linesSinceFlush = 0;

                    foreach (var part in parts)
                    {
                        using (var inStream = new FileStream(
                           part,
                           FileMode.Open,
                           FileAccess.Read,
                           FileShare.Read,
                           FileBufferSize,
                           FileOptions.SequentialScan))
                        using (var reader = new StreamReader(inStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
                        {
                            bool seenNewStar = false;   // found "NEW /*"
                            int newDepth = 0;           // nesting depth inside NEW/END once NEW /* is found

                            string carry = null;        // one-line lookahead (when we peek next line)
                            while (true)
                            {
                                string line = carry ?? reader.ReadLine();
                                carry = null;

                                if (line == null) break;

                                // 1) HEADER MODE: write everything until and including the FIRST "NEW /*"
                                if (!headerWritten)
                                {
                                    WriteLineRaw(outStream, line, ref linesSinceFlush);
                                    wroteAnything = true;

                                    if (line.Trim() == "NEW /*")
                                    {
                                        headerWritten = true;
                                        seenNewStar = true;
                                        newDepth = 1; // we've entered NEW /*
                                    }
                                    continue;
                                }

                                // 2) For files 2..N: skip everything until we hit "NEW /*"
                                if (!seenNewStar)
                                {
                                    if (line.Trim() == "NEW /*")
                                    {
                                        seenNewStar = true;
                                        newDepth = 1;
                                    }
                                    continue; // do NOT write "NEW /*" for file 2..N
                                }

                                // 3) Inside NEW /* ... END block: fix 2-line broken quoted assignments safely
                                //    (never join into END/NEW/another :=)
                                if (IsBrokenQuotedAssignment(line))
                                {
                                    string next = reader.ReadLine();

                                    if (next == null)
                                    {
                                        // EOF while quote is open -> force close
                                        line = ForceCloseQuotedAssignment(line);
                                    }
                                    else if (IsStructuralLine(next) || IsAssignmentLine(next))
                                    {
                                        // next is END/NEW/another assignment -> don't join; force close current, then re-process next
                                        line = ForceCloseQuotedAssignment(line);
                                        carry = next;
                                    }
                                    else if (next.Contains("'"))
                                    {
                                        // normal 2-line case -> join
                                        line = line + next.TrimStart();
                                    }
                                    else
                                    {
                                        // unexpected continuation without quote -> force close to avoid killing Navis parse
                                        line = ForceCloseQuotedAssignment(line);
                                        carry = next;
                                    }
                                }

                                // 4) Maintain nesting: cut EXACTLY at the matching END for NEW /*
                                string t = line.Trim();

                                if (t.StartsWith("NEW "))
                                {
                                    newDepth++;
                                    WriteLineRaw(outStream, line, ref linesSinceFlush);
                                    wroteAnything = true;
                                    continue;
                                }

                                if (t == "END")
                                {
                                    newDepth--;

                                    if (newDepth == 0)
                                    {
                                        // this END closes the file's NEW /* wrapper -> skip it (we'll close once at the end)
                                        break; // stop reading this part
                                    }

                                    WriteLineRaw(outStream, line, ref linesSinceFlush);
                                    wroteAnything = true;
                                    continue;
                                }

                                // normal line
                                WriteLineRaw(outStream, line, ref linesSinceFlush);
                                wroteAnything = true;
                            }

                            // If we never found NEW /* in this file, log it
                            if (!seenNewStar)
                                Console.WriteLine("WARN: Did not find 'NEW /*' in " + part);
                        }
                    }

                    // Close the one global NEW /* wrapper once
                    if (wroteAnything)
                    {
                        WriteLineRaw(outStream, "END", ref linesSinceFlush);
                    }

                    // ensure final data flushed
                    FlushIfNeeded(outStream, ref linesSinceFlush, 0);
                }

                File.Copy(tmpDest, destinationFile, true);
                File.Delete(tmpDest);

                var mergeEnd = DateTime.UtcNow;
                double mergeMs = (mergeEnd - mergeStart).TotalMilliseconds;
                string structured = $"MERGE|{mergeMs.ToString(System.Globalization.CultureInfo.InvariantCulture)}|OK|";
                int sitesCountIfKnown = -1; // hvis du har et sites-tall fra eksport, send det; ellers -1
                AppendLogForBase(destinationFile, structured, sitesCountIfKnown);



                Console.WriteLine("All files appended into " + destinationFile);

                // optional: delete parts
                for (int i = 1; i <= fileCount; i++)
                {
                    string p = PartPath(i);
                    if (File.Exists(p))
                    {
                        File.Delete(p);
                        Console.WriteLine("Deleted file: " + p);
                    }
                }
            }
            catch (Exception ex)
            {
                var errorTime = DateTime.UtcNow;
                double errorMs = (errorTime - mergeStart).TotalMilliseconds;
                string structured = $"MERGE|{errorMs.ToString(System.Globalization.CultureInfo.InvariantCulture)}|ERROR|{ex.Message}";
                AppendLogForBase(destinationFile, structured, -1);
                Console.WriteLine("An error occurred in MergeAndManipulateFiles: " + ex.Message);
                try { if (File.Exists(tmpDest)) File.Delete(tmpDest); } catch { }
            }
        }

        private static void FlushIfNeeded(Stream outStream, ref int linesSinceFlush, int threshold = 5000)
        {
            if (threshold <= 0)
            {
                // force flush (used for final flush)
                outStream.Flush();
                linesSinceFlush = 0;
                return;
            }

            if (linesSinceFlush >= threshold)
            {
                outStream.Flush();
                linesSinceFlush = 0;
            }
        }

        private static void WriteLineRaw(Stream outStream, string line, ref int linesSinceFlush)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(line);
            outStream.Write(bytes, 0, bytes.Length);
            byte[] crlf = new byte[] { 0x0D, 0x0A };
            outStream.Write(crlf, 0, crlf.Length);

            // increment and maybe flush
            linesSinceFlush++;
            FlushIfNeeded(outStream, ref linesSinceFlush);
        }

        private static bool IsStructuralLine(string line)
        {
            var t = (line ?? "").Trim();
            return t == "END" || t.StartsWith("NEW ");
        }

        private static bool IsAssignmentLine(string line)
        {
            return ((line ?? "").Trim()).Contains(":=");
        }

        private static bool IsBrokenQuotedAssignment(string line)
        {
            var t = (line ?? "").TrimEnd();
            int idx = t.IndexOf(":=");
            if (idx < 0) return false;

            string after = t.Substring(idx + 2);
            if (after.IndexOf('\'') < 0) return false;

            int q = 0;
            foreach (char c in after)
                if (c == '\'') q++;

            return (q % 2) != 0;
        }

        private static string ForceCloseQuotedAssignment(string line)
        {
            // close with a trailing quote to avoid EOF errors
            return (line ?? "").TrimEnd() + "'";
        }

        // put this inside AiPdmsNavisUtilities class (replaces existing AppendLogForBase)
        private static readonly object _logLock = new object();

        [PMLNetCallable()]
        public void AppendLogForBase(string baseFilePath, string message, int siteCount = -1)
        {
            try
            {
                if (string.IsNullOrEmpty(baseFilePath)) return;

                string baseDir = Path.GetDirectoryName(baseFilePath) ?? string.Empty;
                DirectoryInfo di = Directory.GetParent(baseDir);
                string parentDir = di?.FullName ?? baseDir;
                string logDir = Path.Combine(parentDir, "log");

                if (!Directory.Exists(logDir))
                    Directory.CreateDirectory(logDir);

                // Use a single consolidated logfile name
                string logFile = Path.Combine(logDir, "ModelExportAttributes_log.txt");

                // If message is structured (OP|DURATION_MS|STATUS|EXTRA) keep backward compat
                string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", System.Globalization.CultureInfo.InvariantCulture);

                string line;
                if (!string.IsNullOrEmpty(message) && message.Contains("|"))
                {
                    // old structured message parsing: parts = OP|DURATION_MS|STATUS|EXTRA (optional)
                    var parts = message.Split(new[] { '|' }, 4);
                    string operation = parts.Length > 0 ? parts[0].Trim() : "";
                    double durationMs = 0;
                    double minutesVal = 0.0;
                    if (parts.Length > 1 && double.TryParse(parts[1].Trim(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out durationMs))
                    {
                        minutesVal = durationMs / 60000.0;
                    }
                    string minutesStr = minutesVal.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) + " min";

                    string status = parts.Length > 2 ? parts[2].Trim() : "";
                    string extra = parts.Length > 3 ? parts[3].Trim() : "";

                    // file size in MB
                    double mb = File.Exists(baseFilePath) ? new FileInfo(baseFilePath).Length / 1024.0 / 1024.0 : 0.0;
                    string mbStr = mb.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) + " MB";

                    // build tab-separated fields
                    // Timestamp \t Operation \t Filename \t Status \t Duration \t Size \t SiteCount(optional) \t Extra
                    var fields = new List<string>
            {
                timestamp,
                operation,
                Path.GetFileName(baseFilePath),
                status,
                minutesStr,
                mbStr
            };
                    if (siteCount >= 0) fields.Add(siteCount.ToString());
                    if (!string.IsNullOrEmpty(extra)) fields.Add(extra);

                    line = string.Join("\t", fields);
                }
                else
                {
                    // raw message fallback -> still use a tab at start for consistent columns
                    var fields = new List<string>
            {
                timestamp,
                "MSG",
                Path.GetFileName(baseFilePath),
                "-", // status unknown
                "-", // duration unknown
                "-", // size unknown
            };
                    if (siteCount >= 0) fields.Add(siteCount.ToString());
                    fields.Add(message);

                    line = string.Join("\t", fields);
                }

                lock (_logLock)
                    File.AppendAllText(logFile, line + Environment.NewLine, Encoding.UTF8);
            }
            catch
            {
                // never throw from logger
            }
        }








    }

}
