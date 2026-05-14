namespace Box2dNetGen.Generators
{
    internal class GeneratorUtils(TypeMapper typeMapper)
    {
        /// <summary>
        /// Generates a list of parameters for a function.
        /// </summary>
        public string GenerateParameterList(List<ApiParameter> parameters, bool includeMarshalAttributes,
            bool delegateAsIntPtr, out bool containsDelegateParameters)
        {
            containsDelegateParameters = false;
            var csParameters = new List<string>();
            for (var i = 0; i < parameters.Count; i++)
            {
                var p = parameters[i];
                var nextParameterIdentifier = i < parameters.Count - 1 ? parameters[i + 1].Identifier : "";
                // naive but seems to work for box2d: when a pointer parameter is followed by a 'count' parameter, it's an array, else it's a ptr to a single item.
                var isArray = p.Type.EndsWith("*") &&
                              (p.Identifier.EndsWith("array", StringComparison.OrdinalIgnoreCase) ||
                               nextParameterIdentifier.Contains("count", StringComparison.OrdinalIgnoreCase) ||
                               nextParameterIdentifier.Contains("capacity", StringComparison.OrdinalIgnoreCase));
                var attribute = "";
                if (includeMarshalAttributes)
                    attribute = (p.Type == "bool") ? "[MarshalAs(UnmanagedType.U1)] " : "";
                csParameters.Add(
                    $"{attribute}{typeMapper.MapType(p.Type, !isArray, CodeDirection.ClrToNative, delegateAsIntPtr, out var isDelegate, out _)} {p.Identifier}");
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
                // naive but seems to work for box2d: when a pointer parameter is followed by a 'count' parameter, it's an array, else it's a ptr to a single item.
                var isArray = p.Type.EndsWith('*') &&
                              (p.Identifier.EndsWith("array", StringComparison.OrdinalIgnoreCase) ||
                               nextParameterIdentifier.Contains("count", StringComparison.OrdinalIgnoreCase) ||
                               nextParameterIdentifier.Contains("capacity", StringComparison.OrdinalIgnoreCase));
                typeMapper.MapType(p.Type, !isArray, CodeDirection.ClrToNative, false, out var isDelegate, out _);
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

        /// <summary>
        /// Generates a list of parameters for a function.
        /// </summary>
        public List<MainParameter> GenerateMappedParameterList(List<ApiParameter> parameters)
        {
            var csParameters = new List<MainParameter>();
            for (var i = 0; i < parameters.Count; i++)
            {
                List<MappedParameter> mappedParameters = [];
                List<AltParameter> altParameters = [];

                var p = parameters[i];
                var nextParameterIdentifier = i < parameters.Count - 1 ? parameters[i + 1].Identifier : "";
                // naive but seems to work for box2d: when a pointer parameter is followed by a 'count' parameter, it's an array, else it's a ptr to a single item.
                var isArray = p.Type.EndsWith('*') &&
                              (p.Identifier.EndsWith("array", StringComparison.OrdinalIgnoreCase) ||
                               nextParameterIdentifier.Contains("count", StringComparison.OrdinalIgnoreCase) ||
                               nextParameterIdentifier.Contains("capacity", StringComparison.OrdinalIgnoreCase));

                var mainParam = typeMapper.MapParameter(p, !isArray);
                mappedParameters.Add(mainParam);

                if (isArray)
                {
                    i++; // skip the 'count' parameter - combine it into this one

                    var p2 = parameters[i];
                    var capacityParam = typeMapper.MapParameter(p2, !isArray);
                    mappedParameters.Add(capacityParam);
                }

                if (mainParam.HasAlternativeForm)
                {
                    altParameters.AddRange(typeMapper.MapAlternativeTypes(p, isArray ? parameters[i] : null));
                }

                csParameters.Add(new MainParameter(mappedParameters, altParameters));
            }

            return csParameters;
        }
    }
}