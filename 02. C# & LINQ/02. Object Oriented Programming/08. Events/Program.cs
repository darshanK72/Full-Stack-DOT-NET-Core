using MyClassLibrary;

namespace Events;

class CreditCard
{
    public delegate void MyDelegate();

    public event MyDelegate? myEvent;
    public int CreditCardNo { get; set; }
    public string CardHolderName { get; set; } = string.Empty;
    public double BalanceAmount { get; set; }
    public double CreditLimit { get; set; }

    public double GetCreditLimit()
    {
        return CreditLimit;
    }

    public double GetBalance()
    {
        return BalanceAmount;
    }

    public void MakePayment(double amount)
    {
        if (BalanceAmount < amount || BalanceAmount - amount < CreditLimit)
        {
            myEvent?.Invoke();
        }
        else
        {
            Console.WriteLine("Payment Done!!");
        }
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        CreditCard creditCard = new CreditCard
        {
            CreditCardNo = 101,
            CardHolderName = "Darshan Khairnar",
            BalanceAmount = 5000,
            CreditLimit = 1000
        };

        creditCard.myEvent += MyEventHandler;
        creditCard.MakePayment(5000);
    }

    public static void MyEventHandler()
    {
        Class1 class1 = new Class1();
        class1.Hello();

        Console.WriteLine("Can't Complete Payment");
    }
}
