<%@ WebService Language="C#" Class="TextEditor" %>

using System;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using Arena.Core;
using Arena.Portal;
using Arena.Security;

[WebService(Namespace = "http://localhost/Arena")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[ScriptService]
public class TextEditor : WebService {

    [WebMethod(EnableSession = true)]
    public void SaveContent(int moduleInstanceID, string content)
    {
        var user = ArenaContext.Current.User;

        if (user.Identity.IsAuthenticated)
        {
            ModuleInstance module = new ModuleInstance(moduleInstanceID);
            
            if (module.Permissions.Allowed(OperationType.Edit, user))
            {
                module.Details = HttpContext.Current.Server.UrlDecode(content);
                module.Save(user.Identity.Name);
            }
            else
            {
                throw new Exception("You do not have permission to edit this page.");
            }
        }
        else
        {
            throw new Exception("You must be logged in to edit content on this page.");
        }
    }
    
}

