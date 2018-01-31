using Doji.Models;

namespace Doji.Pages.UtilityPages.GlossaryUtility
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Services;
    using ViewModels;

    public class GlossaryUtilityViewModel : MyViewModelBase
    {
        private readonly IDataService _dataService;

        public GlossaryUtilityViewModel(IDataService dataService)
        {
            this._dataService = dataService;
            this.InitializeAsync();
        }

        private IEnumerable<GlossaryItemsGroup> _groupedGlossaryItems;
        public IEnumerable<GlossaryItemsGroup> GroupedGlossaryItems
        {
            get => _groupedGlossaryItems;
            set => Set(ref _groupedGlossaryItems, value);
        }

        private async Task InitializeAsync()
        {
            this.IsLoading = true;

            var glossaryItems = await this._dataService.GetGlossaryAsync();
            this.GroupedGlossaryItems = glossaryItems
                .OrderBy(x => x.FirstLetter)
                .GroupBy(x => x.FirstLetter, (key, list) => new GlossaryItemsGroup(key, list));

            this.IsLoading = false;
        }

    }
}