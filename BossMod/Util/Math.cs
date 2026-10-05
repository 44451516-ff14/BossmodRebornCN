namespace BossMod;

public static class UIntExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPrime(this uint number)
    {
        if (number <= 1u)
        {
            return false;
        }

        if (number is 2u or 3u or 5u or 7u or 11u or 13u or 17u or 19u or 23u or 29u or 31u or 37u or 41u or 43u) // no known case in the game needs prime numbers higher than 43
        {
            return true;
        }

        if (number < 47 || (number & 1u) == 0u) // already checked all prime numbers below 47
        {
            return false;
        }

        if (number % 3u == 0u)
        {
            return number == 3u;
        }

        var limit = (uint)Math.Sqrt(number);

        for (var divisor = 5u; divisor <= limit; divisor += 6u)
        {
            if (number % divisor == 0u || number % (divisor + 2u) == 0u)
            {
                return false;
            }
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDivisible(this uint dividend, uint divisor) => dividend % divisor == 0f;
}
