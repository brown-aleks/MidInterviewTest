using Xunit;

namespace MidInterviewTest;

/*
    Given a string s, 
    find the first non-repeating character in it and return its index. 
    If it does not exist, return -1.
*/
public static class Finder {
    /*
        Сравнительная таблица
        Вариант             |   Время   |   Память  |   Когда использовать
        --------------------------------------------------------------------------------
        1. Dictionary       |   O(n)    |   O(k)    |   Универсальный, Unicode
        2. Массив           |   O(n)    |   O(1)    |   Максимальная скорость, ASCII
        3. LinkedHashMap    |   O(n)    |   O(k)    |   Если нужен streaming
        4. Наивный          |   O(n²)   |   O(1)    |   Только для очень коротких строк

        Рекомендация для собеседования: Вариант 1 — баланс читаемости и эффективности.
        Вариант 2 — если хочешь показать знание низкоуровневых оптимизаций.
    */

    // Вариант 1: Два прохода с Dictionary
    // Сложность: O(n) по времени, O(k) по памяти, где k — размер алфавита (≤ n)
    public static int FirstUniqCharV1(string s)
    {
        var counts = new Dictionary<char, int>();
    
        // Первый проход: подсчёт вхождений
        foreach (var c in s)
            counts[c] = counts.GetValueOrDefault(c) + 1;
        
        // Второй проход: поиск первого уникального
        for (int i = 0; i < s.Length; i++)
            if (counts[s[i]] == 1)
                return i;
        
        return -1;
    }

    //Вариант 2: Массив вместо Dictionary (для ASCII/латиницы)
    // Сложность: O(n) по времени, O(1) по памяти (фиксированный массив 128 элементов)
    // Самый быстрый на практике из-за отсутствия накладных расходов хеширования.
    public static int FirstUniqCharV2(string s)
    {
        var counts = new int[128]; // Для ASCII

        // Первый проход: подсчёт вхождений
        foreach (var c in s)
            counts[c]++;
        
        // Второй проход: поиск первого уникального
        for (int i = 0; i < s.Length; i++)
            if (counts[s[i]] == 1)
                return i;
        
        return -1;
    }

    // Вариант 3: LinkedHashMap-подход (сохранение порядка)
    // Сложность: O(n) по времени, O(k) по памяти
    // Менее эффективен на практике из-за Min().
    public static int FirstUniqCharV3(string s)
    {
        var charIndex = new Dictionary<char, int>();  // символ -> первый индекс
        var duplicates = new HashSet<char>();
        
        for (int i = 0; i < s.Length; i++) {
            if (duplicates.Contains(s[i]))
                continue;
            if (charIndex.ContainsKey(s[i])) {
                charIndex.Remove(s[i]);
                duplicates.Add(s[i]);
            } else {
                charIndex[s[i]] = i;
            }
        }
        
        return charIndex.Count > 0 ? charIndex.Values.Min() : -1;
    }

    // Вариант 4: Наивный с вложенным циклом
    // Сложность: O(n²) по времени, O(1) по памяти
    // Не требует дополнительной памяти, но крайне медленный на длинных строках.
    public static int FirstUniqChar(string s)
    {
        for (int i = 0; i < s.Length; i++) {
            bool unique = true;
            for (int j = 0; j < s.Length; j++) {
                if (i != j && s[i] == s[j]) {
                    unique = false;
                    break;
                }
            }
            if (unique) return i;
        }
        return -1;
    }
}


public class TestFinder {
    [Theory]
    [InlineData("works", 0)]
    [InlineData("soundscool", 2)]
    [InlineData("zzbb", -1)]
    public void Test(string s, int expected) {
        var actual = Finder.FirstUniqCharV1(s);
        Assert.Equal(expected, actual);
    }
}