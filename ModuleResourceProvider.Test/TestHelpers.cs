using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleResourceProvider.Test;

public class TestHelpers
{
    public static string GetMeta(char indent, int indentStep, char delimiter, char quoteLiteral, char quoteExpansion) =>
        meta(str(indent), indentStep, str(delimiter), str(quoteLiteral), str(quoteExpansion));

    private static string str(char? c)
    {
        if (c == null || c == '\0')
            return "";
        else
            return c.Value.ToString();
    }

    private static string meta(string i, int s, string d, string q, string e) => $"""
		meta =
			QuoteMeta = '
			Indent= '{i}'
			IndentStep = {s}
			Delimiter = '{d}'
			QuoteLiteral = '{q}'
			QuoteExpansion = '{e}'
		""";

}
