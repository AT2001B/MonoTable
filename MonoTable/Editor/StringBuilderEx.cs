using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class StringBuilderEx 
{
    public static void AppendLineWithIndent(this StringBuilder sb, int indent, string line)
    {
        for (int i = 0; i < indent; i++)
        {
            sb.Append("    ");
        }
        sb.AppendLine(line);
    }
}
