using Ardalis.SmartEnum;

namespace Demo.Api.Domain
{
    public sealed class UnitOfMeasure : SmartEnum<UnitOfMeasure, int>
    {
        // can make subclasses for each of these to store additional data and behavior,
        // like a strategy pattern
        public static readonly UnitOfMeasure Teaspoon = new(nameof(Teaspoon), 1);
        public static readonly UnitOfMeasure Tablespoon = new(nameof(Tablespoon), 2);
        public static readonly UnitOfMeasure Cup = new(nameof(Cup), 3);
        public static readonly UnitOfMeasure Pint = new(nameof(Pint), 4);
        public static readonly UnitOfMeasure Quart = new(nameof(Quart), 5);
        public static readonly UnitOfMeasure Gallon = new(nameof(Gallon), 6);
        public static readonly UnitOfMeasure Ounce = new(nameof(Ounce), 7);

        private UnitOfMeasure(string name, int value) : base(name, value)
        {
        }
    }
}
