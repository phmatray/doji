namespace Doji.Models
{
    public class Exchange
    {
        public Exchange(string name, int nbrPairs)
        {
            this.Name = name;
            this.NbrPairs = nbrPairs;
        }

        public string Name { get; set; }
        public int NbrPairs { get; set; }
    }
}