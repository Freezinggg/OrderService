using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Domain.Exception
{
    public sealed class InvalidStateTransitionException : DomainException
    {
        public InvalidStateTransitionException(string message)
            : base(message, FailureCategory.State)
        {
        }
    }
}
