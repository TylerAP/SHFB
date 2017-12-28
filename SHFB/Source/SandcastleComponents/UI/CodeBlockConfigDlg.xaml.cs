﻿//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : CodeBlockConfigDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/20/2017
// Note    : Copyright 2006-2017, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a form that is used to configure the settings for the Code Block Component.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 11/24/2006  EFW  Created the code
// 02/12/2007  EFW  Added code block language filter option, default title option, and "Copy" image URL
// 03/05/2008  EFW  Added support for the keepSeeTags attribute
// 04/05/2008  EFW  JavaScript and XAMl are now treated as separate languages for proper language filter support
// 08/15/2008  EFW  Added option to allow missing source/regions
// 01/23/2009  EFW  Added removeRegionMarkers option
// 10/21/2012  EFW  Removed obsolete options and moved options over from the Post-Transform Component
//                  configuration dialog box.
// 12/20/2017  EFW  Converted the form to WPF for better high DPI scaling support on 4K displays
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Xml.Linq;
using Microsoft.Win32;
using Sandcastle.Core;

namespace SandcastleBuilder.Components.UI
{
    /// <summary>
    /// This form is used to configure the settings for the <see cref="CodeBlockComponent"/>
    /// </summary>
    public partial class CodeBlockConfigDlg : Window
    {
        #region Private data members
        //=====================================================================

        private XElement config;     // The configuration

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to return the configuration information
        /// </summary>
        public string Configuration => config.ToString();

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="currentConfig">The current XML configuration XML fragment</param>
        public CodeBlockConfigDlg(string currentConfig)
        {
            InitializeComponent();

            var languages =(new Dictionary<string, string> {
                { "none", "None" },
                { "cs", "C#" },
                { "vbnet", "VB.NET" },
                { "cpp", "C++" },
                { "fs", "F#" },
                { "xml", "XML" },
                { "xaml", "XAML" },
                { "python", "Python" },
                { "sql", "SQL" },
                { "pshell", "PowerShell" },
                { "c", "C" },
                { "javascript", "JavaScript" },
                { "jscriptnet", "JScript.NET" },
                { "jsharp", "J#" },
                { "vbscript", "VBScript" } }).ToList();

            cboLanguage.ItemsSource = languages;
            cboLanguage.SelectedIndex = 1;

            // Load the current settings
            config = XElement.Parse(currentConfig);

            txtBasePath.Text = (string)config.Element("basePath")?.Attribute("value");
            chkAllowMissingSource.IsChecked = ((bool?)config.Element("allowMissingSource")?.Attribute("value") ?? false);
            chkRemoveRegionMarkers.IsChecked = ((bool?)config.Element("removeRegionMarkers")?.Attribute("value") ?? false);

            var node = config.Element("colorizer");

            if(node != null)
            {
                txtSyntaxFile.Text = (string)node.Attribute("syntaxFile");
                txtXsltStylesheetFile.Text = (string)node.Attribute("styleFile");
                txtCssStylesheet.Text = (string)node.Attribute("stylesheet");
                txtScriptFile.Text = (string)node.Attribute("scriptFile");

                var attr = node.Attribute("language");

                if(attr != null && languages.Any(l => l.Key == attr.Value))
                    cboLanguage.SelectedValue = attr.Value;

                int tabSize = ((int?)node.Attribute("tabSize") ?? 0);

                if(tabSize < 0 || tabSize > 25)
                    tabSize = 0;

                udcTabSize.Value = tabSize;
                chkNumberLines.IsChecked = ((bool?)node.Attribute("numberLines") ?? false);
                chkOutlining.IsChecked = ((bool?)node.Attribute("outlining") ?? false);
                chkKeepSeeTags.IsChecked = ((bool?)node.Attribute("keepSeeTags") ?? false);
                chkDefaultTitle.IsChecked = ((bool?)node.Attribute("defaultTitle") ?? true);
            }

            if(String.IsNullOrWhiteSpace(txtSyntaxFile.Text))
                txtSyntaxFile.Text = @"{@SHFBFolder}PresentationStyles\Colorizer\highlight.xml";

            if(String.IsNullOrWhiteSpace(txtXsltStylesheetFile.Text))
                txtXsltStylesheetFile.Text = @"{@SHFBFolder}PresentationStyles\Colorizer\highlight.xsl";

            if(String.IsNullOrWhiteSpace(txtCssStylesheet.Text))
                txtCssStylesheet.Text = @"{@SHFBFolder}PresentationStyles\Colorizer\highlight.css";

            if(String.IsNullOrWhiteSpace(txtScriptFile.Text))
                txtScriptFile.Text = @"{@SHFBFolder}PresentationStyles\Colorizer\highlight.js";
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// Validate the configuration and save it
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            bool isValid = true;

            txtBasePath.Text = txtBasePath.Text.Trim();
            txtSyntaxFile.Text = txtSyntaxFile.Text.Trim();
            txtXsltStylesheetFile.Text = txtXsltStylesheetFile.Text.Trim();
            txtCssStylesheet.Text = txtCssStylesheet.Text.Trim();
            txtScriptFile.Text = txtScriptFile.Text.Trim();

            txtSyntaxFile.SetValidationState(true, null);
            txtXsltStylesheetFile.SetValidationState(true, null);
            txtCssStylesheet.SetValidationState(true, null);
            txtScriptFile.SetValidationState(true, null);

            if(txtSyntaxFile.Text.Length == 0)
            {
                txtSyntaxFile.SetValidationState(false, "The syntax filename is required");
                isValid = false;
            }

            if(txtXsltStylesheetFile.Text.Length == 0)
            {
                txtXsltStylesheetFile.SetValidationState(false, "The XSLT style sheet filename is required");
                isValid = false;
            }

            if(txtCssStylesheet.Text.Length == 0)
            {
                txtCssStylesheet.SetValidationState(false, "The CSS style sheet filename is required");
                isValid = false;
            }

            if(txtScriptFile.Text.Length == 0)
            {
                txtScriptFile.SetValidationState(false, "The script filename is required");
                isValid = false;
            }

            if(isValid)
            {
                // Store the changes
                config.RemoveNodes();

                config.Add(
                    new XElement("basePath",
                        new XAttribute("value", txtBasePath.Text)),
                    new XElement("outputPaths", "{@HelpFormatOutputPaths}"),
                    new XElement("allowMissingSource",
                        new XAttribute("value", chkAllowMissingSource.IsChecked)),
                    new XElement("removeRegionMarkers",
                        new XAttribute("value", chkRemoveRegionMarkers.IsChecked)),
                    new XElement("colorizer",
                        new XAttribute("syntaxFile", txtSyntaxFile.Text),
                        new XAttribute("styleFile", txtXsltStylesheetFile.Text),
                        new XAttribute("stylesheet", txtCssStylesheet.Text),
                        new XAttribute("scriptFile", txtScriptFile.Text),
                        new XAttribute("disabled", "{@DisableCodeBlockComponent}"),
                        new XAttribute("language", cboLanguage.SelectedValue),
                        new XAttribute("tabSize", (int)udcTabSize.Value),
                        new XAttribute("numberLines", chkNumberLines.IsChecked),
                        new XAttribute("outlining", chkOutlining.IsChecked),
                        new XAttribute("keepSeeTags", chkKeepSeeTags.IsChecked),
                        new XAttribute("defaultTitle", chkDefaultTitle.IsChecked)));

                this.DialogResult = true;
                this.Close();
            }
        }

        /// <summary>
        /// Select the output folder for the IntelliSense files
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            using(System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog())
            {
                dlg.Description = "Select the base source code folder";
                dlg.SelectedPath = Directory.GetCurrentDirectory();

                // If selected, set the new folder
                if(dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    txtBasePath.Text = dlg.SelectedPath + @"\";
            }
        }

        /// <summary>
        /// Select one of the file types
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            TextBox t;

            var dlg = new OpenFileDialog();

            if(b == btnSelectSyntax)
            {
                t = txtSyntaxFile;
                dlg.Title = "Select the language syntax file";
                dlg.Filter = "XML files (*.xml)|*.xml|All Files (*.*)|*.*";
                dlg.DefaultExt = "xml";
            }
            else
                if(b == btnSelectXsltStylesheet)
                {
                    t = txtXsltStylesheetFile;
                    dlg.Title = "Select the XSL transformation file";
                    dlg.Filter = "XSL files (*.xsl, *.xslt)|*.xsl;*.xslt|All Files (*.*)|*.*";
                    dlg.DefaultExt = "xsl";
                }
                else
                    if(b == btnSelectCssStylesheet)
                    {
                        t = txtCssStylesheet;
                        dlg.Title = "Select the colorized code style sheet file";
                        dlg.Filter = "Style sheet files (*.css)|*.css|All Files (*.*)|*.*";
                        dlg.DefaultExt = "css";
                    }
                    else
                    {
                        t = txtScriptFile;
                        dlg.Title = "Select the colorized code JavaScript file";
                        dlg.Filter = "JavaScript files (*.js)|*.js|All Files (*.*)|*.*";
                        dlg.DefaultExt = "js";
                    }

            dlg.InitialDirectory = Directory.GetCurrentDirectory();

            // If selected, set the filename
            if(dlg.ShowDialog() ?? false)
                t.Text = dlg.FileName;
        }

        /// <summary>
        /// Go to the Sandcastle Help File Builder project site
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lnkProjectSite_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(e.Uri.ToString());
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                System.Windows.MessageBox.Show("Unable to launch link target.  Reason: " + ex.Message,
                    "Code Block Component", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
        #endregion
    }
}
