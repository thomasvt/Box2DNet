namespace Box2dNetGen.Generators
{
    internal class TypeMapper(
        Dictionary<string, string> structTypeReplacer,
        Dictionary<string, ApiStruct> structs,
        Dictionary<string, ApiEnum> enums,
        Dictionary<string, ApiDelegate> delegates,
        HashSet<string> excludedTypes)
    {
        /// <param name="isNotArray">Only relevant when the type is a pointer. Else ignored.</param>
        public string MapType(in string cType, bool isNotArray, CodeDirection codeDirection, bool returnDelegateAsIntPtr, out bool isDelegate)
        {
            isDelegate = false;
            var type = RemoveSpaces(cType);
            var isConst = false;
            if (type.StartsWith("const"))
            {
                isConst = true;
                type = type[5..];
            }
            var isPointer = type.EndsWith("*");
            if (isPointer) type = type.TrimEnd('*');

            // check intrinsic types:
            if (isPointer)
            {
                if (type == "char") return "string";
                if (type == "void") return "IntPtr /* void* */";
                if (isNotArray)
                {
                    if (type == "uint8_t") return "ref byte";
                    if (type == "uint16_t") return "ref ushort";
                    if (type == "uint32_t") return "ref uint";
                    if (type == "uint64_t") return "ref ulong";
                    if (type == "float") return "ref float";
                }
                else
                {
                    if (type == "uint8_t") return "IntPtr /* uint8_t* */";
                    if (type == "uint64_t") return "IntPtr /* uint64_t* */";
                }
            }
            else
            {
                if (type == "bool") return "bool";
                if (type == "float") return "float";
                if (type == "int") return "int";
                if (type == "uint8_t") return "byte";
                if (type == "int16_t") return "short";
                if (type == "uint16_t") return "ushort";
                if (type == "int32_t") return "int";
                if (type == "uint32_t") return "uint";
                if (type == "int64_t") return "long";
                if (type == "uint64_t") return "ulong";
                if (type == "unsignedint") return "uint";
                if (type == "void") return "void";
            }

            // type is a enum?
            if (enums.TryGetValue(type, out var apiEnum))
            {
                if (isPointer) throw new Exception($"Used type seems to be enum '{apiEnum.Identifier}' but it's a pointer, which is suspicious in C.");
                return apiEnum.Identifier;
            }

            // type is a delegate?
            if (delegates.TryGetValue(type, out var apiDelegate))
            {
                if (!isPointer) throw new Exception($"Used type seems to be delegate '{apiDelegate.Identifier}' but it's not a pointer, which is invalid C.");
                isDelegate = true;
                if (returnDelegateAsIntPtr)
                    return $"IntPtr";
                return type;
            }

            // type is a struct?
            var isReplacedByDotNetStruct = false;
            if (structTypeReplacer.TryGetValue(type, out var dotNetStruct))
            {
                type = dotNetStruct;
                isReplacedByDotNetStruct = true;
            }

            var typeIsUserStruct = structs.ContainsKey(type); // it's a struct defined by Box2D code
            if (typeIsUserStruct || isReplacedByDotNetStruct)
            {
                if (isPointer)
                {
                    if (isNotArray)
                    {
                        if (isConst) return "in " + type;
                        return "ref " + type;
                    }

                    if (codeDirection == CodeDirection.NativeToClr)
                        return "IntPtr"; // 'returning' arrays won't allocate .NET arrays. We have to accept the array as an IntPtr and loop over it. See helper method NativeArrayAsSpan in Box2dNet.

                    return $"{type}[]";
                }

                return type;
            }

            if (excludedTypes.Contains(type))
            {
                throw new NoGenException($"Type '{cType}' is in the 'Excluded' list.");
            }
            throw new NoGenException($"No known mapping for type '{cType}'.");
        }

        private static string RemoveSpaces(string src)
        {
            return new string(src.Where(ch => ch != ' ').ToArray());
        }
    }
}
