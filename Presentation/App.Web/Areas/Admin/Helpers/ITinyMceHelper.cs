using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Helpers
{
    public partial interface ITinyMceHelper
    {
        Task<string> GetTinyMceLanguageAsync();
    }
}