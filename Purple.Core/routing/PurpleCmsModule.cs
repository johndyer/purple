using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Purple.Core
{
	public class PurpleCmsModule : IHttpModule
	{
		public void Dispose()
		{			
		}

		public void Init(HttpApplication app)
		{
			app.BeginRequest += new EventHandler(Application_BeginRequest);
		}

		protected void Application_BeginRequest(object sender, EventArgs e)
		{

			
		}
	}
}
