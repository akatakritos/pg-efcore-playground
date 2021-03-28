using System;

namespace Demo.Api.Shared
{
    public static class Verify
    {
        public static Verifier<T> Param<T>(T value, string paramName)
        {
            return new(value, paramName);
        }
    }

    public readonly struct Verifier<T>
    {
        public T Value { get; }
        public string ParamName { get; }

        public Verifier(T value, string paramName)
        {
            Value = value;
            ParamName = paramName;
        }
    }

    public static class VerifyExtensions
    {
        public static Verifier<T> IsNotNull<T>(this Verifier<T> verifier) where T : class
        {
            if (verifier.Value == null)
            {
                throw new ArgumentNullException(verifier.ParamName);
            }

            return verifier;
        }

        public static Verifier<string> IsNotNullOrEmpty(this Verifier<string> verifier)
        {
            if (string.IsNullOrEmpty(verifier.Value))
            {
                var description = verifier.Value == null ? "null" : "empty";
                throw new ArgumentException(
                    $"'{verifier.ParamName}' should have contents but it was {description}", verifier.ParamName);
            }

            return verifier;
        }

        public static Verifier<decimal> IsGreaterThan(this Verifier<decimal> verifier, decimal target)
        {
            if (verifier.Value <= target)
            {
                throw new ArgumentOutOfRangeException(verifier.ParamName,
                    $"'{verifier.ParamName}' should be greater than {target} but it was {verifier.Value}");
            }

            return verifier;
        }

        public static Verifier<T> IsDefinedEnum<T>(this Verifier<T> verifier) where T : struct, Enum
        {
            if (!Enum.IsDefined(verifier.Value))
            {
                throw new ArgumentOutOfRangeException(verifier.ParamName,
                    $"Enum value {verifier.Value} was not a member of the {typeof(T).Name} enum");
            }

            return verifier;
        }
    }
}
