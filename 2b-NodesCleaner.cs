using Xunit;
using Xunit.Abstractions;

namespace MidInterviewTest;

/*
    Задача состоит в удалении из дерева объектов всех узлов,
    у которых свойство Alive имеет значение false. Однако, если у узла
    свойство Alive имеет значение true, все его родители
    и потомки должны остаться в дереве.

    Extra:
    Выполните задание, используя рекурсивные и итеративные алгоритмы,
    объясните, в каких случаях следует использовать каждый из них, а также
    преимущества и недостатки каждого подхода.
*/

public record Node(int Id, int? ParentId, bool Alive, List<Node> Children);

public static class NodesCleaner
{
    /*
        Оптимальный подход — два прохода:
        Снизу вверх: помечаем, какие узлы нужно сохранить (у них или потомков есть Alive = true)
        Сверху вниз: фильтруем детей, оставляя только помеченные

        Принцип работы рекурсивного алгоритма:
        Рекурсия идёт в глубину (post-order traversal)
        Когда возвращаемся из рекурсии, уже знаем, есть ли живые потомки
        Если node.Alive = true — сохраняем узел и всех его потомков (они уже обработаны, но не удалены, потому что родитель жив)
        Если node.Alive = false, но есть живой потомок — цепочка предков сохраняется через возврат true
    */
    // Рекурсивный алгоритм очистки дерева
    public static void Clean(Node root)
    {
        MarkAndSweep(root);
    }

    // Возвращает true, если этот узел или любой из его потомков Alive
    private static bool MarkAndSweep(Node node)
    {
        // Рекурсивно обрабатываем детей и фильтруем
        // Оставляем только тех, у кого есть живые потомки (или они сами живые)
        var childrenToKeep = new List<Node>();
        
        foreach (var child in node.Children)
        {
            if (MarkAndSweep(child))
            {
                childrenToKeep.Add(child);
            }
        }
        
        node.Children.Clear();
        node.Children.AddRange(childrenToKeep);
        
        // Этот узел сохраняем, если он сам Alive ИЛИ у него остались дети
        return node.Alive || childrenToKeep.Count > 0;
    }

    // Итеративный алгоритм очистки дерева
    public static void CleanIterative(Node root)
    {
        var stack = new Stack<(Node node, bool processed)>();
        var keepNode = new Dictionary<Node, bool>();
        
        stack.Push((root, false));
        
        while (stack.Count > 0)
        {
            var (node, processed) = stack.Pop();
            
            if (!processed)
            {
                stack.Push((node, true)); // вернёмся после обработки детей
                foreach (var child in node.Children)
                    stack.Push((child, false));
            }
            else
            {
                // Post-order: все дети уже обработаны
                var childrenToKeep = node.Children
                    .Where(c => keepNode.GetValueOrDefault(c))
                    .ToList();
                
                node.Children.Clear();
                node.Children.AddRange(childrenToKeep);
                
                keepNode[node] = node.Alive || childrenToKeep.Count > 0;
            }
        }
    }
}

public class Tests
{
    [Fact]
    public void Test1()
    {
        var root = new Node(1, null, true, new List<Node>());
        var l11 = root.F();
        var l12 = root.T();
        l11.F().T();
        l12.F().T();
        root.T().F();
        root.F().T();
        //visualize tree
        //Print(root);
        NodesCleaner.Clean(root);
        var expected = """
            T1
            -F11
            --F111
            ---T1111
            -T12
            --F121
            ---T1211
            -T13
            --F131
            -F14
            --T141
            """.Trim();
        Assert.Equal(expected, Stringify(root).Trim());
    }

    [Fact]
    public void Test2()
    {
        var root = new Node(1, null, false, new List<Node>());
        root.F().F().F();
        root.F().T().F();
        //visualize tree
        //Print(root);
        NodesCleaner.Clean(root);
        var expected = """
            F1
            -F12
            --T121
            ---F1211
            """.Trim();
        Assert.Equal(expected, Stringify(root).Trim());
    }

    [Fact]
    public void Test3()
    {
        var root = new Node(1, null, false, new List<Node>());
        root.F().F().T();
        root.F().T().T();
        root.T().T().T();
        root.F().T().T();
        root.F().F().T();
        root.F().F().F();
        var l11 = root.F();
        l11.F();
        l11.T();
        l11.F().F();
        l11.F().T();
        l11.T().F();
        l11.T().T();

        var l12 = root.T();
        l12.F();
        l12.T();
        l12.F().F();
        l12.F().T();
        l12.T().F();
        l12.T().T();

        //visualize tree
        //Print(root);
        NodesCleaner.Clean(root);
        var expected = """
            F1
            -F11
            --F111
            ---T1111
            -F12
            --T121
            ---T1211
            -T13
            --T131
            ---T1311
            -F14
            --T141
            ---T1411
            -F15
            --F151
            ---T1511
            -F17
            --T172
            --F174
            ---T1741
            --T175
            ---F1751
            --T176
            ---T1761
            -T18
            --F181
            --T182
            --F183
            ---F1831
            --F184
            ---T1841
            --T185
            ---F1851
            --T186
            ---T1861
            """.Trim();

        Assert.Equal(expected, Stringify(root).Trim());
    }

    private readonly ITestOutputHelper _output;

    public Tests(ITestOutputHelper output)
    {
        _output = output;
    }

    private string Stringify(Node node, int level = 0)
    {
        return $"{new string('-', level)}{node.Alive.ToString()[0]}{node.Id}"
            + Environment.NewLine
            + string.Join("", node.Children.Select(child => Stringify(child, level + 1)));
        /*return $"{new string(' ', level * 2)}Id:{node.Id} pId{(node.ParentId.HasValue ? node.ParentId : "R")}:{node.Alive}"
            + Environment.NewLine
            + string.Join("", node.Children.Select(child => Stringify(child, level + 1)));*/
    }

    private void Print(Node node)
    {
        _output.WriteLine(Stringify(node));
    }
}

public static class NodeUtils
{
    public static Node T(this Node parent) => CreateChild(parent, true);

    public static Node F(this Node parent) => CreateChild(parent, false);

    public static Node CreateChild(this Node parent, bool alive)
    {
        var child = new Node(
            (parent.Children.Count > 0 ? parent.Children[^1].Id : parent.Id * 10) + 1,
            parent.Id,
            alive,
            new List<Node>()
        );

        parent.Children.Add(child);
        return child;
    }
}
