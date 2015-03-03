﻿// /*
//   SharpNative - C# to D Transpiler
//   (C) 2014 Irio Systems 
// */

#region Imports

using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

#endregion

namespace SharpNative.Compiler
{
    internal static class WriteIdentifierName
    {
        public static void Go(OutputWriter writer, IdentifierNameSyntax identifier, bool byRef = false)
        {
            var symbolInfo = TypeProcessor.GetSymbolInfo(identifier);
            var symbol = symbolInfo.Symbol;

            if (symbol == null)
            {
                //Yield issue ...
                symbol = symbolInfo.CandidateSymbols[0];
            }

            if (symbol.IsStatic)
            {
                //                writer.Write(symbol.ContainingNamespace.FullNameWithDot());
                //                writer.Write(symbol.ContainingType.FullName());
                writer.Write(TypeProcessor.ConvertType(symbol.ContainingType)+".");
            }

          
            var binExpression = identifier.Parent as BinaryExpressionSyntax;
            string memberName = TransformIdentifier(identifier.Identifier.ToString());

            if (symbol.Kind == SymbolKind.Property) // Using dlang properties
            {
                if (symbol.ContainingType.TypeKind == TypeKind.Interface ||
                    Equals(symbol.ContainingType.FindImplementationForInterfaceMember(symbol), symbol))
                {
                    memberName =
                        Regex.Replace(
                            TypeProcessor.ConvertType(symbol.ContainingType.OriginalDefinition)
                                .RemoveFromStartOfString(symbol.ContainingNamespace + ".Namespace.") + "_" + memberName,
                            @" ?!\(.*?\)", string.Empty);
                }

                var interfaceMethods =
                    symbol.ContainingType.AllInterfaces.SelectMany(
                        u =>
                            u.GetMembers(memberName)).ToArray();

                ISymbol interfaceMethod =
                    interfaceMethods.FirstOrDefault(
                        o => symbol.ContainingType.FindImplementationForInterfaceMember(o) == symbol);

             

                if (interfaceMethod != null)
                {
                    if (symbol.ContainingType.SpecialType == SpecialType.System_Array)
                        writer.Write("");
                    else
                    {
                        var typenameI =
                            Regex.Replace(
                                TypeProcessor.ConvertType(interfaceMethod.ContainingType.ConstructedFrom, true),
                                @" ?!\(.*?\)", string.Empty);
                            //TODO: we should be able to get the original interface name, or just remove all generics from this
                        if (typenameI.Contains('.'))
                            typenameI = typenameI.SubstringAfterLast('.');
                        writer.Write(typenameI + "_");
                    }
                }
           }
                writer.Write(memberName);

        }

        public static string TransformIdentifier(string ident, ITypeSymbol type =null)
        {
            ident = ident.Trim();//Cant have spaces in identifiers
            var name = ident;
            //limited support for badly named identifiers
            if (ident.StartsWith("__cs"))
            {
                name =  "__" + ident;
            }

            if (type != null)
            {
                //Special Classes
                if (type.SpecialType == SpecialType.System_Object)
                {
                    name =  "NObject";
                    return name;
                }

                if (type == Context.Exception)
                {
                    name = "NException"; //TODO: this is not right, we can have object defined in another namespace ?
                    return name;

                }

            }


            switch (name)
            {
                case "version":
                case "body":
                case "auto":
                case "typeof":
                case "immutable":
                case "ubyte":
                case "lazy":
                case "scope":
                case "shared":
                case "Error":
                case "toString":
                case "opEquals":
                case "opCmp":
                case "opCall":
                case "toHash":
                case "final":
                case "synchronized":
                case "assert":
                case "enforce":
                case "typeid":
                case "alias":
                case "inout":
                case "_gshared":
                case "__TypeOf":
                case "__Delegate":
                case "__Event":
                case "BOX":
                case "UNBOX":
                    name= "__cs" + ident;
                    break;
                case "Object": // Dlang does not accept defining Object / Exception in any module except object.di
                case "Exception": // Dlang does not accept defining Object / Exception in any module except object.di
                    name = "__CS" + ident;
                    break;
                default:
                    name = ident;
                    break;
            }

            if (type != null)
            {
                var tnamed = type as INamedTypeSymbol;
                if (tnamed != null && tnamed.IsGenericType)
                {
                    name += "__G";
                }
            }


            return name;
        }
    }
}