using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Domain.Exception
{
    public sealed class PolicyViolationException : DomainException
    {
        public PolicyViolationException(string message)
            : base(message, FailureCategory.Policy)
        {
        }
    }
}
