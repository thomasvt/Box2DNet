namespace Box2dNetGen;

internal record MappedParameter(
    string Type,
    string Identifier,
    bool IsUnsafe,
    bool HasAlternativeForm = false,
    string? Attribute = "")
{
    public override string ToString() => $"{Attribute}{Type} {Identifier}";
}

internal readonly record struct MainParameter(
    List<MappedParameter> SubParameters,
    List<AltParameter>? AltParameters = null)
{
    public override string ToString() => SubParameters.ToCsv();
}

internal record AltParameter(List<ParamAndArg> ParamAndArgs)
{
    public bool IsAlsoExtern => ParamAndArgs.All(x => x.ArgConversionFormat is null);

    public string ToArgString() => ParamAndArgs.Select(x => x.ToArgString()).ToCsv();
}

internal record ParamAndArg(
    MappedParameter Parameter,
    List<ApiParameter> RepresentsApiParameters,
    string? ArgConversionFormat = null,
    CallWrap? CallWrap = null)
{
    public string ToArgString() =>
        ArgConversionFormat is null ? Parameter.Identifier : string.Format(ArgConversionFormat, Parameter.Identifier);

    public string GetPreCallStr() =>
        CallWrap.HasValue
            ? string.Format(CallWrap.Value.PreCallFormat, Parameter.Identifier)
            : throw new InvalidOperationException();
}

internal readonly record struct CallWrap(string PreCallFormat, bool IncrementScope = true);