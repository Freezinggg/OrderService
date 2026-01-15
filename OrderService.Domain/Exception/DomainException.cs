using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Domain.Exception
{
    public abstract class DomainException : System.Exception
    {
        public FailureCategory Category { get; }

        protected DomainException(string message, FailureCategory category)
            : base(message)
        {
            Category = category;
        }

        public enum FailureCategory
        {
            Invariant,   // invalid data / invariant failure, cannot exist
            Policy,      // business rejection
            State        // invalid transition
        }
    }
}
