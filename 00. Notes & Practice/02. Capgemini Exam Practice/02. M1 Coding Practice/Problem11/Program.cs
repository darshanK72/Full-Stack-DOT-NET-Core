using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Problem11
{
    class ScreenSeat
    {
        public string SeatNumber { get; set; }
        public bool IsBooked { get; set; }

    }

    class Booking
    {
        private IList<ScreenSeat> _seats;

        public Booking(IList<ScreenSeat> _seats)
        {
            foreach(ScreenSeat seat in _seats)
            {
               
                int count = 0;
                foreach(ScreenSeat seat1 in _seats)
                {
                    if(seat1.SeatNumber == seat.SeatNumber)
                    {
                        count++;
                    }
                }
                if(count > 1)
                {
                    throw new InvalidOperationException("Contains Duplicate Elements");
                }
            }
            this._seats = _seats;
        }

        public bool BookSeat(int ticketQuantity, string[] seatNumbers)
        {
            if(ticketQuantity != seatNumbers.Length)
            {
                throw new InvalidOperationException("Tickit Quantity Do not Match");
            }

            var newList = _seats.Where(seat =>seatNumbers.Contains(seat.SeatNumber));

            foreach(var seat in newList)
            {
                if (seat.IsBooked)
                {
                    return false;
                }
            }

            foreach(var s in newList)
            {
                foreach (ScreenSeat seat in _seats)
                {
                    if(s.SeatNumber == seat.SeatNumber)
                    {
                        seat.IsBooked = true;
                    }
                }

            }

            return true;
          
        }

        public bool CancleSeat(int ticketQuantity, string[] seatNumbers)
        {
            if (ticketQuantity != seatNumbers.Length)
            {
                throw new InvalidOperationException("Tickit Quantity Do not Match");
            }
            foreach (string num in seatNumbers)
            {
                foreach (ScreenSeat seat in _seats)
                {
                    if (seat.SeatNumber == num)
                    {
                        if (!seat.IsBooked)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }

    class Program
    {
        public static void Main(string[] args)
        {
            var screenSeats = new List<ScreenSeat>()
            {
                new ScreenSeat(){SeatNumber = "1", IsBooked = true},
                new ScreenSeat(){SeatNumber = "2", IsBooked = false},
                new ScreenSeat(){SeatNumber = "3", IsBooked = false},
                new ScreenSeat(){SeatNumber = "4", IsBooked = false},
                new ScreenSeat(){SeatNumber = "5", IsBooked = false},
                new ScreenSeat(){SeatNumber = "6", IsBooked = false},
                new ScreenSeat(){SeatNumber = "7", IsBooked = false},
                new ScreenSeat(){SeatNumber = "8", IsBooked = false},
                new ScreenSeat(){SeatNumber = "9", IsBooked = false}
            };

            

            try
            {
                var booking = new Booking(screenSeats);
                Console.WriteLine(booking.BookSeat(2, new[] {"2","3"}));
                Console.WriteLine(booking.CancleSeat(2, new[] { "2", "3" }));
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


        }
    }
}