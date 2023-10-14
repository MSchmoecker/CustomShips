using System;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Jotunn;

namespace CustomShips {
    public static class CodeMatcherExtensions {
        public static CodeMatcher GetPosition(this CodeMatcher codeMatcher, out int position) {
            position = codeMatcher.Pos;
            return codeMatcher;
        }

        public static CodeMatcher AddLabel(this CodeMatcher codeMatcher, out Label label) {
            label = new Label();
            codeMatcher.AddLabels(new[] { label });
            return codeMatcher;
        }

        public static CodeMatcher GetOperand(this CodeMatcher codeMatcher, out object operand) {
            operand = codeMatcher.Operand;
            return codeMatcher;
        }

        internal static CodeMatcher Print(this CodeMatcher codeMatcher, int before, int after) {
            for (int i = -before; i <= after; ++i) {
                int currentOffset = i;
                int index = codeMatcher.Pos + currentOffset;

                if (index <= 0) {
                    continue;
                }

                if (index >= codeMatcher.Length) {
                    break;
                }

                try {
                    var line = codeMatcher.InstructionAt(currentOffset);
                    Logger.LogInfo($"[{currentOffset}] " + line.ToString());
                } catch (Exception e) {
                    Logger.LogInfo(e.Message);
                }
            }

            return codeMatcher;
        }

        public static bool IsCall(this CodeInstruction i, string declaringType, string name) {
            return (i.opcode == OpCodes.Callvirt || i.opcode == OpCodes.Call) &&
                   i.operand is MethodInfo methodInfo && methodInfo.DeclaringType?.Name == declaringType && methodInfo.Name == name;
        }
    }
}
