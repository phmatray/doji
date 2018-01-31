namespace Doji.Models
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class GlossaryItemsGroup : IGrouping<string, GlossaryItem>
    {
        private List<GlossaryItem> _glossaryElementsGroup;

        public GlossaryItemsGroup(string key, IEnumerable<GlossaryItem> items)
        {
            this._glossaryElementsGroup = new List<GlossaryItem>(items);
            this.Key = key;
        }

        public string Key { get; }

        public IEnumerator<GlossaryItem> GetEnumerator()
            => this._glossaryElementsGroup.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => this.GetEnumerator();
    }
}