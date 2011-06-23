/**********************************************************************
* Description:  Editor specific functions
* Created By:   Jason Offutt @ Central Christian Church of the East Valley
* Date Created: 2/22/2010
*
* $Workfile: editor.js $
* $Revision: 13 $
* $Header: /trunk/Arena/UserControls/Custom/Cccev/Core/js/editor.js   13   2011-06-23 15:14:57-07:00   JasonO $
*
* $Log: /trunk/Arena/UserControls/Custom/Cccev/Core/js/editor.js $
*  
*  Revision: 13   Date: 2011-06-23 22:14:57Z   User: JasonO 
*  
*  Revision: 12   Date: 2010-08-17 22:11:03Z   User: JasonO 
*  Adding jQuery trigger for "CONTENT_UPDATED" event on successful save. 
*  
*  Revision: 11   Date: 2010-03-11 16:45:33Z   User: JasonO 
*  Adding call to TinyMCE setContent() to ensure editor content is getting 
*  cleared out properly on save/cancel. 
*  
*  Revision: 10   Date: 2010-03-09 22:34:41Z   User: JasonO 
*  Adding exception handling to web service. 
*  
*  Revision: 9   Date: 2010-03-08 22:31:40Z   User: JasonO 
*  Updating to play nicely with UpdatePanels 
*  
*  Revision: 8   Date: 2010-03-05 17:37:30Z   User: JasonO 
*  Moving editor creation into client script to ensure only one is created. 
*  
*  Revision: 7   Date: 2010-03-01 20:15:17Z   User: JasonO 
**********************************************************************/

var central_textEditor = (function () {
    var _moduleContent;
    var _moduleInstanceID;
    var _editor;
    var _hiddenField;

    function _onSaveSuccess(result) {
        $("input[value='" + _moduleInstanceID + "']").siblings("div.content").html(tinymce_cleanup("insert_to_editor", _moduleContent));
        $("div.editor-overlay").fadeOut(function () { $("div.dimmer").hide(); });
        _clearEditor();
        $(document).trigger("CONTENT_RENDERED");
        return false;
    }

    function _onSaveFail(msg, errorText, thrownError) {
        _showErrorMessage(msg);
        _clearEditor();
        $("div.editor-overlay").hide();
        return false;
    }

    function _clearEditor() {
        _moduleContent = "";
        _moduleInstanceID = "";
        _hiddenField.val("");
        _editor.val("");
        tinyMCE.activeEditor.setContent("");
        $("div.dimmer").hide();
        $("div.editor-wrapper").hide();
        $("body").css("overflow", "auto");
    }

    function _buildEditor() {
        if ($("div.editor-wrapper").length == 0) {
            var html = '<div class="dimmer"></div>';
            html += '<div class="editor-wrapper">';
            html += '<img src="images/fancy_closebox.png" class="close-editor" alt="close" />';
            html += '<div class="editor">';
            html += '<div class="editor-overlay"></div>';
            html += '<input type="hidden" class="editor" id="moduleID" />';
            html += '<textarea class="editor mceEditor" id="mceEditor" height="200" width="400"></textarea>';
            html += '<input type="submit" class="editor" id="the_editor_submit" name="the_editor_submit" value="Update" />';
            html += '<input type="submit" class="editor" id="the_editor_cancel" name="the_editor_cancel" value="Cancel" />';
            html += '</div>';
            html += '</div>';
            $("form").append(html);
        }
    }

    function _showErrorMessage(msg) {
        var e = eval("(" + msg.responseText + ")");
        var html = '<div class="editor-errors">';
        html += '<h4 class="errorText">Oops! Something happened to your request.</h4>'
        html += '<span class="errorText">' + e.Message + '</span>';
        html += '</div>';
        $("input[value='" + _moduleInstanceID + "']").siblings("div.content").prepend(html);
    }

    return {
        init: function () {
            _buildEditor();
            tinyMCE.init(tinyMCE_params);

            _editor = $("textarea#mceEditor");
            _hiddenField = $("input#moduleID:hidden");

            $("div.dimmer").hide();
            $("div.editor-wrapper").hide();

            $("input.editor:image").click(function () {
                var moduleID = $(this).siblings("input.editor:hidden:first").val();
                _moduleInstanceID = moduleID;
                var content = $(this).siblings("div.content").html();
                _moduleContent = content;

                _hiddenField.val(moduleID);
                _editor.val(tinymce_cleanup("insert_to_editor", content));
                tinyMCE.activeEditor.setContent(content);
                $("body").css("overflow", "hidden");

                $("div.dimmer").show();
                $("div.editor-wrapper").show();
                $("div.editor-overlay").hide();
                return false;
            });

            $("input#the_editor_cancel.editor:submit, img.close-editor").click(function () {
                _clearEditor();
                return false;
            });

            $("#the_editor_submit").click(function () {
                $("div.editor-overlay").fadeIn();
                tinyMCE.get(_editor.attr("id")).save();
                _moduleContent = _editor.val();
                var postData = "{'moduleInstanceID':'" + _hiddenField.val() + "','content':'" + escape(_editor.val()) + "'}";

                $.ajax({
                    type: "POST",
                    url: "UserControls/Custom/Cccev/Core/TextEditor.asmx/SaveContent",
                    contentType: "application/json; charset=utf-8",
                    data: postData,
                    dataType: "json",
                    success: _onSaveSuccess,
                    error: _onSaveFail
                });

                return false;
            });
        }
    };
})();

$(function () {
    central_textEditor.init();
});

/**
 * Registering callbacks to interact with ASP.NET AJAX page lifecycle.
 * Executes when the request to the server is initiated.
 */
Sys.WebForms.PageRequestManager.getInstance().add_beginRequest(function () {
    var id = $("textarea.mceEditor").attr("id");
    var mce = tinyMCE.getInstanceById(id);

    if (mce != null && mce != "undefined") {
        tinyMCE.execCommand("mceRemoveControl", false, id);
    }
});

/**
* Registering callbacks to interact with ASP.NET AJAX page lifecycle.
* Executes when the request to the server has completed.
*/
Sys.WebForms.PageRequestManager.getInstance().add_endRequest(initTinyMceEditor);

function initTinyMceEditor() {
    central_textEditor.init();
}

/**
 * Callback required by TinyMCE to correctly escape HTML content to bypass Validate Request
 * http://tinymce.moxiecode.com/punbb/viewtopic.php?pid=30184#p30184
 *
 * @param type { string } kind of action
 * @param value { string } text value to escape
 */
function tinymce_cleanup(type, value) {
    if (type == "insert_to_editor") {
        value = value.replace(/&lt;/gi, "<");
        value = value.replace(/&gt;/gi, ">");
    }

    return value;
}

/**
 * Callback required by TinyMCE to save editor html to textarea
 *
 * @param element_id { string } html id of element in question
 * @param html { string } html content of said element
 * @param body { string } ???
 */
function tinymce_save(element_id, html, body) {
    var content = html.replace(/</gi, "&lt;");
    content = content.replace(/>/gi, "&gt;");
    return content;
}