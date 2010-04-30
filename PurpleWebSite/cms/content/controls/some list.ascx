<%@ Control Language="C#" %>
<script runat="server">
	public class MyData
	{
		public string Name { get; set; }
		public DateTime Date { get; set; }
	}
	
	void Page_Load()
	{
		MyRepeater.DataSource = new List<MyData>()
		{
			new MyData() { Name="Today", Date =DateTime.Now},
			new MyData() { Name="A while ago", Date =DateTime.Now.AddDays(-10000)},
		};
		MyRepeater.DataBind();
	}
</script>

<h2>My list</h2>

<p>This page is a simple *.ascx control with &lt;script runat="server"&gt; functionality.</p>

<asp:Repeater ID="MyRepeater" runat="server">
<HeaderTemplate><ul></HeaderTemplate>

<ItemTemplate>
	<li><%# Eval("Name") %> -- <%# Eval("Date", "{0:MM/dd/yyyy}") %></li>
</ItemTemplate>

<FooterTemplate></ul></FooterTemplate>

</asp:Repeater>