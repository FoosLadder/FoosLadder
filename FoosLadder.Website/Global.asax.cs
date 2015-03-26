using System;

namespace FoosLadder.Website
{
    public class Global : Api.Global
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            Start();
        }
    }
}