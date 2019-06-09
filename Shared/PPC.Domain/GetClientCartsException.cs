using System;
using System.Collections.Generic;
using System.Linq;

namespace PPC.Domain
{
    public class GetClientCartsException : AggregateException
    {
        public List<ClientCart> ClientCarts { get; private set; }

        public GetClientCartsException(IEnumerable<ClientCart> carts, IEnumerable<Exception> innerExceptions) : base(innerExceptions)
        {
            ClientCarts = carts.ToList();
        }
    }
}
