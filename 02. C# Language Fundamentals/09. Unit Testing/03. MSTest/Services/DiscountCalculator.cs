using System;
using MSTest.Models;

namespace MSTest.Services;

/*
 * SECTION 2: DISCOUNT CALCULATOR — system under test (SUT)
 *
 * Small pricing service for an e-commerce checkout. Unit tests call these methods
 * with known inputs and verify outputs via Assert.* — they never run Program.Main.
 *
 * Methods exercised across MSTest.Tests/:
 *   ApplyPercentDiscount  — core math + validation (CoreTests, DataTests, AssertTests)
 *   ApplyTierDiscount     — enum switch + tier rates (DataTests, LifecycleTests)
 *   ApplyCoupon           — business rules + exceptions (AssertTests)
 *   IsEligibleForFreeShipping — boolean rules (AssertTests)
 */
public class DiscountCalculator
{
    private const decimal GoldTierRate = 0.15m;
    private const decimal SilverTierRate = 0.10m;
    private const decimal StandardTierRate = 0.05m;
    private const decimal WelcomeCouponRate = 0.20m;
    private const decimal MinimumOrderAmount = 10.00m;

    public decimal ApplyPercentDiscount(decimal orderTotal, decimal discountRate)
    {
        if (orderTotal < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(orderTotal), "Order total cannot be negative.");
        }

        if (discountRate < 0m || discountRate > 1m)
        {
            throw new ArgumentOutOfRangeException(nameof(discountRate), "Discount rate must be between 0 and 1.");
        }

        decimal discount = orderTotal * discountRate;
        return orderTotal - discount;
    }

    public decimal ApplyTierDiscount(decimal orderTotal, CustomerTier tier)
    {
        decimal rate = tier switch
        {
            CustomerTier.Gold => GoldTierRate,
            CustomerTier.Silver => SilverTierRate,
            CustomerTier.Standard => StandardTierRate,
            _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, "Unknown customer tier."),
        };

        return ApplyPercentDiscount(orderTotal, rate);
    }

    public decimal ApplyCoupon(decimal orderTotal, string couponCode)
    {
        if (orderTotal < MinimumOrderAmount)
        {
            throw new InvalidOperationException(
                $"Orders under {MinimumOrderAmount:C} are not eligible for coupon discounts.");
        }

        if (string.Equals(couponCode, "WELCOME20", StringComparison.OrdinalIgnoreCase))
        {
            return ApplyPercentDiscount(orderTotal, WelcomeCouponRate);
        }

        throw new ArgumentException($"Unknown coupon code: {couponCode}", nameof(couponCode));
    }

    public bool IsEligibleForFreeShipping(decimal orderTotal, CustomerTier tier)
    {
        if (orderTotal < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(orderTotal), "Order total cannot be negative.");
        }

        return orderTotal >= 75m || tier == CustomerTier.Gold;
    }
}
