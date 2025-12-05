using System;
using System.Collections.Generic;
using System.IO;


namespace AiPdms.Navis.Utilities
{
    public class PMLDataReader
    {
        public PMLDataReader()
        {

        }

        /// <summary>
        /// collect data from attribute file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="attribLibrary"></param>
        /// <returns></returns>

        public List<List<string>> CollectAttribute(string path, AttribLibrary attribLibrary)
        {
            using (StreamReader sr = File.OpenText(path))
            {
                String s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    if (!(string.IsNullOrWhiteSpace(s)))
                    {
                        if (!s.StartsWith("--") && s.Between("'", ";").Trim().Equals("SITE"))
                        {
                            attribLibrary.TypeOfElementsToExport.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$').Before(";"));

                            attribLibrary.SiteAttrib.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$'));
                            
                        }
                        else if (!s.StartsWith("--") && s.Between("'", ";").Trim().Equals("ZONE"))
                        {
                            attribLibrary.TypeOfElementsToExport.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$').Before(";"));
                            attribLibrary.ZoneAttrib.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$'));
                            
                        }
                        else if (!s.StartsWith("--") && s.Between("'", ";").Trim().Equals("PIPE"))
                        {
                            attribLibrary.TypeOfElementsToExport.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$').Before(";"));
                            attribLibrary.PipeAttrib.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$'));
                            
                        }
                        else if (!s.StartsWith("--") && s.Between("'", ";").Trim().Equals("BRAN"))
                        {
                            attribLibrary.TypeOfElementsToExport.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$').Before(";"));
                            attribLibrary.BranAttrib.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$'));
                            
                        }
                        else if (!s.StartsWith("--") && s.Between("'", ";").Trim().Equals("BRANMEMB"))
                        {
                            attribLibrary.TypeOfElementsToExport.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$').Before(";"));
                            attribLibrary.BranMembAttrib.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$'));
                            
                        }
                        else if (!s.StartsWith("--") && s.Between("'", ";").Trim().Equals("HVAC"))
                        {
                            attribLibrary.TypeOfElementsToExport.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$').Before(";"));
                            attribLibrary.HvacAttrib.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$'));
                            
                        }
                        else if (!s.StartsWith("--") && s.Between("'", ";").Trim().Equals("EQUI"))
                        {
                            attribLibrary.TypeOfElementsToExport.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$').Before(";"));
                            attribLibrary.EquiAttrib.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$'));
                            
                        }
                        else if (!s.StartsWith("--") && s.Between("'", ";").Trim().Equals("SUBE"))
                        {
                            attribLibrary.TypeOfElementsToExport.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$').Before(";"));
                            attribLibrary.SubeAttrib.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$'));
                            
                        }
                        else if (!s.StartsWith("--") && s.Between("'", ";").Trim().Equals("EQUIMEMB"))
                        {
                            attribLibrary.TypeOfElementsToExport.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$').Before(";"));
                            attribLibrary.EquiMembAttrib.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$'));
                            
                        }
                        else if (!s.StartsWith("--") && s.Between("'", ";").Trim().Equals("SUBEMEMB"))
                        {
                            attribLibrary.TypeOfElementsToExport.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$').Before(";"));
                            attribLibrary.SubeMembAttrib.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$'));
                            
                        }
                        else if (!s.StartsWith("--") && s.Between("'", ";").Trim().Equals("SUBSMEMB"))
                        {
                            attribLibrary.TypeOfElementsToExport.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$').Before(";"));
                            attribLibrary.SubsMembAttrib.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$'));
                            
                        }
                        else if (!s.StartsWith("--") && s.Between("'", ";").Trim().Equals("STRUMEMB"))
                        {
                            attribLibrary.TypeOfElementsToExport.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$').Before(";"));
                            attribLibrary.StruMembAttrib.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$'));
                            
                        }
                        else if (!s.StartsWith("--") && s.Between("'", ";").Trim().Equals("STRU"))
                        {
                            attribLibrary.TypeOfElementsToExport.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$').Before(";"));
                            attribLibrary.StruAttrib.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$'));
                            
                        }
                        else if (!s.StartsWith("--") && s.Between("'", ";").Trim().Equals("FRMW"))
                        {
                            attribLibrary.TypeOfElementsToExport.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$').Before(";"));
                            attribLibrary.FrmwAttrib.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$'));
                            
                        }
                        else if (!s.StartsWith("--") && s.Between("'", ";").Trim().Equals("SBFR"))
                        {
                            attribLibrary.TypeOfElementsToExport.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$').Before(";"));
                            attribLibrary.SbfrAttrib.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$'));
                            
                        }
                        else if (!s.StartsWith("--") && s.Between("'", ";").Trim().Equals("GENSEC"))
                        {
                            attribLibrary.TypeOfElementsToExport.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$').Before(";"));
                            attribLibrary.GensecAttrib.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$'));
                            
                        }
                        else if (!s.StartsWith("--") && s.Between("'", ";").Trim().Equals("PANE"))
                        {
                            attribLibrary.TypeOfElementsToExport.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$').Before(";"));
                            attribLibrary.PaneAttrib.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$'));
                            
                        }
                        else if (!s.StartsWith("--") && s.Between("'", ";").Trim().Equals("PNOD"))
                        {
                            attribLibrary.TypeOfElementsToExport.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$').Before(";"));
                            attribLibrary.PnodAttrib.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$'));
                            
                        }
                        else if (!s.StartsWith("--") && s.Between("'", ";").Trim().Equals("REST"))
                        {
                            attribLibrary.TypeOfElementsToExport.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$').Before(";"));
                            attribLibrary.RestAttrib.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$'));
                            
                        }
                        else if (!s.StartsWith("--") && s.Between("'", ";").Trim().Equals("SUPPO"))
                        {
                            attribLibrary.TypeOfElementsToExport.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$').Before(";"));
                            attribLibrary.SuppoAttrib.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$'));

                        }
                        else if (!s.StartsWith("--") && s.Between("'", ";").Trim().Equals("HANG"))
                        {
                            attribLibrary.TypeOfElementsToExport.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$').Before(";"));
                            attribLibrary.HangAttrib.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$'));
                            
                        }
                        else if (!s.StartsWith("--") && s.Between("'", ";").Trim().Equals("TRUNNI"))
                        {
                            attribLibrary.TypeOfElementsToExport.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$').Before(";"));
                            attribLibrary.TrunniAttrib.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$'));

                        }
                        else if (!s.StartsWith("--") && s.Between("'", ";").Trim().Equals("SUPC"))
                        {
                            attribLibrary.TypeOfElementsToExport.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$').Before(";"));
                            attribLibrary.SupcAttrib.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$'));

                        }
                        else if (!s.StartsWith("--") && s.Between("'", ";").Trim().Equals("STWALL"))
                        {
                            attribLibrary.TypeOfElementsToExport.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$').Before(";"));
                            attribLibrary.StwallAttrib.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$'));
                            
                        }
                        else if (!s.StartsWith("--") && s.Between("'", ";").Trim().Equals("SUBS"))
                        {
                            attribLibrary.TypeOfElementsToExport.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$').Before(";"));
                            attribLibrary.SubsAttrib.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$'));
                            
                        }
                        else if (!s.StartsWith("--") && s.Between("'", ";").Trim().Equals("WALL"))
                        {
                            attribLibrary.TypeOfElementsToExport.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$').Before(";"));
                            attribLibrary.WallAttrib.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$'));
                           
                        }
                        else if (!s.StartsWith("--") && s.Between("'", ";").Trim().Equals("FLOOR"))
                        {
                            attribLibrary.TypeOfElementsToExport.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$').Before(";"));
                            attribLibrary.FloorAttrib.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$'));
                            
                        }
                        else if (!s.StartsWith("--") && s.Between("'", ";").Trim().Equals("PCLA"))
                        {
                            attribLibrary.TypeOfElementsToExport.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$').Before(";"));
                            attribLibrary.PclaAttrib.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$'));
                            //Console.WriteLine(s.Between("'", "$").Trim());
                        }
                        else if (!s.StartsWith("--") && s.Between("'", ";").Trim().Equals("TRANCI"))
                        {
                            attribLibrary.TypeOfElementsToExport.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$').Before(";"));
                            attribLibrary.TranciAttrib.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$'));
                            //Console.WriteLine(s.Between("'", "$").Trim());
                        }
                        else if (!s.StartsWith("--") && s.Between("'", ";").Trim().Equals("TRREDU"))
                        {
                            attribLibrary.TypeOfElementsToExport.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$').Before(";"));
                            attribLibrary.TreduAttrib.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$'));
                            //Console.WriteLine(s.Between("'", "$").Trim());
                        }
                        else if (!s.StartsWith("--") && s.Between("'", ";").Trim().Equals("PLAT"))
                        {
                            attribLibrary.TypeOfElementsToExport.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$').Before(";"));
                            attribLibrary.PlatAttrib.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$'));
                            //Console.WriteLine(s.Between("'", "$").Trim());
                        }
                        else if (!s.StartsWith("--") && s.Between("'", ";").Trim().Equals("ANCI"))
                        {
                            attribLibrary.TypeOfElementsToExport.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$').Before(";"));
                            attribLibrary.AnciAttrib.Add(s.Between("'", "$").Trim().Trim('\'').TrimEnd('$'));
                            //Console.WriteLine(s.Between("'", "$").Trim());
                        }
                    }
                }//while

            }//file reader


            List<List<string>> listCollection = new List<List<string>>
            {
                attribLibrary.SiteAttrib,
                attribLibrary.ZoneAttrib,
                attribLibrary.PipeAttrib,
                attribLibrary.BranAttrib,
                attribLibrary.BranMembAttrib,

                attribLibrary.HvacAttrib,
                attribLibrary.EquiAttrib,
                attribLibrary.SubeAttrib,
                attribLibrary.EquiMembAttrib,
                attribLibrary.SubeMembAttrib,

                attribLibrary.SubsMembAttrib,
                attribLibrary.StruAttrib,
                attribLibrary.StruMembAttrib,
                attribLibrary.FrmwAttrib,
                attribLibrary.SbfrAttrib,

                attribLibrary.GensecAttrib,
                attribLibrary.PaneAttrib,
                attribLibrary.PnodAttrib,
                attribLibrary.RestAttrib,
                attribLibrary.SuppoAttrib,
                attribLibrary.HangAttrib,
                attribLibrary.SupcAttrib,
                attribLibrary.TrunniAttrib,

                attribLibrary.StwallAttrib,
                attribLibrary.SubsAttrib,
                attribLibrary.WallAttrib,
                attribLibrary.FloorAttrib,
                attribLibrary.PclaAttrib,
                attribLibrary.AnciAttrib,
                attribLibrary.TranciAttrib,
                attribLibrary.TreduAttrib,
                attribLibrary.PlatAttrib
            };

            return listCollection;
        }


    }
}
