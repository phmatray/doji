// <copyright file="GlossaryUtilityViewModel.cs" company="GLPM">
// Copyright (c) GLPM. All rights reserved.
// </copyright>

namespace Doji.Pages.UtilityPages.GlossaryUtility
{
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using Microsoft.Toolkit.Uwp.Helpers;
    using Models;
    using WatchlistUtility;

    public class GlossaryUtilityViewModel : MyViewModelBase
    {
        public GlossaryUtilityViewModel()
        {
            this.Glossary = new ObservableCollection<GlossaryElement>();
            this.InitializeAsync();
        }

        public ObservableCollection<GlossaryElement> Glossary { get; }

        private async Task InitializeAsync()
        {
            this.IsLoading = true;

            this.Glossary.Clear();
            using (var jsonStream = await StreamHelper.GetPackagedFileStreamAsync("Pages/UtilityPages/GlossaryUtility/glossary.json"))
            {
                var jsonString = await jsonStream.ReadTextAsync();
                var parsed = GlossaryJson.FromJson(jsonString);

                foreach (var category in parsed.Categories)
                {
                    foreach (var element in category.Elements)
                    {
                        var item = new GlossaryElement
                        {
                            Category = category.Category,
                            Description = element.Description,
                            Title = element.Title
                        };
                        this.Glossary.Add(item);
                    }
                }
            }

            this.IsLoading = false;
        }
    }
}