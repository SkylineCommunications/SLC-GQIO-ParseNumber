using Skyline.DataMiner.Analytics.GenericInterface;

/// <summary>
/// Represents a type specific GQI operator that can parse a string column to a target type.
/// </summary>
internal interface IParser : IGQIColumnOperator, IGQIRowOperator
{
}

/// <summary>
/// Common logic for all type specific parsers.
/// </summary>
/// <typeparam name="TTarget">Target type to which the text values will be parsed.</typeparam>
internal abstract class Parser<TTarget> : IParser
{
    protected Parser(GQIColumn<string> textColumn, GQIColumn<TTarget> targetColumn)
    {
        TextColumn = textColumn;
        TargetColumn = targetColumn;
    }

    public GQIColumn<string> TextColumn { get; }

    public GQIColumn<TTarget> TargetColumn { get; }

    /// <summary>
    /// Replaces the text column with a column of the target type.
    /// </summary>
    /// <param name="header">The collection of columns that can be modified.</param>
    public void HandleColumns(GQIEditableHeader header)
    {
        header.DeleteColumns(TextColumn);
        header.AddColumns(TargetColumn);
    }

    /// <summary>
    /// Attempts to parse the value from the <see cref="TextColumn"/> and put the result in the <see cref="TargetColumn"/> for this <paramref name="row"/>.
    /// </summary>
    /// <param name="row">The row containing the text value that should be parsed.</param>
    public void HandleRow(GQIEditableRow row)
    {
        if (!row.TryGetValue(TextColumn, out var textValue))
            return;
        var targetValue = Parse(textValue);
        row.SetValue(TargetColumn, targetValue);
    }

    /// <summary>
    /// Determines the name of the <see cref="TargetColumn"/> based on the <see cref="TextColumn"/>.
    /// Note that currently, these names may not be equal.
    /// </summary>
    /// <param name="textColumnName">The existing name of the text column.</param>
    /// <returns>The new name for the target column.</returns>
    protected static string GetTargetColumnName(string textColumnName) => $"{textColumnName} (parsed)";

    /// <summary>
    /// Parses the text value to the specific target type.
    /// </summary>
    /// <param name="textValue">The text value to parse.</param>
    /// <returns>The type specific parsed value.</returns>
    protected abstract TTarget Parse(string textValue);
}

/// <summary>
/// Parser to parse a text column to a column of type <see cref="int"/>.
/// </summary>
internal sealed class IntParser : Parser<int>
{
    public IntParser(GQIColumn<string> textColumn) : base(textColumn, new GQIIntColumn(GetTargetColumnName(textColumn.Name)))
    {
    }

    protected override int Parse(string textValue) => int.Parse(textValue);
}

/// <summary>
/// Parser to parse a text column to a column of type <see cref="double"/>.
/// </summary>
internal sealed class DoubleParser : Parser<double>
{
    public DoubleParser(GQIColumn<string> textColumn) : base(textColumn, new GQIDoubleColumn(GetTargetColumnName(textColumn.Name)))
    {
    }

    protected override double Parse(string textValue) => double.Parse(textValue);
}
