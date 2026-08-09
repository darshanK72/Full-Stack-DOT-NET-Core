namespace Problem9
{
    class Book
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public int BookId { get; set; }
        public int NumberOfCopies { get; set; }

    }

    class Library
    {
        public IList<Book> Books = null;

        public void AddBook(Book book)
        {
            if (this.Books == null)
            {
                this.Books = new List<Book>();

                this.Books.Add(book);
            }
            else
            {
                List<Book> lst = new List<Book>(Books);

                Book? bk = lst.Find((b) => b.BookId == book.BookId);
                if (bk != null)
                {
                    throw new InvalidOperationException("Book Already Exists");
                }
                else
                {
                    this.Books.Add(book);
                }
            }
        }

        public bool IsBookIssued(int bookId)
        {
            if(this.Books == null)
            {
                return false;
            }
            else
            {
                List<Book> lst = new List<Book>(Books);

                Book? bk = lst.Find((b) => b.BookId == bookId);

                if (bk == null)
                {
                    throw new KeyNotFoundException("Book is not available");
                }
                else
                {
                    if (bk.NumberOfCopies >= 1)
                    {
                        return true;
                    }
                    return false;
                }
            }
            
        }
    }
    class Program
    {
        public static void Main(string[] args)
        {
            var book = new Book();
            book.Name = "Physics";
            book.Author = "S C Verma";
            book.BookId = 1;
            book.NumberOfCopies = 5;

            var library = new Library();

            try
            {

                library.AddBook(book);

                Console.WriteLine(library.IsBookIssued(1));
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
    }
}