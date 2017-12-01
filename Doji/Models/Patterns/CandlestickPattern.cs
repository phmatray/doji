namespace Doji.Models.Patterns
{
    public interface ICandlestickPattern
    {
        string Name { get; }
        string Description { get; }
    }

    public class ThreeLineStrike
        : ICandlestickPattern
    {
        #region Implementation of ICandlestickPattern

        public string Name => "Three Line Strike";
        public string Description { get; set; }

        #endregion
    }
}