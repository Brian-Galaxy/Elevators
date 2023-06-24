using System;
using VRage.Game.ModAPI;
using VRageMath;

namespace Vicizlat.MultifloorElevator
{
    public static class SetDigit
    {
        public static void UpdateFloorDisplay(IMyCubeBlock block, string prefix, int actualFloor, Color color, int floorOffset, int floorsCount = 9)
        {
            if (actualFloor <= floorsCount)
            {
                if (actualFloor + floorOffset < 10)
                {
                    CheckDigit(block, $"{prefix}_1", Math.Abs(actualFloor + floorOffset), color, 1f, true);
                    if (actualFloor + floorOffset < 0) CheckDigit(block, $"{prefix}_2", -1, color, 1f, true);
                    else CheckDigit(block, $"{prefix}_2", 10, color, 0f, true);
                }
                else
                {
                    CheckDigit(block, $"{prefix}_1", (actualFloor + floorOffset) % 10, color, 1f, true);
                    CheckDigit(block, $"{prefix}_2", (actualFloor + floorOffset) / 10, color, 1f, true);
                }
            }
            else
            {
                CheckDigit(block, $"{prefix}_1", 10, color, 0f, true);
                CheckDigit(block, $"{prefix}_2", 10, color, 0f, true);
            }
        }

        private static void CheckDigit(IMyCubeBlock block, string prefix, int digit, Color color, float emissivity, bool isBlock)
        {
            string blacks = digit < 0 ? "1 2 3 5 6 7" : Blacks(digit);
            bool isBlack;
            for (int i = 1; i <= 7; i++)
            {
                isBlack = blacks.Contains($"{i}");
                if (isBlock) block.SetEmissiveParts($"{prefix}{i}", isBlack ? Color.Black : color, isBlack ? 0f : emissivity);
                else block.SetEmissivePartsForSubparts($"{prefix}{i}", isBlack ? Color.Black : color, isBlack ? 0f : emissivity);
            }
        }

        private static string Blacks(int digit)
        {
            switch (digit)
            {
                case 0: return "4";
                case 1: return "1 2 3 4 5";
                case 2: return "1 7";
                case 3: return "1 2";
                case 4: return "2 3 5";
                case 5: return "2 6";
                case 6: return "6";
                case 7: return "1 2 4 5";
                case 8: return string.Empty;
                case 9: return "2";
                default: return "1 2 3 4 5 6 7";
            }
        }
    }
}