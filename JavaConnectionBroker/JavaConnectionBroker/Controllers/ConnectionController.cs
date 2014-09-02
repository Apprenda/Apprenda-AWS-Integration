using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace JavaConnectionBroker.Controllers
{
    public class ConnectionController : ApiController
    {
        // GET: api/Connection
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Connection/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Connection
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Connection/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Connection/5
        public void Delete(int id)
        {
        }
    }
}
