using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EntityFrameworkWithAzureActiveDirectoryAuth.Pages
{
    public class IndexModel : PageModel
    {
        private readonly Query query;

        public IndexModel(Query query)
        {
            this.query = query;
        }

        public int UserCount { get; private set; }

        public async Task OnGet()
        {
            UserCount = await query.GetRecordCount().ConfigureAwait(false);
        }
    }
}
