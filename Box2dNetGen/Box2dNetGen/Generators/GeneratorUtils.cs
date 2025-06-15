namespace Box2dNetGen.Generators
{
    internal class GeneratorUtils(TypeMapper typeMapper)
    {
        /// <summary>
        /// Generates a list of parameters for a function.
        /// </summary>
        public string GenerateParameterList(List<ApiParameter> parameters, bool includeMarshalAttributes, bool delegateAsIntPtr, out bool containsDelegateParameters)
        {
            containsDelegateParameters = false;
            var csParameters = new List<string>();
            for (var i = 0; i < parameters.Count; i++)
            {
                var p = parameters[i];
                var nextParameterIdentifier = i < parameters.Count - 1 ? parameters[i + 1].Identifier : "";
                var isArray = p.Type.EndsWith("*") &&
                              (p.Identifier.ToLower().EndsWith("array") || nextParameterIdentifier.ToLower().Contains("count") || nextParameterIdentifier.ToLower().Contains("capacity")); // naive but seems to work for box2d: when a pointer parameter is followed by a 'count' parameter, it's an array, else it's a ptr to a single item.
                var attribute = "";
                if (includeMarshalAttributes)
                    attribute = (p.Type == "bool") ? "[MarshalAs(UnmanagedType.U1)] " : "";
                csParameters.Add($"{attribute}{typeMapper.MapType(p.Type, !isArray, CodeDirection.ClrToNative, delegateAsIntPtr, out var isDelegate)} {p.Identifier}");
                if (isDelegate)
                    containsDelegateParameters = true;
            }
            return string.Join(", ", csParameters);
        }

        /// <summary>
        /// Generates a list of arguments for a function call using the parameter names as arguments.
        /// </summary>
        public string GenerateArgumentList(List<ApiParameter> parameters)
        {
            var csArguments = new List<string>();
            for (var i = 0; i < parameters.Count; i++)
            {
                var p = parameters[i];
                var nextParameterIdentifier = i < parameters.Count - 1 ? parameters[i + 1].Identifier : "";
                var isArray = p.Type.EndsWith("*") &&
                              (p.Identifier.ToLower().EndsWith("array") || nextParameterIdentifier.ToLower().Contains("count") || nextParameterIdentifier.ToLower().Contains("capacity")); // naive but seems to work for box2d: when a pointer parameter is followed by a 'count' parameter, it's an array, else it's a ptr to a single item.
                typeMapper.MapType(p.Type, !isArray, CodeDirection.ClrToNative, false, out var isDelegate);
                if (isDelegate)
                {
                    csArguments.Add($"Marshal.GetFunctionPointerForDelegate({p.Identifier})");
                }
                else
                {
                    csArguments.Add($"{p.Identifier}");
                }
            }
            return string.Join(", ", csArguments);
        }
    }
}
