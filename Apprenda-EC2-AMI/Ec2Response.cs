using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Apprenda.SaaSGrid.Addons.AWS.EC2
{
    internal class EC2Response
    {
        internal int code { get; set; }

        internal string message { get; set; }

        internal string connectionData { get; set; }
    }
}