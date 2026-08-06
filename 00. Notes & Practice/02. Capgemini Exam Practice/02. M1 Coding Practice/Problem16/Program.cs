namespace Problem16
{
    class PhoneCall
    {
        public delegate void Notify();

        public event Notify PhoneCallEvent;

        private string message;

        public string Message
        {
            get
            {
                return this.message;
            }
        }



        void OnSubscribe()
        {
            this.message = "Subscribed to calls";
        }

        void OnUnsubscribe()
        {
            this.message = "Unsubscribed to calls";
        }

        public void makeAPhoneCall(bool notify)
        {
            if (notify)
            {
                this.PhoneCallEvent += new Notify(OnSubscribe);
            }
            else
            {
                this.PhoneCallEvent += new Notify(OnUnsubscribe);
            }

            PhoneCallEvent();
        }
    }
    class Program
    {
        public static void Main(string[] args)
        {
            PhoneCall call = new PhoneCall();
            call.makeAPhoneCall(true);
            Console.WriteLine($"{call.Message}");
            call.makeAPhoneCall(false);
            Console.WriteLine($"{call.Message}");


        }
    }
}