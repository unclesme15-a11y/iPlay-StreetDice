using System;

namespace IPlay.Demo
{
    // Decisions use public bankroll/turn information only; no dice outcome enters this API.
    public static class DemoOpponentPolicy
    {
        private static readonly int[] Amounts = { 1, 5, 10, 20 };
        private static readonly int[] PreferredTier = { 1, 3, 0, 2 };
        private static readonly double[] Willingness = { 0.68, 0.86, 0.50, 0.74 };

        private static int Profile(int seat)
        {
            if (seat < 0 || seat >= 4) throw new ArgumentOutOfRangeException(nameof(seat));
            return seat;
        }

        private static void ValidateSample(double sample)
        {
            if (double.IsNaN(sample) || sample < 0 || sample >= 1)
                throw new ArgumentOutOfRangeException(nameof(sample));
        }

        public static int OfferAmount(int seat, int available, double sample)
        {
            ValidateSample(sample);
            int tier = PreferredTier[Profile(seat)] + (sample < 0.18 ? -1 : sample > 0.82 ? 1 : 0);
            tier = Math.Max(0, Math.Min(Amounts.Length - 1, tier));
            while (tier >= 0 && Amounts[tier] > available) tier--;
            return tier < 0 ? 0 : Amounts[tier];
        }

        public static bool Accept(int seat, int available, int amount, double sample)
        {
            ValidateSample(sample);
            double willingness = Willingness[Profile(seat)];
            if (amount <= 0 || amount > available || Array.IndexOf(Amounts, amount) < 0) return false;
            double exposure = amount / (double)Math.Max(1, available);
            double chance = willingness * (1 - 0.75 * exposure);
            return sample < chance;
        }

        public static float ResponseDelay(int seat, double sample)
        {
            ValidateSample(sample);
            return (float)(0.55 + (3 - Profile(seat)) * 0.14 + sample * 1.6);
        }

        public static bool Pass(int seat, int available, int stake, double sample)
        {
            ValidateSample(sample);
            double chance = 0.04 + (1 - Willingness[Profile(seat)]) * 0.3;
            return stake <= 0 || available < stake || sample < chance;
        }

        public static bool DoubleUp(int seat, int available, int stake, double sample)
        {
            ValidateSample(sample);
            double chance = (Willingness[Profile(seat)] - 0.4) * 0.7;
            return stake > 0 && available >= (long)stake * 4 && sample < chance;
        }
    }
}
