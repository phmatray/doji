namespace Doji.Models
{
    using System.Linq;
    using GalaSoft.MvvmLight;

    public class GlossaryItem : ObservableObject
    {
        public string Category { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }

        public string FirstLetter
            => this.Title.First().ToString();
    }
}