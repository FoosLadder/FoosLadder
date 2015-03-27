using System;

namespace FoosLadder.Website
{
    public class Global : FoosLadder.Api.Global
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            Start();
        }
    }
}