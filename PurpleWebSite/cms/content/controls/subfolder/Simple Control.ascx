<%@ Control Language="C#"  %>
<script runat="server">
    void Page_Load()
    {
        
    }
    void MyButton_Click(object sender, EventArgs e)
    {
        Output.Text = "You clicked it: " + Input.Text;
    }
</script>
<p>This content lives in a user control</p>


<asp:TextBox ID="Input" runat="server" />
<asp:Button ID="MyButton" runat="server" text="Test Me" onclick="MyButton_Click" />
<br />
<asp:Literal ID="Output" runat="server" />

