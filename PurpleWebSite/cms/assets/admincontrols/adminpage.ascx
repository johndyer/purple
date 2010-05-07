<%@ Control Language="C#" ClientIDMode="Static" %>
<script runat="server">


</script>


<div class="admin-box" id="admin-page-menu">
	<b>Admin:</b> 
	<a id="admin-add-page" href="javascript:void(0);" class="admin-link admin-link-add-page">Add Subpage</a> 
	<a id="admin-page-versions" href="javascript:void(0);" class="admin-link admin-link-page-versions">Versions</a> 
	<a id="admin-edit-page" href="<%= Request.Url.AbsolutePath + "?pagemode=edit" %>" class="admin-link admin-link-edit-page">Edit</a>
</div>

<div id="admin-add-page-box" class="admin-box" style="display:none;">
	<fieldset>
	<legend>Subpage Properties</legend>
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
				<td>Filename</td>
				<td><asp:TextBox ID="WebpageFilename" runat="server" /></td>
			</tr>
			<tr>
				<td>Default URL</td>
				<td><asp:Label id="WebpageFilenamePrefix" runat="server" /><span id="WebpageFilenameDisplay">empty</span>
			</tr>
			<tr>
				<td>Menu Type</td>
				<td><asp:DropDownList ID="WebpageMenuType" runat="server" /></td>
			</tr>
		</table>
	</fieldset>

	<fieldset>
		<legend>MasterPage</legend>
		<asp:RadioButtonList ID="WebpageMasterPageFilename" runat="Server" RepeatLayout="UnorderedList" CssClass="masterpages"></asp:RadioButtonList>
	</fieldset>

	
	<asp:Button ID="WebpageAddButton" Text="Add Subpage" runat="server" />
</div>

<div id="admin-page-versions-box" class="admin-box" style="display:none;">
	<h5>Page Versions</h5>
	<asp:Repeater ID="WebpageRevisionsRepeater" runat="server">
		<HeaderTemplate>
            <table>
                <thead>
                    <th>Date</th>
                    <th>User</th>
                    <th>Status</th>
                    <th>Action</th>
                </thead>
                <tbody>        
        </HeaderTemplate>
		<ItemTemplate>
			        <tr>
                        <td><%# Eval("RevisedDate", "{0:MM/dd/yy mm:HH tt}") %></td>
                        <td><%# Eval("RevisedByUsername") %></td>
                        <td><%# Eval("IsPublished") %></td>
                        <td><input type="button" value="Publish" /></td>
                    </tr>
		</ItemTemplate>
		<HeaderTemplate>
                </tbody>
            </table>
        </HeaderTemplate>
	</asp:Repeater>
</div>

<script>
jQuery(function ($) {

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



	$('#WebpagePageTitle').change(function () {
		if ($('#WebpageMenuTitle').val() == '')
			$('#WebpageMenuTitle').val($(this).val());

		if ($('#WebpageFilename').val() == '')
			$('#WebpageFilename').val($(this).val().replace(/ /gi, '').toLowerCase());
	});

	// show/hide main panel
	$('#admin-add-page').toggle(function () {
		$('#admin-add-page-box').show();
	}, function () {
		$('#admin-add-page-box').hide();
	});

	$('#admin-page-versions').toggle(function () {
		$('#admin-page-versions-box').show();
	}, function () {
		$('#admin-page-versions-box').hide();
	});
});
</script>