<%@ Control Language="C#" ClientIDMode="Static" %>
<script runat="server">


</script>

<script type="text/javascript" src="/cms/assets/js/ckeditor/ckeditor.js"></script> 
<script type="text/javascript" src="/cms/assets/js/ckeditor/adapters/jquery.js"></script>
<script type="text/javascript" src="/cms/assets/js/tiny_mce/jquery.tinymce.js"></script>


<div class="admin-box" id="admin-page-menu">
	<b>Admin:</b> 
	<a href="javascript:void(0);" id="admin-page-properties" class="admin-link admin-link-page-properties">Properties</a> 
	<asp:LinkButton ID="WebpageSaveButton" Text="Save Changes" runat="server" CssClass="admin-link admin-link-save-page" />
	<%-- <asp:Button ID="WebpageSaveButton" Text="Save Changes" runat="server" />--%>
	<a href="<%= Request.Url.AbsolutePath %>" class="admin-link  admin-link-page-cancel">Cancel</a>
</div>


<div id="admin-page-properties-box" class="admin-box" style="display:none;">
    <ul class="admin-tabs">
        <li><a href="#p-tab-main" class="selected">Main</a></li>
        <li><a href="#p-tab-design">Design</a></li>
		<li><a href="#p-tab-settings">Settings</a></li>            
    </ul>
	<div class="clear"></div>
    <div class="admin-sheets">
        <div class="sheet" id="p-tab-main">
        	
			<fieldset>
			<legend>URL</legend>
			<table>
				<tr>
					<td>Filename</td>
					<td>
						<asp:textBox ID="WebpageFilename" runat="Server" CssClass="" />				
					</td>
				</tr>
				<tr>
					<td>Default Url</td>
					<td>
						<asp:Label ID="WebpageFilenamePrefix" runat="server" /><span id="WebpageFilenameDisplay">test</span>
					</td>
				</tr>
				<tr>
					<td>Customized URL</td>
					<td><asp:textBox ID="WebpageUrl" runat="Server" /></td>
				</tr>
				<tr>
					<td></td>
					<td><input type="button" id="webpageSetToDefaultUrl" value="Set to Default URL" /> </td>
				</tr>	
			</table>

			</fieldset>

			
			<fieldset>
			<legend>Title, SEO</legend>

			<table>
				<tr>
					<td>Page Title</td>
					<td><asp:TextBox ID="WebpagePageTitle" runat="server" /></td>
				</tr>
				<tr>
					<td>Menu Title</td>
					<td><asp:TextBox ID="WebpageMenuTitle" runat="server" /></td>
				</tr>
				<tr>
					<td>Meta Description</td>
					<td><asp:textBox ID="WebpageMetaDescription" runat="Server" TextMode="MultiLine" /></td>
				</tr>
				<tr>
					<td>Meta Keywords</td>
					<td><asp:textBox ID="WebpageMetaKeywords" runat="Server" TextMode="MultiLine" /></td>
				</tr>			
			</table>

			</fieldset>


        </div>


        <div class="sheet" id="p-tab-design" style="display:none;">
			<fieldset>
				<legend>Menu</legend>
				<table>
					<tr>
						<td>Menu Type</td>
						<td>
							<asp:CheckBox ID="WebpageShowInMenu" Text="Show in Menu" runat="Server" />		
							as					
							<asp:DropDownList ID="WebpageMenuType" runat="server" />
						</td>
					</tr>	
					<tr>
						<td>Sort Order</td>
						<td>
							
							<asp:TextBox ID="WebpageSortOrder" Text="" runat="Server" />
						</td>
					</tr>													        
				</table>
			</fieldset>


			<fieldset>
				<legend>MasterPage</legend>
				<asp:RadioButtonList ID="WebpageMasterPageFilename" runat="Server" RepeatLayout="UnorderedList" CssClass="masterpages"></asp:RadioButtonList>
			</fieldset>

			<fieldset>
				<legend>Common Header</legend>
                <asp:CheckBox ID="WebpageIgnoreParentHeader" Text="Ignore Parent Header" runat="Server" />	
                <br />
				<asp:TextBox ID="WebpageCommonAreaHeader" runat="Server" TextMode="MultiLine" CssClass="purple-commonareaheader" />
			</fieldset>
			
			
		</div>
        <div class="sheet" id="p-tab-settings" style="display:none;">
			
			<fieldset>
				<legend>Allowed Editors</legend>
				<table>
					<tr>
						<td>Editors</td>
						<td><asp:CheckBoxList ID="WebpageEditors" runat="Server" /></td>
					</tr>
                    <tr>
                        <td>Settings</td>
                        <td>
			                   <asp:CheckBox ID="WebpageForceSsl" Text="Force SSL" runat="Server" />						
                               <br />
			                   <asp:TextBox ID="WebpageContentExpirationDate" runat="server" />
                        </td>
                    </tr>									        
				</table>
			</fieldset>
        </div>            
 
    </div>

</div>

<script>
jQuery(function ($) {
    // TABS

    $('.admin-tabs a').click(function (e) {
        e.preventDefault();
        $(this).parent().siblings().find('a').removeClass('selected');
        $(this).addClass('selected');
        $($(this).attr('href')).show().siblings().hide();
    });


    $('#admin-page-properties').toggle(function () {
        $('#admin-page-properties-box').show();
    }, function () {
        $('#admin-page-properties-box').hide();
    });


    // URL/filename stuff
    $('#WebpageFilename').keydown(function (e) {
        var code = (e.keyCode ? e.keyCode : e.which);
        console.log(code);
        if (code == 32) { // space
            return false;
        }
    });
    $('#WebpageFilename').keyup(function () {
        $('#WebpageFilenameDisplay').html($(this).val());
    });
    $('#WebpageFilenameDisplay').html($('#WebpageFilename').val());


    function setUrlToDefault() {
        $('#WebpageUrl').val($('#WebpageFilenamePrefix').html() + $('#WebpageFilename').val());
    }
    $('#webpageSetToDefaultUrl').click(setUrlToDefault);
    $('#WebpageUrl').change(function () {
        if ($(this).val() == '')
            setUrlToDefault();
    });


    // HTML Editor
    // grab all the link elements and push them into a string
    var cssUrls = [];

    var links = document.getElementsByTagName('link');
    for (var i = 0; i < links.length; i++) {
        if (links[i].rel.toLowerCase() === 'stylesheet')
            cssUrls.push(links[i].href);
    }

    
    // ckeditor 
    //$('.htmleditor').ckeditor({
    
    CKEDITOR.replace( $('.htmleditor')[0],{
        contentsCss: cssUrls
        , fillEmptyBlocks: false
        , bodyClass: 'richEditor container_16'
        , skin: 'office2003'
        , toolbar: [
            ['Bold', 'Italic', 'Underline', 'Strike', '-', 'Subscript', 'Superscript'],
            ['NumberedList', 'BulletedList', '-', 'Blockquote'],
            '/',
            ['Source', 'Format'],
            ['Link', 'Unlink', '-', 'Image', 'Table', 'HorizontalRule', 'SpecialChar', '-', 'PasteText', 'PasteFromWord']
        ]
        , on: {
            instanceReady: function (ev) {
                this.dataProcessor.writer.setRules('p', {
                    indent: false,
                    breakBeforeOpen: true,
                    breakAfterOpen: false,
                    breakBeforeClose: false,
                    breakAfterClose: true
                });
                this.dataProcessor.writer.setRules('h1', {
                    indent: false,
                    breakBeforeOpen: true,
                    breakAfterOpen: false,
                    breakBeforeClose: false,
                    breakAfterClose: true
                });
                this.dataProcessor.writer.setRules('h2', {
                    indent: false,
                    breakBeforeOpen: true,
                    breakAfterOpen: false,
                    breakBeforeClose: false,
                    breakAfterClose: true
                });
                this.dataProcessor.writer.setRules('h3', {
                    indent: false,
                    breakBeforeOpen: true,
                    breakAfterOpen: false,
                    breakBeforeClose: false,
                    breakAfterClose: true
                });
                this.dataProcessor.writer.setRules('h4', {
                    indent: false,
                    breakBeforeOpen: true,
                    breakAfterOpen: false,
                    breakBeforeClose: false,
                    breakAfterClose: true
                });
                this.dataProcessor.writer.setRules('td', {
                    indent: false,
                    breakBeforeOpen: true,
                    breakAfterOpen: false,
                    breakBeforeClose: false,
                    breakAfterClose: true
                });
                this.dataProcessor.writer.setRules('th', {
                    indent: false,
                    breakBeforeOpen: true,
                    breakAfterOpen: false,
                    breakBeforeClose: false,
                    breakAfterClose: true
                });
                this.dataProcessor.writer.setRules('li', {
                    indent: true,
                    breakBeforeOpen: true,
                    breakAfterOpen: false,
                    breakBeforeClose: false,
                    breakAfterClose: true
                });
                this.dataProcessor.writer.setRules('div', {
                    indent: true,
                    breakBeforeOpen: true,
                    breakAfterOpen: true,
                    breakBeforeClose: true,
                    breakAfterClose: false
                });
            }
        }
    });
    
    /*

    // tiny_mce
    $('.htmleditor').tinymce({
        // Location of TinyMCE script
        script_url: '/cms/assets/js/tiny_mce/tiny_mce.js',
        // General options
        theme: "advanced",
        plugins: "pagebreak,style,layer,table,save,advhr,advimage,advlink,emotions,iespell,inlinepopups,insertdatetime,preview,media,searchreplace,print,contextmenu,paste,directionality,fullscreen,noneditable,visualchars,nonbreaking,xhtmlxtras,template,advlist",

        // Theme options
        theme_advanced_buttons1: "code,bold,italic,underline,strikethrough,sub,sup,|,cut,copy,paste,pastetext,pasteword,|,undo,redo,|,search,replace",
        theme_advanced_buttons2: "formatselect,styleselect,|,bullist,numlist,|,outdent,indent,blockquote,|,link,unlink,anchor,image,cleanup",
        theme_advanced_buttons3: '',
        //theme_advanced_buttons3: "tablecontrols,|,hr,removeformat,charmap,",
        //theme_advanced_buttons4: "insertlayer,moveforward,movebackward,absolute,|,styleprops,|,cite,abbr,acronym,del,ins,attribs,|,visualchars,nonbreaking,template,pagebreak",
        theme_advanced_toolbar_location: "top",
        theme_advanced_toolbar_align: "left",
        theme_advanced_statusbar_location: "bottom",
        theme_advanced_resizing: true,


        apply_source_formatting: true, // indent text (crap: taken out of 3.0

        content_css: cssUrls.join(','),

        relative_urls: false,
        remove_script_host: true,

        extended_valid_elements: 'style[*],object[*],embed[*],script[*]',


        forced_root_block : false,
        force_p_newlines : false,
        remove_linebreaks : false
        //force_br_newlines : true,
        //remove_trailing_nbsp : false,   
        //verify_html : false,
    });
    */

});
</script>