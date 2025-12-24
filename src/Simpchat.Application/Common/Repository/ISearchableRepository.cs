using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simpchat.Application.Common.Repository
{
    public interface ISearchableRepository<T> where T : class
    {
        public Task<List<T>> SearchAsync(string term);
        public Task<(List<T> Items, int TotalCount)> SearchPaginatedAsync(string term, int page, int pageSize);
    }
}
