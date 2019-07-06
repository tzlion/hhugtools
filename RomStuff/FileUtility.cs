﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace RomStuff
{
    class FileUtility
    {
        public static string selectInputFile()
        {
            OpenFileDialog filebox = new OpenFileDialog();
            filebox.Filter = "gb/gbc romz|*.gb;*.gbc";
            filebox.ShowDialog();
            return filebox.FileName;
        }

        public static string selectOutputFile()
        {
            OpenFileDialog outfilebox = new OpenFileDialog();
            outfilebox.Filter = "gbc rom|*.gbc|gb rom|*.gb";
            outfilebox.CheckFileExists = false;
            outfilebox.ShowDialog();
            return outfilebox.FileName;
        }

        public static string determineOutputFilename(string inputFilename)
        {
            string origoutfilename = Regex.Replace(inputFilename, "\\.([^\\.]+)$", ".fix.${1}");
            string outfilename = origoutfilename;
            int appendNumber = 0;
            while (System.IO.File.Exists(outfilename))
            {
                appendNumber++;
                outfilename = Regex.Replace(origoutfilename, "\\.fix\\.([^\\.]+)$", ".fix" + appendNumber + ".${1}");
            }
            return outfilename;
        }
    }
}
