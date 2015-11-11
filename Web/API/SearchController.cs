using Microsoft.AspNet.Mvc;
using Web.Services;
using Web.ViewModels;

namespace Web.API
{
    [Route("/api/search")]
    public class SearchController : Controller
    {
        private AzureSearchService _searchService;

        public SearchController(AzureSearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpPost]
        public IActionResult Search([FromBody]SearchCriteria searchCriteria)
        {
            var searchResult = _searchService.Search(
                searchCriteria.Query, searchCriteria.Username);

            return Json(searchResult);
        }
    }
}
