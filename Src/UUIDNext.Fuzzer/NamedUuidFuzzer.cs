using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace UUIDNext.Fuzzer;

internal class NamedUuidFuzzer
{
    private const int TextMaxLength = 2048;

    public void Fuzz()
    {
        Console.WriteLine("Fuzzing Uuid.NewNameBased(namespaceId, name)");
        Console.WriteLine();

        long count = 0;

        var namespaceId = Uuid.NewRandom();

        foreach (var text in EnumerateAllStrings())
        {
            try
            {
                var uuid = Uuid.NewNameBased(namespaceId, text);
            }
            catch (Exception e)
            {
                LogError(namespaceId, text, e);
                return;
            }

            if (++count % 1_000_000 == 0)
            {
                Console.Write($"\r{count:n0} inputs fuzzed");
            }
        }
    }

    private void LogError(Guid namespaceId, string text, Exception e)
    {
        Console.WriteLine($"Error for namespaceId : {namespaceId}, text : '{text}'");
        Console.WriteLine($"Exception : {e}");
        Console.WriteLine();
    }

    private IEnumerable<string> EnumerateAllStrings()
    {
        Rune[] allRunes = GetAllRunes().ToArray();
        for (int length = 1; length < TextMaxLength; length++)
        {
            foreach (var runeStream in EnumerateAllCombinations(allRunes, length))
            {
                StringBuilder sb = new(length);
                foreach (var rune in runeStream)
                    sb.Append(rune);

                yield return sb.ToString();
            }
        }

        IEnumerable<Rune> GetAllRunes()
        {
            const int codeMax = 0xCFFF;
            for (int code = 1; code <= codeMax; code++)
                if (Rune.IsValid(code))
                    yield return new(code);
        }
    }

    private static IEnumerable<T[]> EnumerateAllCombinations<T>(T[] values, int length)
    {
        T[] combination = new T[length];
        var totalCombinations = (long)Math.Pow(values.Length, length);
        for (long combinationIndex = 0; combinationIndex < totalCombinations; combinationIndex++)
        {
            for (int i = 0; i < length; i++)
            {
                var positionSelector = (long)Math.Pow(values.Length, i);
                var valueIndex = (combinationIndex / positionSelector) % values.Length;
                combination[i] = values[valueIndex];
            }

            yield return combination;
        }
    }
}
