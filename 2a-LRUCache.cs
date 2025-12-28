using Xunit;

namespace MidInterviewTest;

/*
    Реализуйте класс кэша наименее часто используемых ключей (LRU):

    LRUCache(int capacity) Инициализирует кэш LRU положительным значением capacity.
    int get(int key) Возвращает значение ключа, если ключ существует, в противном случае возвращает -1.
    void put(int key, int value) Обновляет значение ключа, если ключ существует.
    В противном случае добавляет пару ключ-значение в кэш. Если количество ключей превышает
    емкость, полученную в результате этой операции, удаляет наименее часто используемый ключ.
    
    Дополнительно:
    Функции get и put должны выполняться в среднем за время O(1).
    
    LRUCache lRUCache = new LRUCache(2);
    lRUCache.put(1, 1); // cache is {1=1}
    lRUCache.put(2, 2); // cache is {1=1, 2=2}
    lRUCache.get(1);    // return 1
    lRUCache.put(3, 3); // LRU key was 2, evicts key 2, cache is {1=1, 3=3}
    lRUCache.get(2);    // returns -1 (not found)
    lRUCache.put(4, 4); // LRU key was 1, evicts key 1, cache is {4=4, 3=3}
    lRUCache.get(1);    // return -1 (not found)
    lRUCache.get(3);    // return 3
    lRUCache.get(4);    // return 4
*/

/*
    Сравнительная таблица
    Вариант                     |   Get     |   Put     |   Сложность   |   Когда использовать
                                |           |           |   реализации  |
    ----------------------------|-----------|-----------|---------------|------------------------------
    1.Dictionary + LinkedList   |   O(1)    |   O(1)    |   Средняя     |   Продакшен, собеседования
    2.Свой двусвязный список    |   O(1)    |   O(1)    |   Выше        |   Когда просят без стандартных коллекций
    3.OrderedDictionary         |   O(n)    |   O(n)    |   Низкая      |   Прототипы, малый capacity

    Для собеседования рекомендую вариант 1 — он показывает понимание структур
    данных и при этом использует стандартные коллекции .NET без изобретения велосипедов.
*/

// Вариант 1: Dictionary + LinkedList (оптимальный)
// Классическое решение с O(1) для обеих операций.
// Сложность: Операция/ВремяПамятьGetO(1)—PutO(1)—Общая память—O(capacity)
/*
    //Пример работы:

    var cache = new LRUCache(2);

    cache.Put(1, 100);
    // _cache: { 1 → Node(1,100) }
    // _order: [Node(1,100)]

    cache.Put(2, 200);
    // _cache: { 1 → Node(1,100), 2 → Node(2,200) }
    // _order: [Node(2,200)] ⟷ [Node(1,100)]
    //          ^ newest          ^ oldest (LRU)

    cache.Get(1);  // Обращаемся к ключу 1
    // Находим node через _cache[1] за O(1)
    // Перемещаем в начало _order за O(1)
    // _order: [Node(1,100)] ⟷ [Node(2,200)]
    //          ^ newest          ^ oldest (теперь 2 — LRU)

    cache.Put(3, 300);  // Превышаем capacity
    // Удаляем _order.Last (это Node(2,200)) — LRU
    // _cache: { 1 → Node(1,100), 3 → Node(3,300) }
    // _order: [Node(3,300)] ⟷ [Node(1,100)]
*/
public class LRUCacheV1
{
    private readonly int _capacity;
    private readonly Dictionary<int, LinkedListNode<(int Key, int Value)>> _cache;
    private readonly LinkedList<(int Key, int Value)> _order;

    public LRUCacheV1(int capacity)
    {
        _capacity = capacity;
        _cache = new Dictionary<int, LinkedListNode<(int Key, int Value)>>(capacity);
        _order = new LinkedList<(int Key, int Value)>();
    }

    public int Get(int key)
    {
        if (!_cache.TryGetValue(key, out var node))
            return -1;

        // Перемещаем в начало (самый недавно использованный)
        _order.Remove(node);
        _order.AddFirst(node);
        return node.Value.Value;
    }

    public void Put(int key, int value)
    {
        if (_cache.TryGetValue(key, out var existingNode))
        {
            // Обновляем значение и перемещаем в начало
            _order.Remove(existingNode);
            existingNode.Value = (key, value);
            _order.AddFirst(existingNode);
            return;
        }

        // Вытесняем LRU если превышена ёмкость
        if (_cache.Count >= _capacity)
        {
            var lru = _order.Last!;
            _cache.Remove(lru.Value.Key);
            _order.RemoveLast();
        }

        // Добавляем новый элемент в начало
        var newNode = new LinkedListNode<(int Key, int Value)>((key, value));
        _order.AddFirst(newNode);
        _cache[key] = newNode;
    }
}

// Вариант 2: Собственный двусвязный список
// Тот же подход, но с полным контролем над структурой (иногда требуют на собесах).
// Сложность: идентична варианту 1 — O(1) / O(1).
public class LRUCacheV2
{
    private class Node
    {
        public int Key, Value;
        public Node? Prev, Next;
    }

    private readonly int _capacity;
    private readonly Dictionary<int, Node> _cache;
    private readonly Node _head = new(); // dummy head
    private readonly Node _tail = new(); // dummy tail

    public LRUCacheV2(int capacity)
    {
        _capacity = capacity;
        _cache = new Dictionary<int, Node>(capacity);
        _head.Next = _tail;
        _tail.Prev = _head;
    }

    private void Remove(Node node)
    {
        node.Prev!.Next = node.Next;
        node.Next!.Prev = node.Prev;
    }

    private void AddToFront(Node node)
    {
        node.Next = _head.Next;
        node.Prev = _head;
        _head.Next!.Prev = node;
        _head.Next = node;
    }

    public int Get(int key)
    {
        if (!_cache.TryGetValue(key, out var node))
            return -1;

        Remove(node);
        AddToFront(node);
        return node.Value;
    }

    public void Put(int key, int value)
    {
        if (_cache.TryGetValue(key, out var node))
        {
            node.Value = value;
            Remove(node);
            AddToFront(node);
            return;
        }

        if (_cache.Count >= _capacity)
        {
            var lru = _tail.Prev!;
            Remove(lru);
            _cache.Remove(lru.Key);
        }

        var newNode = new Node { Key = key, Value = value };
        AddToFront(newNode);
        _cache[key] = newNode;
    }
}

// Вариант 3: OrderedDictionary (простой, но O(n))
// Минимум кода, но не удовлетворяет требованию O(1).
using System.Collections.Specialized;

public class LRUCache
{
    private readonly int _capacity;
    private readonly OrderedDictionary _cache;

    public LRUCache(int capacity)
    {
        _capacity = capacity;
        _cache = new OrderedDictionary(capacity);
    }

    public int Get(int key)
    {
        if (!_cache.Contains(key))
            return -1;

        var value = (int)_cache[key]!;
        _cache.Remove(key);      // O(n)
        _cache.Add(key, value);  // O(1)
        return value;
    }

    public void Put(int key, int value)
    {
        if (_cache.Contains(key))
            _cache.Remove(key);  // O(n)
        else if (_cache.Count >= _capacity)
            _cache.RemoveAt(0);  // O(n)

        _cache.Add(key, value);
    }
}

public class TestLruCache
{
    [Theory]
    [InlineData(2,
        1, 1, null,
        2, 2, null,
        1, null, 1,
        3, 3, null,
        2, null, -1,
        4, 4, null,
        1, null, -1,
        3, null, 3,
        4, null, 4
    )]
    public void Test(int capacity, params int?[] input)
    {
        var cache = new LRUCacheV1(capacity);
        for (var i = 0; i < input.Length; i += 3)
        {
            var key = input[i]!.Value;
            var value = input[i + 1];
            var expected = input[i + 2];
            if (value.HasValue)
            {
                cache.Put(key, value.Value);
            }
            else if (expected.HasValue)
            {
                Assert.Equal(expected.Value, cache.Get(key));
            }
            else
            {
                throw new Exception("Invalid input");
            }
        }
    }
}