/*
 * PROBLEM: Member Tier Discounts
 *
 * A retail loyalty program applies tier percentages, coupon codes, and free-shipping
 * rules at checkout. MSTest exercises lifecycle hooks and data-driven rows.
 *
 * This exercise covers:
 *   ch03 — [TestClass], [TestMethod], Assert.AreEqual/IsTrue/ThrowsException
 *   ch03 — [TestInitialize] per-test setup
 *   ch03 — [DataTestMethod] + [DataRow]
 *   ch03 — [ClassInitialize] once-per-class hook
 */

using System;

namespace RetailLoyalty
{
    public enum MemberTier
    {
        Standard,
        Silver,
        Gold
    }

    /*
     * Loyalty pricing rules — pure logic, no I/O.
     */
    public sealed class LoyaltyDiscountService
    {
        /*
         * Standard 0%, Silver 5%, Gold 10% off orderTotal.
         * Throws ArgumentOutOfRangeException when orderTotal < 0.
         */
        public decimal ApplyTierDiscount(decimal orderTotal, MemberTier tier)
        {
            // TODO: apply tier percent
            throw new NotImplementedException();
        }

        /*
         * WELCOME20 = 20% off; FLAT10 = $10 off.
         * Unknown code → InvalidOperationException.
         * orderTotal must be >= 25 or throw InvalidOperationException.
         */
        public decimal ApplyCoupon(decimal orderTotal, string couponCode)
        {
            // TODO: validate minimum; apply coupon
            throw new NotImplementedException();
        }

        /*
         * Gold always eligible; Silver/Standard when orderTotal >= 75.
         */
        public bool IsEligibleForFreeShipping(decimal orderTotal, MemberTier tier)
        {
            // TODO: tier and threshold rules
            throw new NotImplementedException();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // TODO: demo tier on $120 Silver, WELCOME20 on $50, free ship $80 Standard
            throw new NotImplementedException();
        }
    }
}
