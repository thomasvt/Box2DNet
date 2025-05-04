namespace Box2dNetGen
{
    internal record ApiConstant(string Identifier, string Value, string Type, List<string> Comment);
    internal record ApiParameter(string Identifier, string Type);
    internal record ApiFunction(string Identifier, string ReturnType, List<ApiParameter> Parameters, List<string> Comment);
    internal record ApiDelegate(string Identifier, string ReturnType, List<ApiParameter> Parameters, List<string> Comment);

    internal class ApiStructField
    {
        public ApiStructField(string identifier, string type, bool isFixedArray, string arrayLength, List<string> comment)
        {
            Identifier = identifier;
            Type = type;
            IsFixedArray = isFixedArray;
            ArrayLength = arrayLength;
            Comment = comment;
        }

        public string Identifier { get; set; }
        public string Type { get; set; }
        public bool IsFixedArray { get; set; }
        public string ArrayLength { get; }
        public List<string> Comment { get; set; }
    }

    internal record ApiStruct(string Identifier, List<ApiStructField> Fields, List<string> Comment);

    internal record ApiEnumField(string Identifier, string? Value, List<string> Comment);
    internal record ApiEnum(string Identifier, List<ApiEnumField> Fields);
}
