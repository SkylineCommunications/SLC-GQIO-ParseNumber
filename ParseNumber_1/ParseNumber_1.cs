using Skyline.DataMiner.Analytics.GenericInterface;
using System;

[GQIMetaData(Name = "Parse number")]
public class ParseNumberOperator : IGQIColumnOperator, IGQIRowOperator, IGQIInputArguments
{
    // Arguments
    private readonly GQIColumnDropdownArgument _columnArg;
    private readonly GQIStringDropdownArgument _typeArg;

    // Type specific parser
    private IParseOperator _typedParseOperator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParseNumberOperator"/> class.
    /// Initializes the input arguments.
    /// Called by GQI and should be parameterless.
    /// </summary>
    public ParseNumberOperator()
    {
        // Initialize an argument to select a string column
        _columnArg = new GQIColumnDropdownArgument("Text column")
        {
            IsRequired = true,
            Types = new[] { GQIColumnType.String },
        };

        // Initialize an argument to select the target column type
        var typeOptions = new[]
        {
            GQIColumnType.Double.ToString(),
            GQIColumnType.Int.ToString(),
        };
        _typeArg = new GQIStringDropdownArgument("Target type", typeOptions)
        {
            IsRequired = true,
            DefaultValue = typeOptions[0],
        };
    }

    /// <summary>
    /// Called by GQI to define the input arguments.
    /// Defines 2 arguments:
    /// <list type="number">
    /// <item>to select the string column that contains the value to parse.</item>
    /// <item>to select the target type to which the value should be parsed.</item>
    /// </list>
    /// </summary>
    /// <returns>The defined arguments.</returns>
    public GQIArgument[] GetInputArguments()
    {
        return new GQIArgument[]
        {
            _columnArg,
            _typeArg,
        };
    }

    /// <summary>
    /// Called by GQI to expose the chosen argument values.
    /// Validates the provided arguments values and initializes the type specific <see cref="_typedParseOperator"/>.
    /// </summary>
    /// <param name="args">Collection of chosen argument values.</param>
    /// <returns>Unused.</returns>
    public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
    {
        // Retrieve, validate chosen text column
        var selectedColumn = args.GetArgumentValue(_columnArg);
        if (!(selectedColumn is GQIColumn<string> textColumn))
            throw new Exception("Selected column is not a text column.");

        // Retrieve, validate chosen target type
        var selectedTargetType = args.GetArgumentValue(_typeArg);
        if (!Enum.TryParse(selectedTargetType, out GQIColumnType targetType))
            throw new Exception("Selected target type is invalid.");

        _typedParseOperator = CreateTypedParseOperator(textColumn, targetType);

        return default;
    }

    /// <summary>
    /// Called by GQI to determine the resulting columns.
    /// Lets the type specific <see cref="_typedParseOperator"/> replace the text column with a column of the target type.
    /// </summary>
    /// <param name="header">The current collection of columns that should be modified.</param>
    public void HandleColumns(GQIEditableHeader header)
    {
        _typedParseOperator.HandleColumns(header);
    }

    /// <summary>
    /// Called by GQI to handle each <paramref name="row"/> in turn.
    /// Lets the type specific <see cref="_typedParseOperator"/> parse the value in the text column and store the parsed value back into the <paramref name="row"/>.
    /// </summary>
    /// <param name="row">The next row that needs to be handled.</param>
    public void HandleRow(GQIEditableRow row)
    {
        _typedParseOperator.HandleRow(row);
    }

    /// <summary>
    /// Creates the type specific parser that can parse a string value to the target type.
    /// </summary>
    /// <param name="textColumn">The text column that will contain the values to parse.</param>
    /// <param name="targetType">The type to which the text values should be parsed.</param>
    /// <returns>The type specific parser logic.</returns>
    /// <exception cref="NotImplementedException">If the <paramref name="targetType"/> is not supported.</exception>
    private IParseOperator CreateTypedParseOperator(GQIColumn<string> textColumn, GQIColumnType targetType)
    {
        switch (targetType)
        {
            case GQIColumnType.Double:
                var doubleParser = new DoubleParser();
                return new TypedParseOperator<double>(doubleParser, textColumn);
            case GQIColumnType.Int:
                var intParser = new IntParser();
                return new TypedParseOperator<int>(intParser, textColumn);
            default:
                throw new NotImplementedException("Target type is not supported.");
        }
    }
}