using System.Collections.Generic;
using System.Linq;

namespace Bookstore.Repositories
{
    public partial class BookCount_Repository
    {
        public partial IEnumerable<BookCount> Load(BookCountFilter parameter)
        {
            var q = _domRepository.Bookstore.Book.Query();

            if (parameter.Od != null)
                q = q.Where(book => book.PublishDate >= parameter.Od);

            if (parameter.Do != null)
                q = q.Where(book => book.PublishDate <= parameter.Do);

            int count = q.Count();

            return new[]
            {
                new BookCount
                {
                    Count = count
                }
            };
        }
    }
}
