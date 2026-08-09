namespace DigitSwapping
{
    class Program
    {
        public static string getLargestNumber(string num) {

            char[] digits = num.ToCharArray();

            int max = 0;

            while (true)
            {
                for (int i = 0; i < digits.Length - 1; i++)
                {
                    int current = int.Parse(digits[i] + " ");
                    int next = int.Parse(digits[i + 1] + " ");

                    if (current % 2 == 0 && next % 2 == 0)
                    {
                        if (current < next)
                        {
                            char temp = digits[i];
                            digits[i] = digits[i + 1];
                            digits[i + 1] = temp;
                        }

                    }
                    else if (current % 2 == 1 && next % 2 == 1)
                    {
                        if (current < next)
                        {
                            char temp = digits[i];
                            digits[i] = digits[i + 1];
                            digits[i + 1] = temp;
                        }
                    }

                }

                if (int.Parse(string.Join("", digits))  > max){
                    max = int.Parse(string.Join("", digits));
                }
                else
                {
                    break;
                }
            }

            
            return string.Join("", digits);


        }

        public static void Main(string[] args){
            getLargestNumber("7596801");
        }
    }
}