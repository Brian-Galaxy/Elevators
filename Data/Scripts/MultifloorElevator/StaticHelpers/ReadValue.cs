namespace Vicizlat.MultifloorElevator
{
    public static class ReadValue
    {
        public static int GetNumberInt(string text, string searchString, int defaultValue = 0, string startPosition = "[", string endPosition = "]", int startPositionOffset = 1)
        {
            if (text == null || !text.Contains(searchString)) return defaultValue;
            int[] valuePosAndLength = ValuePosAndLength(text, searchString, startPosition, endPosition, startPositionOffset);
            int parsedValue;
            if (!int.TryParse(text.Substring(valuePosAndLength[0], valuePosAndLength[1]), out parsedValue)) parsedValue = defaultValue;
            return parsedValue;
        }

        public static long GetNumberLong(string text, string searchString, long defaultValue = 0, string startPosition = "[", string endPosition = "]", int startPositionOffset = 1)
        {
            if (text == null || !text.Contains(searchString)) return defaultValue;
            int[] valuePosAndLength = ValuePosAndLength(text, searchString, startPosition, endPosition, startPositionOffset);
            long parsedValue;
            if (!long.TryParse(text.Substring(valuePosAndLength[0], valuePosAndLength[1]), out parsedValue)) parsedValue = defaultValue;
            return parsedValue;
        }

        public static float GetNumberFloat(string text, string searchString, float defaultValue = 0, string startPosition = "[", string endPosition = "]", int startPositionOffset = 1)
        {
            if (text == null || !text.Contains(searchString)) return defaultValue;
            int[] valuePosAndLength = ValuePosAndLength(text, searchString, startPosition, endPosition, startPositionOffset);
            float parsedValue;
            if (!float.TryParse(text.Substring(valuePosAndLength[0], valuePosAndLength[1]), out parsedValue)) parsedValue = defaultValue;
            return parsedValue;
        }

        public static bool GetBool(string text, string searchString, bool defaultValue = false, string startPosition = "[", string endPosition = "]", int startPositionOffset = 1)
        {
            if (text == null || !text.Contains(searchString)) return defaultValue;
            int[] valuePosAndLength = ValuePosAndLength(text, searchString, startPosition, endPosition, startPositionOffset);
            bool parsedValue;
            if (!bool.TryParse(text.Substring(valuePosAndLength[0], valuePosAndLength[1]), out parsedValue)) parsedValue = defaultValue;
            return parsedValue;
        }

        private static int[] ValuePosAndLength(string text, string searchString, string startPosition, string endPosition, int startPositionOffset)
        {
            int[] valuePosAndLength = new int[2];
            valuePosAndLength[0] = text.IndexOf(startPosition, text.IndexOf(searchString)) + startPositionOffset;
            valuePosAndLength[1] = text.IndexOf(endPosition, text.IndexOf(searchString)) - valuePosAndLength[0];
            return valuePosAndLength;
        }
    }
}