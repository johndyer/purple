<%@ Control Language="C#" %>
<script runat="server">


</script>

<div class="purple-admin-area-edit">
	<div class="purple-admin-area-panel">
		Control: <asp:DropDownList ID="ControlName" runat="server" ClientIDMode="AutoID" /> 
        <br />Upload: <asp:FileUpload ID="ControlUploadFile"  ClientIDMode="AutoID" runat="server" /> to <asp:DropDownList ID="ControlUploadFolder" runat="server" ClientIDMode="AutoID" /><asp:Button ID="ControlUploadButton" Text="Upload" ClientIDMode="AutoID" runat="server" />				
	</div>
	<div class="purple-admin-html-editor">
		HTML<br/>
		<asp:Textbox ID="HtmlContent" runat="server" TextMode="MultiLine" CssClass="htmleditor" width="100%"  ClientIDMode="AutoID" />
	</div>
</div>

<script type="text/javascript">
jQuery(function () {

	function checkHtml(ddl) {
		var controlVal = ddl.val();
		var htmlArea = ddl.closest('.purple-admin-area-edit').find('.purple-admin-html-editor');

		if (controlVal == '') {
			htmlArea.show();
		} else {
			htmlArea.hide();
		}
	}

	checkHtml($('#<%= ControlName.ClientID %>'));

	$('#<%= ControlName.ClientID %>').change(function () {
		checkHtml($(this));
	});
});
</script>