using System;

namespace Demo.Api.Shared
{
    public static class Verify
    {
        public static void IsNotNull(object o, string paramName)
        {
            if (o == null) throw new ArgumentNullException(paramName);
        }

        public static void IsGreaterThan(decimal d, decimal value, string paramName)
        {
            if (d <= value)
                throw new ArgumentOutOfRangeException(paramName, $"'${paramName}' must be greater than {value} but it was {d}");
        }

    }
}
