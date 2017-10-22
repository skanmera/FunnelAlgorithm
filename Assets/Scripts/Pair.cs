namespace FunnelAlgorithm
{
    public class Pair<T>
    {
        public T Left { get; private set; }
        public T Right { get; private set; }

        public Pair(T left, T right)
        {
            Left = left;
            Right = right;
        }
    }
}
