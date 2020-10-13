namespace PhymarcyAppK204
{
    static class Extensions
    {
        public static bool IsEmpty(string[] arr, string text)
        {
            foreach (var array in arr)
            {
                if (array == text)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
