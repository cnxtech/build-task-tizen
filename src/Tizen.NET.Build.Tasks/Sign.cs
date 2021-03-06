﻿/*
 * Copyright 2017 (c) Samsung Electronics Co., Ltd  All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * 	http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.IO;
using System.IO.Compression;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Diagnostics;
using Tizen.NET.Build.Tasks.Signer;

namespace Tizen.NET.Build.Tasks
{
    public class Sign : Task
    {
        private const string AuthorSignatureFileName = "author-signature.xml";
        private const string DistributorSignatureFilePrefix = "signature";
        private const string DistributorSignatureFilePostfix = ".xml";

        [Required]
        public string RootDir { get; set; }

        [Required]
        public string AuthorFile { get; set; }

        [Required]
        public string AuthorPassword { get; set; }

        [Required]
        public string DistFile { get; set; }

        [Required]
        public string DistPassword { get; set; }

        public string DistFile2 { get; set; }

        public string DistPassword2 { get; set; }

        public string OutputSignedTpk { get; set; }

        public override bool Execute()
        {
            if (!Directory.Exists(RootDir) && !File.Exists(RootDir))
            {
                Log.LogError("Root File or Directory does not exist {0}", RootDir);
                return false;
            }

            if (!File.Exists(AuthorFile))
            {
                Log.LogError("Author File or Directory does not exist {0}", AuthorFile);
                return false;
            }

            if (!File.Exists(DistFile))
            {
                Log.LogError("DistFile File or Directory does not exist {0}", DistFile);
                return false;
            }

            try
            {
                Do();
            }
            catch (TizenSignException e)
            {
                Log.LogError(null, "TS0005", "", "", 0, 0, 0, 0, "Error occurred during tpk signing. Please check certificate file & password\n" + e.Message);
                return false;
            }
            catch (Exception e)
            {
                Log.LogError(e.Message);
                return false;
            }

            return true;
        }

        private void CreateSignedXML(string filename, string root, WidgetDigSig role, string pkcs12, string password)
        {
            pkcs12 = Path.GetFullPath(pkcs12);

            if (string.IsNullOrEmpty(password))
            {
                password = Utility.AskPassword($"Insert Password of {pkcs12} : ");
            }

            SHA256WithRSA sha = new SHA256WithRSA(pkcs12, password);
            XmldsigForWidgetDigSig xmld = new XmldsigForWidgetDigSig(role, sha, root);

            var sig = xmld;

            string signedXml = Path.Combine(root, filename);
            File.WriteAllText(signedXml, sig.ToString().Replace("\r\n", "\n"));
        }

        public void Do()
        {
            string Target = RootDir;
            string Output = OutputSignedTpk;

            string root = Target;
            string outputname = Path.GetFileName(Target);
            if (File.Exists(Target))
            {
                string tempdir = Utility.GetTempDirectory();

                DirectoryInfo tempdirinfo = Directory.CreateDirectory(tempdir);
                ZipFile.ExtractToDirectory(Target, tempdirinfo.FullName);
                root = tempdirinfo.FullName;
            }
            else if (!Directory.Exists(Target))
            {
                throw new InvalidOperationException($"Can not find a directory : {Target}");
            }
            else
            {
                outputname += ".tpk";
            }

            if (!string.IsNullOrEmpty(Output))
            {
                outputname = Output;
            }

            var dir = new DirectoryInfo(root);

            foreach (var file in dir.EnumerateFiles("*signature*.xml"))
            {
                file.Delete();
            }

            //Author Sign
            try
            {
                CreateSignedXML(AuthorSignatureFileName, root, WidgetDigSig.Author, AuthorFile, AuthorPassword);
            }
            catch (Exception e)
            {
                throw new TizenSignException("message : " + e.Message + "\n" + "file: " + AuthorFile);
            }

            //Dist Sign
            int count = 1;
            string filename = $"{DistributorSignatureFilePrefix}{count}{DistributorSignatureFilePostfix}";

            try
            {
                CreateSignedXML(filename, root, WidgetDigSig.Distributor, DistFile, DistPassword);
            }
            catch (Exception e)
            {
                throw new TizenSignException("message : " + e.Message + "\n" + "file: " + DistFile);
            }

            if (!String.IsNullOrEmpty(DistFile2) && !String.IsNullOrEmpty(DistPassword2))
            {
                count = 2;
                filename = $"{DistributorSignatureFilePrefix}{count}{DistributorSignatureFilePostfix}";
                try
                {
                    CreateSignedXML(filename, root, WidgetDigSig.Distributor, DistFile2, DistPassword2);
                }
                catch (Exception e)
                {
                    throw new TizenSignException("message : " + e.Message + "\n" + "file: " + DistFile2);
                }
            }

            //Create Tpk
            if (!String.IsNullOrEmpty(OutputSignedTpk))
            {
                if (File.Exists(outputname))
                {
                    File.Delete(outputname);
                }

                ZipFile.CreateFromDirectory(root, outputname, CompressionLevel.Optimal, false, new SlashEncoder());
            }
        }
    }
}
