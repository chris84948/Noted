﻿using System;

namespace NotedUI.Models
{
    public class LinkConverter
    {
        public static string GetLnkTarget(string lnkPath)
        {
            try
            {
                var shl = new Shell32.Shell();         // Move this to class scope
                var dir = shl.NameSpace(System.IO.Path.GetDirectoryName(lnkPath));
                var itm = dir.Items().Item(System.IO.Path.GetFileName(lnkPath));
                var lnk = (Shell32.ShellLinkObject)itm.GetLink;
                return lnk.Target.Path;
            }
            catch (Exception)
            {
                return lnkPath;
            }
        }
    }
}
