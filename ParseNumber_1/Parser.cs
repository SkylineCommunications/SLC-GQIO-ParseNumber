using Skyline.DataMiner.Analytics.GenericInterface;

/// <summary>
/// Represents a parser for a specific type.
/// </summary>
/// <typeparam name="TTarget">Target type.</typeparam>
internal interface IParser<TTarget>
{
    /// <summary>
    /// Creates the target column for the parsed result.
    /// </summary>
    /// <param name="name">Name of the original text column.</param>
    /// <returns>The target column.</returns>
    GQIColumn<TTarget> CreateTargetColumn(string name);

    /// <summary>
    /// Parses the text value to the specific target type.
    /// </summary>
    /// <param name="textValue">The text value to parse.</param>
    /// <param name="targetValue">The type specific parsed value if parsing succeeded.</param>
    /// <returns>True when parsing succeeded.</returns>
    bool TryParse(string textValue, out TTarget targetValue);
}

/// <summary>
/// Parser to parse a text column to a column of type <see cref="int"/>.
/// </summary>
internal sealed class IntParser : IParser<int>
{
    public GQIColumn<int> CreateTargetColumn(string name) => new GQIIntColumn($"INT({name})");

    public bool TryParse(string textValue, out int targetValue) => int.TryParse(textValue, out targetValue);
}

/// <summary>
/// Parser to parse a text column to a column of type <see cref="double"/>.
/// </summary>
internal sealed class DoubleParser : IParser<double>
{
    public GQIColumn<double> CreateTargetColumn(string name) => new GQIDoubleColumn($"DOUBLE({name})");

    public bool TryParse(string textValue, out double targetValue) => double.TryParse(textValue, out targetValue);
}