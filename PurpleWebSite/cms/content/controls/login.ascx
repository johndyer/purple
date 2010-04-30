<%@ Control Language="C#"%>
<script runat="server">
	bool testMode = false;
	
	void Page_Load()
	{		
		if (!IsPostBack) {
			if (HttpContext.Current.User.Identity.IsAuthenticated)
			{
				IsLoggedInView.Visible = true;
				LoginFormHolder.Visible = false;

				string returnUrl = Request["ReturnUrl"];
				string loginUrl = Request["LoginUrl"];

				if (returnUrl == "")
					Response.Redirect("/");
				else
					Response.Redirect(returnUrl);
				
			} else {
				IsLoggedInView.Visible = false;
				LoginFormHolder.Visible = true;							
			} 
			
		}
	
	}

	void Login_Click(object sender, EventArgs e)
	{
		Page.Validate();

		if (Page.IsValid)
		{
			string username = Username.Text;
			string password = Password.Text;
			bool rememberMe = RememberMe.Checked;

			string returnUrl = Request["ReturnUrl"];
			string loginUrl = Request["LoginUrl"];

			// TODO: replace with Membership
			//if (username=="admin" && password == "password")
			if (Membership.ValidateUser(username, password))		
			{
				FormsAuthentication.SetAuthCookie(username, rememberMe);
				FormsAuthentication.RedirectFromLoginPage(username, rememberMe);

			}
			else
			{
				// invalid login
				MessageHolder.Visible = true;
				ErrorMessage.Text = "Invalid Login";
			}
		}
		else
		{
			// not filled out
			MessageHolder.Visible = true;
			ErrorMessage.Text = "Invalid Login";
		}
	}
</script>



	<asp:PlaceHolder ID="MessageHolder" runat="server" Visible="false">
		<div id="message" class="message-error mainbox">
			<asp:Literal ID="ErrorMessage" runat="server" />	
		</div>
	</asp:PlaceHolder>
	
	<div id="login" class="mainbox">

		<asp:PlaceHolder ID="IsLoggedInView" runat="server">
				You are logged in. <a href="logout.aspx">Logout</a> if you wish.
		</asp:PlaceHolder>
		
		<asp:PlaceHolder ID="LoginFormHolder" runat="server">
					
				<table>
					<tr>
						<td>Username</td>
						<td><asp:TextBox ID="Username" runat="server" /></td>
					</tr>
					<tr>
						<td>Password</td>
						<td><asp:TextBox ID="Password" TextMode="Password" runat="server" /></td>
					</tr>
					<tr>
						<td></td>
						<td>
							<asp:CheckBox ID="RememberMe" runat="server" Text="Keep me logged in" />
						</td>
					</tr>
					<tr>
						<td colspan="2">
							<asp:Button ID="LoginButton" runat="server" Text="Login" OnClick="Login_Click" />
						</td>
					</tr>		
				</table>
		
		</asp:PlaceHolder>
	</div>

	<p>For now, just use username=admin and password=password.</p>