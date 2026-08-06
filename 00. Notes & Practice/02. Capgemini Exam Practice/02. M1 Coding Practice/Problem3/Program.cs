using Problem3;
using System.Diagnostics.Metrics;

namespace Problem3
{
    public interface IBrodbandPlan
    {
        int GetBrodbandPlanAmount();
        string Type();
    }

    public class Black : IBrodbandPlan
    {
        private bool _isSubscriptionValid;
        private int _discountPercentage;
        private const int Planamount = 3000;

        public Black(bool isSubscriptionValid, int discountPercentage)
        {
            if (discountPercentage < 0 || discountPercentage > 50)
            {
                throw new ArgumentOutOfRangeException(nameof(discountPercentage));
            }
            else
            {
                _discountPercentage = discountPercentage;
            }
            _isSubscriptionValid = isSubscriptionValid;
        }

        public int GetBrodbandPlanAmount()
        {
            if (this._isSubscriptionValid)
            {
                return (int) (Planamount * (1 - (double)this._discountPercentage / 100.0));
            }
            else
            {
                return Planamount;
            }
        }

        public string Type()
        {
            return "Black";
        }


    }

    public class Gold : IBrodbandPlan
    {
        private bool _isSubscriptionValid;
        private int _discountPercentage;
        private const int Planamount = 1500;

        public Gold(bool isSubscriptionValid, int discountPercentage)
        {

            if (discountPercentage < 0 || discountPercentage > 30)
            {
                throw new ArgumentOutOfRangeException(nameof(discountPercentage));
            }
            else
            {
                _discountPercentage = discountPercentage;
            }
            _isSubscriptionValid = isSubscriptionValid;
        }

        public int GetBrodbandPlanAmount()
        {
            if (this._isSubscriptionValid)
            {
                return (int)(Planamount * (1 - (double)this._discountPercentage / 100.0));
            }
            else
            {
                return Planamount;
            }
        }

        public string Type()
        {
            return "Gold";
        }
    }

    class SubscriberPlan
    {
        private IList<IBrodbandPlan> _brodbandPlans;

        public SubscriberPlan(IList<IBrodbandPlan> brodbandPlans)
        {
            if (brodbandPlans == null)
            {
                throw new ArgumentNullException(nameof(brodbandPlans));
            }
            else
            {
                _brodbandPlans = brodbandPlans;
            }
        }

        public IList<Tuple<string, int>> GetSubscriptionPlan()
        {
            IList<Tuple<string, int>> tuples = new List<Tuple<string, int>>();

            foreach (IBrodbandPlan plan in this._brodbandPlans)
            {
                tuples.Add(Tuple.Create(plan.Type(), plan.GetBrodbandPlanAmount()));
            }

            return tuples;
        }
    }
    class Program
    {
        public static void Main(string[] args)
        {
            var plans = new List<IBrodbandPlan>
            {
                new Black(true,50),
                new Black(false,10),
                new Gold(true,30),
                new Black(true,20),
                new Gold(false,20)
            };

            SubscriberPlan subscriberPlan = new SubscriberPlan(plans);

            foreach(var item in subscriberPlan.GetSubscriptionPlan())
            {
                Console.WriteLine($"{ item.Item1} : {item.Item2}");
            }

        }
       
    }
}