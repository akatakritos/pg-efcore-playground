using Ardalis.SmartEnum;
using MediatR;
using Microsoft.Extensions.Options;

namespace Demo.Api.Domain
{
    public sealed class UnitOfMeasure : SmartEnum<UnitOfMeasure, int>
    {
        private UnitOfMeasure(string name, int value): base(name, value){}

        // can make subclasses for each of these to store additional data and behavior,
        // like a strategy pattern
        public static readonly UnitOfMeasure Teaspoon = new UnitOfMeasure(nameof(Teaspoon), 1);
        public static readonly UnitOfMeasure Tablespoon = new UnitOfMeasure(nameof(Tablespoon), 2);
        public static readonly UnitOfMeasure Cup = new UnitOfMeasure(nameof(Cup), 3);
        public static readonly UnitOfMeasure Pint = new UnitOfMeasure(nameof(Pint), 4);
        public static readonly UnitOfMeasure Quart = new UnitOfMeasure(nameof(Quart), 5);
        public static readonly UnitOfMeasure Gallon = new UnitOfMeasure(nameof(Gallon), 6);
        public static readonly UnitOfMeasure Ounce = new UnitOfMeasure(nameof(Ounce), 7);
    }
}
