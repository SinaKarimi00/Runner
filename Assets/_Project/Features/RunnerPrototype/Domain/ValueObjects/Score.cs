namespace Studio.Runner3d.Features.RunnerPrototype.Domain.ValueObjects
{
    public readonly struct Score
    {
        public static readonly Score Zero = new Score(0);

        public int Value { get; }

        public Score(int value)
        {
            Value = value < 0 ? 0 : value;
        }

        public Score Add(int amount)
        {
            return amount <= 0 ? this : new Score(Value + amount);
        }
    }
}
