<%@ Application  Language="C#" %>
<%@ Import Namespace="System.Web.Routing" %>

<script runat="server">

	protected void Application_Start(object sender, EventArgs e)
	{
		RegisterRoutes(RouteTable.Routes);
	}

	protected void Session_Start(object sender, EventArgs e)
	{

	}

	// http://msdn.microsoft.com/en-us/library/cc668201.aspx
	public static void RegisterRoutes(RouteCollection routes)
	{
		// don't want to slow down the system
		routes.RouteExistingFiles = false;
		
		// custom
		routes.Ignore("scripts/{*css}");
		routes.Ignore("styles/{*css}");

		// this is the one line to make PurpleCMS work
		Purple.Core.PurpleRoutes.AddRoutes(routes);
	}

	protected void Application_AuthenticateRequest(object sender, EventArgs e)
	{

	}

	protected void Application_Error(object sender, EventArgs e)
	{

	}

	protected void Session_End(object sender, EventArgs e)
	{

	}

	protected void Application_End(object sender, EventArgs e)
	{

	}


</script>
