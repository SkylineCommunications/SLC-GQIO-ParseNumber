using Skyline.DataMiner.Analytics.GenericInterface;

/// <summary>
/// Represents a type specific GQI operator that can parse a string column to a target type.
/// </summary>
internal interface IParseOperator : IGQIColumnOperator, IGQIRowOperator
{
}

/// <summary>
/// Common logic for all type specific parsers.
/// </summary>
/// <typeparam name="TTarget">Target type to which the text values will be parsed.</typeparam>
internal class TypedParseOperator<TTarget> : IParseOperator
{
    private readonly IParser<TTarget> _parser;
    private readonly GQIColumn<string> _textColumn;
    private readonly GQIColumn<TTarget> _targetColumn;

    public TypedParseOperator(IParser<TTarget> parser, GQIColumn<string> textColumn)
    {
        _parser = parser;
        _textColumn = textColumn;
        _targetColumn = parser.CreateTargetColumn(textColumn.Name);
    }

    /// <summary>
    /// Replaces the text column with a column of the target type.
    /// </summary>
    /// <param name="header">The collection of columns that can be modified.</param>
    public void HandleColumns(GQIEditableHeader header)
    {
        header.DeleteColumns(_textColumn);
        header.AddColumns(_targetColumn);
    }

    /// <summary>
    /// Attempts to parse the value from the <see cref="_textColumn"/> and put the result in the <see cref="_targetColumn"/> for this <paramref name="row"/>.
    /// </summary>
    /// <param name="row">The row containing the text value that should be parsed.</param>
    public void HandleRow(GQIEditableRow row)
    {
        if (!row.TryGetValue(_textColumn, out var textValue))
            return;

        if (!_parser.TryParse(textValue, out var targetValue))
            return;

        row.SetValue(_targetColumn, targetValue);
    }
}