using System;
using Apprenda.SaaSGrid;
using root.Contracts;
using root.Proxies;

namespace root
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            nameLabel.Text = UserContext.Instance.CurrentUser.FirstName;
        }

        protected void OnServiceCallButtonClick(object sender, EventArgs args)
        {
            Proxy p = new Proxy();
            statusMessage.Text = "Call completed. Result was " + p.SomeMethod(new DataTransferObject() { Result = 5 }).Result;
            p.Close();
        }
    }
}
