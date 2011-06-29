/**********************************************************************
* Description:  Alternative Rich Text Editor Control implementation of TinyMCE
* Created By:   Jason Offutt @ Central Christian Church of the East Valley
* Date Created: 2/18/2010
*
* $Workfile: AdvHtmlText.ascx.cs $
* $Revision: 20 $
* $Header: /trunk/Arena/UserControls/Custom/Cccev/Core/AdvHtmlText.ascx.cs   20   2011-06-28 16:54:24-07:00   JasonO $
*
* $Log: /trunk/Arena/UserControls/Custom/Cccev/Core/AdvHtmlText.ascx.cs $
*  
*  Revision: 20   Date: 2011-06-28 23:54:24Z   User: JasonO 
*  Updating TinyMCE version and adding optional font family/size selectors. 
*  
*  Revision: 19   Date: 2011-06-28 17:37:18Z   User: JasonO 
*  Adding support for optional usage of TinyMCE style selector. 
*  
*  Revision: 18   Date: 2011-06-23 22:14:46Z   User: JasonO 
*  
*  Revision: 17   Date: 2011-06-23 21:11:58Z   User: JasonO 
*  Cleaning up usings. 
*  
*  Revision: 16   Date: 2011-05-25 00:26:27Z   User: JasonO 
*  Adding editor js file with BasePage method rather than home-brewed Linq 
*  query. 
*  
*  Revision: 15   Date: 2010-12-02 21:19:03Z   User: JasonO 
*  Moving jQuery include up to <head> section via Arena's preferred method. 
*  
*  Revision: 14   Date: 2010-08-17 22:11:03Z   User: JasonO 
*  Adding jQuery trigger for "CONTENT_UPDATED" event on successful save. 
*  
*  Revision: 13   Date: 2010-03-11 23:38:59Z   User: JasonO 
*  Updating css and cleaning up comments. 
*  
*  Revision: 12   Date: 2010-03-09 22:34:41Z   User: JasonO 
*  Adding exception handling to web service. 
*  
*  Revision: 11   Date: 2010-03-08 22:31:40Z   User: JasonO 
*  Updating to play nicely with UpdatePanels 
*  
*  Revision: 10   Date: 2010-03-05 17:37:30Z   User: JasonO 
*  Moving editor creation into client script to ensure only one is created. 
*  
*  Revision: 9   Date: 2010-03-03 21:53:40Z   User: JasonO 
*  Fixing typo 
*  
*  Revision: 8   Date: 2010-03-03 21:51:04Z   User: JasonO 
*  Adding module setting to configure presence of FileManager plugin for 
*  TinyMCE. 
*  
*  Revision: 7   Date: 2010-03-02 20:55:43Z   User: JasonO 
**********************************************************************/

using System;
using System.Text;
using System.Web.UI;
using Arena.Portal;
using Arena.Security;

namespace ArenaWeb.UserControls.Custom.Cccev.Core
{
    public partial class AdvHtmlText : PortalControl
    {
        [BooleanSetting("Use Moxie Code File Manager", "Will inject the File Manager by MoxieCode into the TinyMCE Init script (defaults to true).", false, true)]
        public string UseMoxieFileManagerSetting { get { return Setting("UseMoxieFileManager", "true", false); } }

        [BooleanSetting("Enable rich text editing", "If checked, the stylesheet defined under 'Style Selector CSS Path' will be loaded into the editor and on the page, along with font size/family selectors.", false, false)]
        public string EnableRichTextEditingSetting { get { return Setting("EnableRichTextEditing", "false", false); } }

        [TextSetting("Style Selector CSS Path", "If set, will set a custom CSS document and enable the TinyMCE style selector.", false)]
        public string StyleSelectCssSetting { get { return Setting("StyleSelectCss", "usercontrols/custom/cccev/core/js/editor-styles.css", false); } }

        protected bool viewEnabled;
        protected bool editEnabled;
        private bool richTextEnabled;

        protected void Page_Load(object sender, EventArgs e)
        {
            richTextEnabled = bool.Parse(EnableRichTextEditingSetting);
            viewEnabled = CurrentModule.Permissions.Allowed(OperationType.View, CurrentUser);
            editEnabled = CurrentModule.Permissions.Allowed(OperationType.Edit, CurrentUser);
            
            if (viewEnabled)
            {
                RegisterClientScripts();
                BuildInitBlock();
            }

            ShowView();
        }
        
        private void RegisterClientScripts()
        {
            BasePage.AddCssLink(Page, "~/UserControls/Custom/Cccev/Core/js/tinymce/editor.css");
            BasePage.AddJavascriptInclude(Page, BasePage.JQUERY_INCLUDE);
            BasePage.AddJavascriptInclude(Page, "UserControls/Custom/Cccev/Core/js/tinymce/tiny_mce.js");

            // Add module-specific code through script manager to take advantage of client page lifecycle
            smpScripts.Scripts.Add(new ScriptReference("~/UserControls/Custom/Cccev/Core/js/editor.js"));

            if (richTextEnabled)
            {
                BasePage.AddCssLink(Page, StyleSelectCssSetting);
            }
        }

        private void BuildInitBlock()
        {
            // If using the File Manager plugin from MoxieCode (http://tinymce.moxiecode.com/plugins_filemanager.php) 
            // and you're running IIS7, you'll need to make sure to set the DefaultAppPool's 'Load User Profile'
            // setting to "True" to get the file manager to work correctly.
            // http://blogs.msdn.com/vijaysk/archive/2009/03/08/iis-7-tip-3-you-can-now-load-the-user-profile-of-the-application-pool-identity.aspx
            
            StringBuilder tinyMce = new StringBuilder();
            tinyMce.AppendLine("var tinyMCE_params = {");
            tinyMce.AppendLine("\tmode: \"specific_textareas\",");
            tinyMce.AppendLine("\teditor_selector: \"mceEditor\",");
            tinyMce.AppendLine("\ttheme: \"advanced\",");
            tinyMce.AppendLine("\tcleanup_callback: \"tinymce_cleanup\",");
            tinyMce.AppendLine("\tsave_callback: \"tinymce_save\",");
            tinyMce.AppendLine("\theight: \"400\",");
            tinyMce.AppendLine("\twidth: \"700\",");

            if (richTextEnabled)
            {
                tinyMce.AppendLine(string.Format("\tcontent_css: \"{0}\",", StyleSelectCssSetting));
            }

            tinyMce.AppendLine(string.Format("\tplugins: \"safari,inlinepopups,spellchecker,paste,media,fullscreen,tabfocus,table{0}\",", 
                bool.Parse(UseMoxieFileManagerSetting) ? ",filemanager" : string.Empty));
            tinyMce.AppendLine("\tskin: \"wp_theme\",");
            tinyMce.AppendLine("\tdialog_type: \"modal\",");
            tinyMce.AppendLine("\n\ttheme_advanced_buttons1: \"bold,italic,underline,strikethrough,|,formatselect,|,bullist,numlist,blockquote,|,outdent,indent,|justifyleft,justifycenter,justifyright,justifyfull,|,pastetext,pasteword,removeformat,|,image,link,unlink,|,media,charmap\",");
            tinyMce.AppendLine("\ttheme_advanced_buttons2: \"tablecontrols,|,undo,redo,|,code,fullscreen,help\",");
            tinyMce.AppendLine(string.Format("\ttheme_advanced_buttons3: \"{0}\",", richTextEnabled ? "styleselect,fontselect,fontsizeselect" : string.Empty));
            tinyMce.AppendLine("\ttheme_advanced_buttons4: \"\",");
            tinyMce.AppendLine("\n\ttheme_advanced_toolbar_location: \"top\",");
            tinyMce.AppendLine("\ttheme_advanced_toolbar_align: \"left\",");
            tinyMce.AppendLine("\ttheme_advanced_statusbar_location: \"bottom\",");
            tinyMce.AppendLine("\ttheme_advanced_resizing: false,");
            tinyMce.AppendLine("\trelative_urls: true");
            tinyMce.AppendLine("};");
            ScriptManager.RegisterClientScriptBlock(Page, GetType(), "InitTinyMCE", tinyMce.ToString(), true);
        }

        private void ShowView()
        {
            if (viewEnabled)
            {
                ModuleInstance module = new ModuleInstance(CurrentModule.ModuleInstanceID);
                phContent.Controls.Clear();
                phContent.Controls.Add(new LiteralControl(Server.HtmlDecode(module.Details)));
                ihModuleInstanceID.Value = module.ModuleInstanceID.ToString();
            }
        }
    }
}