using Xunit;

namespace MidInterviewTest;

/*
    Вам дана двумерная матрица n x n, представляющая изображение.
    Поверните изображение на 90 градусов (по часовой стрелке).
    
    Extra:
    Вам необходимо повернуть матрицу на месте, что означает, что вы
    должны изменить входную двумерную матрицу напрямую.
    НЕ СОЗДАВАЙТЕ новую двумерную матрицу и не выполняйте поворот сразу.
    
    Input: matrix = [[1,2,3],[4,5,6],[7,8,9]]
    Output: [[7,4,1],[8,5,2],[9,6,3]]
    
    1 2 3      7 4 1
    4 5 6  ->  8 5 2
    7 8 9      9 6 3
*/

/*
    Классическое решение для поворота матрицы на месте — это транспонирование + отражение по вертикали.

    Сложность
    Метрика |   Значение
    --------|---------------
    Время   |   O(n²)
    Память  |   O(1) — in-place
*/
public static class MatrixRotator
{
    public static int[][] Rotate(int[][] matrix)
    {
        int n = matrix.Length;
    
        // Шаг 1: Транспонирование (отражение по главной диагонали)
        // Меняем matrix[i][j] <-> matrix[j][i]
        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                (matrix[i][j], matrix[j][i]) = (matrix[j][i], matrix[i][j]);
            }
        }
        
        // Шаг 2: Отражение по вертикали (реверс каждой строки)
        for (int i = 0; i < n; i++)
        {
            Array.Reverse(matrix[i]);
        }
        
        return matrix;
    }
}

public class TestMatrixRotator
{
    [Theory]
    [InlineData(
         1, 2, 3, 4, 5, 6, 7, 8, 9,
         7, 4, 1, 8, 5, 2, 9, 6, 3 
    )]
    public void Test(params int[] input)
    {
        var originalMatrix = new int[3][];
        for (int i = 0; i < 3; i++)
        {
            originalMatrix[i] = new int[3];
            for (int j = 0; j < 3; j++)
            {
                originalMatrix[i][j] = input[i * 3 + j];
            }
        }
        var expectedMatrix = new int[3][];
        for (int i = 0; i < 3; i++)
        {
            expectedMatrix[i] = new int[3];
            for (int j = 0; j < 3; j++)
            {
                expectedMatrix[i][j] = input[i * 3 + j + 9];
            }
        }
        var actualMatrix = MatrixRotator.Rotate(originalMatrix);
        Assert.Equal(expectedMatrix, actualMatrix);
    }
}

/*
    Bonus:
    
    Дополнение к имеющемуся рещению - Если из условия убрать требование именно
    "изменить входную двумерную матрицу напрямую", то лучшим решением будет
    изменением её представления. С ленивым доступом к элементам матрицы.
*/
/*
    // Вариант использования
    
    var matrix = new int[][]
    {
        new[] {1, 2, 3},
        new[] {4, 5, 6},
        new[] {7, 8, 9}
    };

    var rotatable = new RotatableMatrix(matrix);

    rotatable.RotateClockwise(); // O(1)!

    Console.WriteLine(rotatable[0, 0]); // 7
    Console.WriteLine(rotatable[0, 1]); // 4
    Console.WriteLine(rotatable[0, 2]); // 1

    var rotatable = new RotatableMatrix(matrix);

    rotatable.RotateClockwise(); // O(1)!

    Console.WriteLine(rotatable[0, 0]); // 7
    Console.WriteLine(rotatable[0, 1]); // 4
    Console.WriteLine(rotatable[0, 2]); // 1
*/

// Класс - обёртка над матрицей с возможностью поворота, без реального изменения данных.
// Который запоминает количество поворотов и при доступе к элементам
// вычисляет реальные координаты с учётом поворота.
public class RotatableMatrix
{
    private readonly int[][] _data;
    private readonly int _n;
    private int _rotation; // 0, 1, 2, 3 (количество поворотов по 90°)

    public RotatableMatrix(int[][] matrix)
    {
        _data = matrix;
        _n = matrix.Length;
        _rotation = 0;
    }

    public int N => _n;

    // Поворот на 90° по часовой — просто инкремент счётчика: O(1)
    public void RotateClockwise() => _rotation = (_rotation + 1) % 4;

    // Поворот против часовой
    public void RotateCounterClockwise() => _rotation = (_rotation + 3) % 4;

    public int this[int i, int j]
    {
        get
        {
            var (ri, rj) = MapCoordinates(i, j);
            return _data[ri][rj];
        }
        set
        {
            var (ri, rj) = MapCoordinates(i, j);
            _data[ri][rj] = value;
        }
    }

    private (int i, int j) MapCoordinates(int i, int j)
    {
        return _rotation switch
        {
            0 => (i, j),                     // без поворота
            1 => (N - 1 - j, i),             // 90° по часовой
            2 => (N - 1 - i, N - 1 - j),     // 180°
            3 => (j, N - 1 - i),             // 270° (или 90° против)
            _ => (i, j)
        };
    }

    // Материализация — если всё-таки нужна реальная матрица
    public int[][] Materialize()
    {
        var result = new int[_n][];
        for (int i = 0; i < _n; i++)
        {
            result[i] = new int[_n];
            for (int j = 0; j < _n; j++)
            {
                result[i][j] = this[i, j];
            }
        }
        return result;
    }
}