<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AdvHtmlText.ascx.cs" Inherits="ArenaWeb.UserControls.Custom.Cccev.Core.AdvHtmlText" %>
<asp:ScriptManagerProxy ID="smpScripts" runat="server" />

<div class="dynamic-content">
    <% if (editEnabled) { %>
        <input type="image" src="UserControls/Custom/Cccev/Core/images/edit.png" class="editor" onclick="return false;" />
    <% } %>
    <input type="hidden" runat="server" id="ihModuleInstanceID" class="editor" />
    <div class="content"><asp:PlaceHolder ID="phContent" runat="server" /></div>
</div>